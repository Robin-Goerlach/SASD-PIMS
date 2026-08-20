using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Sasd.Pims.Application.Search;
using Sasd.Pims.Domain.Projects;
using Sasd.Pims.Domain.Requirements;
using Sasd.Pims.Infrastructure.Persistence;
using Xunit;

namespace Sasd.Pims.IntegrationTests;

public sealed class SearchAndAuditTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 20, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SearchCoversAllObjectTypesAndSupportsProjectScopeSortingAndLimit()
    {
        await using var database = await TestDatabase.CreateAsync();
        var projects = new SqliteProjectRepository(database.Factory);
        var first = Project.Create(Guid.NewGuid(), "ALPHA", "Alpha Search", "needle project",
            "Goal", "Benefit", "TYPE", "AREA", "Owner", ["needle-tag"], Now);
        var second = Project.Create(Guid.NewGuid(), "BETA", "Beta", null, Now);
        await projects.AddAsync(first, TestContext.Current.CancellationToken);
        await projects.AddAsync(second, TestContext.Current.CancellationToken);
        await new SqliteProjectBlockerRepository(database.Factory).AddAsync(ProjectBlocker.Create(Guid.NewGuid(),
            first.Id, "needle blocker", "details", Now), TestContext.Current.CancellationToken);
        var requirement = Requirement.Create(Guid.NewGuid(), first.Id, "REQ-001", "needle requirement", null,
            null, RequirementPriority.Should, RequirementDecisionStatus.Proposed, null,
            RequirementSourceType.Internal, null, null, null, []);
        await new SqliteRequirementRepository(database.Factory).AddAsync(requirement, TestContext.Current.CancellationToken);
        await new SqliteExternalReferenceRepository(database.Factory).AddAsync(ExternalReference.Create(Guid.NewGuid(),
            first.Id, requirement.Id, ExternalReferenceType.WebUrl, "needle reference", "https://example.test"),
            TestContext.Current.CancellationToken);

        var reader = new SqliteSearchReader(database.Factory);
        var page = await reader.SearchAsync(new SearchQuery("needle", first.Id, Sort: SearchSort.ObjectType,
            Limit: 3), TestContext.Current.CancellationToken);
        Assert.Equal(4, page.TotalCount);
        Assert.True(page.HasMore);
        Assert.Equal(3, page.Items.Count);
        Assert.All(page.Items, item => Assert.Equal(first.Id, item.ProjectId));
        Assert.Equal([SearchObjectType.Project, SearchObjectType.Requirement, SearchObjectType.Blocker],
            page.Items.Select(item => item.ObjectType));
        Assert.Empty((await reader.SearchAsync(new SearchQuery("not-present"), TestContext.Current.CancellationToken)).Items);
    }

    [Fact]
    public async Task ChangeEventsAreWhitelistedAndSensitiveValuesAreRedacted()
    {
        await using var database = await TestDatabase.CreateAsync();
        var projects = new SqliteProjectRepository(database.Factory);
        var project = Project.Create(Guid.NewGuid(), "AUDIT", "Audit", null, Now);
        await projects.AddAsync(project, TestContext.Current.CancellationToken);
        project.UpdateDetails("Changed title is not audited", "secret description", Now.AddMinutes(1));
        await projects.UpdateAsync(project, 1, TestContext.Current.CancellationToken);
        project.UpdateSteering(ProjectPhase.Execution, ActivityState.Active, new DateOnly(2026, 9, 1), null,
            null, Now.AddMinutes(2));
        await projects.UpdateAsync(project, 2, TestContext.Current.CancellationToken);

        var requirementRepository = new SqliteRequirementRepository(database.Factory);
        var requirement = Requirement.Create(Guid.NewGuid(), project.Id, "REQ-001", "Requirement", null, null,
            RequirementPriority.Should, RequirementDecisionStatus.Proposed, null, RequirementSourceType.Internal,
            null, null, null, []);
        await requirementRepository.AddAsync(requirement, TestContext.Current.CancellationToken);
        var sensitiveReason = "token=must-never-be-audited";
        await requirementRepository.UpdateAsync(requirement.Update("Requirement", null, null,
            RequirementPriority.Could, RequirementDecisionStatus.Rejected, sensitiveReason,
            RequirementSourceType.Internal, null, null, null, []), 1, TestContext.Current.CancellationToken);

        var referenceRepository = new SqliteExternalReferenceRepository(database.Factory);
        var reference = ExternalReference.Create(Guid.NewGuid(), project.Id, null, ExternalReferenceType.WebUrl,
            "Reference", "https://example.test/old-secret");
        await referenceRepository.AddAsync(reference, TestContext.Current.CancellationToken);
        await referenceRepository.UpdateAsync(reference.Update(ExternalReferenceType.WebUrl, "Renamed",
            "https://example.test/new-secret"), 1, TestContext.Current.CancellationToken);

        var events = await new SqliteChangeEventReader(database.Factory).ListByProjectAsync(project.Id,
            TestContext.Current.CancellationToken);
        Assert.DoesNotContain(events, item => item.EventType is "Name" or "ShortDescription");
        Assert.Contains(events, item => item.EntityType == "Project" && item.EventType == "Phase");
        Assert.Contains(events, item => item.EntityType == "Requirement" && item.EventType == "DecisionReason" &&
            item.NewValue == "Present");
        Assert.Contains(events, item => item.EntityType == "ExternalReference" && item.EventType == "TargetChanged" &&
            item.OldValue is null && item.NewValue is null);
        var serialized = string.Join('|', events.SelectMany(item => new[] { item.OldValue, item.NewValue }));
        Assert.DoesNotContain(sensitiveReason, serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("old-secret", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("new-secret", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MigrationFrom030PreservesRowsAndAddsOnlyAuditAndIndexes()
    {
        await using var database = await TestDatabase.CreateAsync();
        var project = Project.Create(Guid.NewGuid(), "UPGRADE-030", "Preserved 0.3", null, Now);
        await new SqliteProjectRepository(database.Factory).AddAsync(project, TestContext.Current.CancellationToken);
        var requirement = Requirement.Create(Guid.NewGuid(), project.Id, "REQ-001", "Preserved", null, null,
            RequirementPriority.Should, RequirementDecisionStatus.Proposed, null, RequirementSourceType.Internal,
            null, null, null, []);
        await new SqliteRequirementRepository(database.Factory).AddAsync(requirement,
            TestContext.Current.CancellationToken);
        await using (var oldContext = database.Factory.CreateDbContext())
        {
            // Recreate the exact additive 0.3 boundary from the current schema: 0.4 adds only these objects.
            await oldContext.Database.ExecuteSqlRawAsync("""
                DROP TABLE "ChangeEvents";
                DROP INDEX "IX_Projects_Phase_ActivityState";
                DROP INDEX "IX_Projects_ProjectType_ProjectArea";
                DROP INDEX "IX_Requirements_Priority_DecisionStatus_SourceType";
                DROP INDEX "IX_ExternalReferences_Type_ProjectId";
                DELETE FROM "__EFMigrationsHistory" WHERE "MigrationId" = '202608200004_SearchTraceabilityAndExchange';
                """, TestContext.Current.CancellationToken);
        }
        var backups = Path.Combine(database.Root, "migration-backups");
        await new DatabaseMigrator(database.Factory).MigrateAsync(backups, "0.4.0",
            TestContext.Current.CancellationToken);
        await using var context = database.Factory.CreateDbContext();
        var migrations = await context.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken);
        Assert.Contains("202608200004_SearchTraceabilityAndExchange", migrations);
        Assert.True(await context.Database.CanConnectAsync(TestContext.Current.CancellationToken));
        Assert.Empty(await context.ChangeEvents.AsNoTracking().ToListAsync(TestContext.Current.CancellationToken));
        Assert.NotNull(await new SqliteProjectRepository(database.Factory).GetByIdAsync(project.Id,
            TestContext.Current.CancellationToken));
        Assert.NotNull(await new SqliteRequirementRepository(database.Factory).GetByIdAsync(requirement.Id,
            TestContext.Current.CancellationToken));
        Assert.Single(Directory.GetFiles(backups, "*.zip"));
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private TestDatabase(string root)
        {
            Root = root;
            Factory = new PimsDbContextFactory(Path.Combine(root, "pims.db"), pooling: false);
        }

        public string Root { get; }
        public PimsDbContextFactory Factory { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var root = Path.Combine(Path.GetTempPath(), "SASD-PIMS", "search-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var database = new TestDatabase(root);
            await new DatabaseMigrator(database.Factory).MigrateAsync(
                cancellationToken: TestContext.Current.CancellationToken);
            return database;
        }

        public static Task<TestDatabase> CreateUnmigratedAsync()
        {
            var root = Path.Combine(Path.GetTempPath(), "SASD-PIMS", "search-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root); return Task.FromResult(new TestDatabase(root));
        }

        public ValueTask DisposeAsync()
        {
            if (Directory.Exists(Root)) Directory.Delete(Root, true);
            return ValueTask.CompletedTask;
        }
    }
}
