using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Sasd.Pims.Application.Projects;
using Sasd.Pims.Application.Requirements;
using Sasd.Pims.Domain.Projects;
using Sasd.Pims.Domain.Requirements;
using Sasd.Pims.Infrastructure.Persistence;
using Xunit;

namespace Sasd.Pims.IntegrationTests;

public sealed class SqliteRequirementPersistenceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 20, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Version020MigratesAdditivelyAndPreservesExistingProject()
    {
        await using var database = await TestDatabase.CreateUnmigratedAsync();
        await using (var context = database.Factory.CreateDbContext())
        {
            // Recreate the released 0.2 physical schema so this upgrade test cannot use 0.3 metadata.
            await context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE "__EFMigrationsHistory" ("MigrationId" TEXT NOT NULL PRIMARY KEY, "ProductVersion" TEXT NOT NULL);
                INSERT INTO "__EFMigrationsHistory" VALUES ('202608190001_InitialProject', '10.0.10');
                INSERT INTO "__EFMigrationsHistory" VALUES ('202608200001_ProjectCatalog', '10.0.10');
                INSERT INTO "__EFMigrationsHistory" VALUES ('202608200002_ProjectSteering', '10.0.10');
                CREATE TABLE "Projects" (
                    "Id" TEXT NOT NULL PRIMARY KEY, "Key" TEXT NOT NULL, "Name" TEXT NOT NULL,
                    "ShortDescription" TEXT NULL, "CreatedAtUtc" TEXT NOT NULL, "ModifiedAtUtc" TEXT NOT NULL,
                    "Revision" INTEGER NOT NULL, "Benefit" TEXT NULL, "Goal" TEXT NULL,
                    "IsArchived" INTEGER NOT NULL DEFAULT 0, "ProjectArea" TEXT NULL, "ProjectType" TEXT NULL,
                    "Responsibility" TEXT NULL, "ActivityState" TEXT NOT NULL DEFAULT 'NotStarted',
                    "LastReviewedAtUtc" TEXT NULL, "NextReviewDueAtUtc" TEXT NULL,
                    "Phase" TEXT NOT NULL DEFAULT 'Idea', "TargetDate" TEXT NULL);
                CREATE UNIQUE INDEX "IX_Projects_Key" ON "Projects" ("Key");
                CREATE TABLE "ProjectTags" ("ProjectId" TEXT NOT NULL, "Value" TEXT COLLATE NOCASE NOT NULL,
                    PRIMARY KEY ("ProjectId", "Value"), FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("Id") ON DELETE CASCADE);
                CREATE TABLE "ProjectBlockers" ("Id" TEXT NOT NULL PRIMARY KEY, "ProjectId" TEXT NOT NULL,
                    "Summary" TEXT NOT NULL, "Details" TEXT NULL, "CreatedAtUtc" TEXT NOT NULL,
                    "ResolvedAtUtc" TEXT NULL, "ResolutionNote" TEXT NULL,
                    FOREIGN KEY ("ProjectId") REFERENCES "Projects" ("Id") ON DELETE CASCADE);
                CREATE INDEX "IX_ProjectBlockers_ProjectId_ResolvedAtUtc" ON "ProjectBlockers" ("ProjectId", "ResolvedAtUtc");
                """, TestContext.Current.CancellationToken);
        }
        var project = Project.Create(Guid.NewGuid(), "V020-DATA", "Existing 0.2", "Preserved", Now);
        Assert.Equal(ProjectWriteResult.Saved, await new SqliteProjectRepository(database.Factory)
            .AddAsync(project, TestContext.Current.CancellationToken));

        var backupDirectory = Path.Combine(database.RootPath, "migration-backups");
        await new DatabaseMigrator(database.Factory).MigrateAsync(backupDirectory, "0.3.0",
            TestContext.Current.CancellationToken);

        var reopenedFactory = new PimsDbContextFactory(database.DatabasePath, pooling: false);
        var loaded = await new SqliteProjectRepository(reopenedFactory).GetByIdAsync(project.Id,
            TestContext.Current.CancellationToken);
        await using var reopenedContext = reopenedFactory.CreateDbContext();
        Assert.Equal("Preserved", loaded?.ShortDescription);
        Assert.Contains("202608200003_RequirementsAndTypedReferences",
            await reopenedContext.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken));
        Assert.Single(Directory.GetFiles(backupDirectory, "*.zip"));
    }

    [Fact]
    public async Task RequirementCriteriaAndSharedReferencesSurviveReopen()
    {
        await using var database = await TestDatabase.CreateAsync();
        var project = Project.Create(Guid.NewGuid(), "REQ-ROUNDTRIP", "Requirement roundtrip", null, Now);
        await new SqliteProjectRepository(database.Factory).AddAsync(project, TestContext.Current.CancellationToken);
        var references = new SqliteExternalReferenceRepository(database.Factory);
        var shared = ExternalReference.Create(Guid.NewGuid(), project.Id, null, ExternalReferenceType.Document,
            "Lastenheft", "https://example.test/requirements");
        Assert.Equal(RequirementWriteResult.Saved,
            await references.AddAsync(shared, TestContext.Current.CancellationToken));
        var requirements = new SqliteRequirementRepository(database.Factory);
        var id = Guid.NewGuid();
        var requirement = Requirement.Create(id, project.Id,
            await requirements.GetNextKeyAsync(project.Id, TestContext.Current.CancellationToken),
            "Trace source", "Description", "Rationale", RequirementPriority.Must,
            RequirementDecisionStatus.Proposed, null, RequirementSourceType.Stakeholder,
            new DateOnly(2026, 8, 20), "Workshop", shared.Id,
            [AcceptanceCriterion.Create(Guid.NewGuid(), id, 1, "Source is visible", shared.Id)]);
        Assert.Equal(RequirementWriteResult.Saved,
            await requirements.AddAsync(requirement, TestContext.Current.CancellationToken));

        var reopened = new SqliteRequirementRepository(new PimsDbContextFactory(database.DatabasePath, pooling: false));
        var loaded = await reopened.GetByIdAsync(id, TestContext.Current.CancellationToken);
        Assert.Equal("REQ-001", loaded?.Key);
        Assert.Equal(shared.Id, loaded?.SourceReferenceId);
        Assert.Equal(shared.Id, Assert.Single(loaded!.AcceptanceCriteria).VerificationReferenceId);
    }

    [Fact]
    public async Task KeysAreProjectLocalMonotonicAndUniqueConstraintIsAuthoritative()
    {
        await using var database = await TestDatabase.CreateAsync();
        var projects = new SqliteProjectRepository(database.Factory);
        var firstProject = Project.Create(Guid.NewGuid(), "KEY-A", "A", null, Now);
        var secondProject = Project.Create(Guid.NewGuid(), "KEY-B", "B", null, Now);
        await projects.AddAsync(firstProject, TestContext.Current.CancellationToken);
        await projects.AddAsync(secondProject, TestContext.Current.CancellationToken);
        var repository = new SqliteRequirementRepository(database.Factory);
        var first = CreateRequirement(firstProject.Id, "REQ-001", "First");
        var second = CreateRequirement(firstProject.Id, "REQ-002", "Second");
        var otherProject = CreateRequirement(secondProject.Id, "REQ-001", "Other");
        Assert.Equal(RequirementWriteResult.Saved, await repository.AddAsync(first, TestContext.Current.CancellationToken));
        Assert.Equal(RequirementWriteResult.Saved, await repository.AddAsync(second, TestContext.Current.CancellationToken));
        Assert.Equal(RequirementWriteResult.Saved, await repository.AddAsync(otherProject, TestContext.Current.CancellationToken));
        Assert.Equal("REQ-003", await repository.GetNextKeyAsync(firstProject.Id, TestContext.Current.CancellationToken));
        Assert.Equal("REQ-002", await repository.GetNextKeyAsync(secondProject.Id, TestContext.Current.CancellationToken));
        Assert.Equal(RequirementWriteResult.DuplicateKey,
            await repository.AddAsync(CreateRequirement(firstProject.Id, "REQ-002", "Duplicate"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task StaleRequirementAndReferenceWritesAreObservable()
    {
        await using var database = await TestDatabase.CreateAsync();
        var project = Project.Create(Guid.NewGuid(), "REQ-CONFLICT", "Conflict", null, Now);
        await new SqliteProjectRepository(database.Factory).AddAsync(project, TestContext.Current.CancellationToken);
        var requirements = new SqliteRequirementRepository(database.Factory);
        var requirement = CreateRequirement(project.Id, "REQ-001", "Original");
        await requirements.AddAsync(requirement, TestContext.Current.CancellationToken);
        var first = (await requirements.GetByIdAsync(requirement.Id, TestContext.Current.CancellationToken))!
            .Update("First", null, null, RequirementPriority.Should, RequirementDecisionStatus.Approved, null,
                RequirementSourceType.Internal, null, null, null, []);
        var stale = requirement.Update("Stale", null, null, RequirementPriority.Should,
            RequirementDecisionStatus.Approved, null, RequirementSourceType.Internal, null, null, null, []);
        Assert.Equal(RequirementWriteResult.Saved,
            await requirements.UpdateAsync(first, 1, TestContext.Current.CancellationToken));
        Assert.Equal(RequirementWriteResult.ConcurrencyConflict,
            await requirements.UpdateAsync(stale, 1, TestContext.Current.CancellationToken));

        var references = new SqliteExternalReferenceRepository(database.Factory);
        var reference = ExternalReference.Create(Guid.NewGuid(), project.Id, null, ExternalReferenceType.WebUrl,
            "Original", "https://example.test");
        await references.AddAsync(reference, TestContext.Current.CancellationToken);
        Assert.Equal(RequirementWriteResult.Saved, await references.UpdateAsync(reference.Update(
            ExternalReferenceType.WebUrl, "First", "https://example.test/first"), 1, TestContext.Current.CancellationToken));
        Assert.Equal(RequirementWriteResult.ConcurrencyConflict, await references.UpdateAsync(reference.Update(
            ExternalReferenceType.WebUrl, "Stale", "https://example.test/stale"), 1, TestContext.Current.CancellationToken));
    }

    private static Requirement CreateRequirement(Guid projectId, string key, string title) => Requirement.Create(
        Guid.NewGuid(), projectId, key, title, null, null, RequirementPriority.Should,
        RequirementDecisionStatus.Proposed, null, RequirementSourceType.Internal, null, null, null, []);

    private sealed class TestDatabase : IAsyncDisposable
    {
        private TestDatabase(string rootPath)
        { RootPath = rootPath; DatabasePath = Path.Combine(rootPath, "data", "pims.db"); Factory = new(DatabasePath, pooling: false); }
        public string RootPath { get; }
        public string DatabasePath { get; }
        public PimsDbContextFactory Factory { get; }
        public static Task<TestDatabase> CreateUnmigratedAsync()
        {
            var root = Path.Combine(Path.GetTempPath(), "SASD-PIMS", "tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "data"));
            return Task.FromResult(new TestDatabase(root));
        }
        public static async Task<TestDatabase> CreateAsync()
        {
            var database = await CreateUnmigratedAsync();
            await new DatabaseMigrator(database.Factory).MigrateAsync(cancellationToken: TestContext.Current.CancellationToken);
            return database;
        }
        public ValueTask DisposeAsync()
        { if (Directory.Exists(RootPath)) Directory.Delete(RootPath, true); return ValueTask.CompletedTask; }
    }
}
