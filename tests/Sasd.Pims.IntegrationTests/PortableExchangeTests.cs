using System.Text.Json;
using Sasd.Pims.Application.Traceability;
using Sasd.Pims.Domain.Projects;
using Sasd.Pims.Domain.Requirements;
using Sasd.Pims.Infrastructure.Export;
using Sasd.Pims.Infrastructure.Persistence;
using Xunit;

namespace Sasd.Pims.IntegrationTests;

public sealed class PortableExchangeTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 20, 15, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task JsonExchangeIsDeterministicCompleteVersionedAndMarksLocalReferences()
    {
        await using var database = await TestDatabase.CreateAsync();
        var project = Project.Create(Guid.NewGuid(), "EXPORT", "Export", "Description", "Goal", "Benefit",
            "TYPE", "AREA", "Owner", ["zeta", "alpha"], Now);
        await new SqliteProjectRepository(database.Factory).AddAsync(project, TestContext.Current.CancellationToken);
        var requirementId = Guid.NewGuid();
        var requirement = Requirement.Create(requirementId, project.Id, "REQ-001", "Requirement", "Body", "Reason",
            RequirementPriority.Must, RequirementDecisionStatus.Proposed, null, RequirementSourceType.Research,
            null, "Source", null, [AcceptanceCriterion.Create(Guid.NewGuid(), requirementId, 1, "Criterion")]);
        await new SqliteRequirementRepository(database.Factory).AddAsync(requirement,
            TestContext.Current.CancellationToken);
        var localPath = Path.Combine(database.Root, "machine-local.txt");
        await new SqliteExternalReferenceRepository(database.Factory).AddAsync(ExternalReference.Create(Guid.NewGuid(),
            project.Id, requirement.Id, ExternalReferenceType.LocalFile, "Local", localPath),
            TestContext.Current.CancellationToken);
        await new SqliteProjectBlockerRepository(database.Factory).AddAsync(ProjectBlocker.Create(Guid.NewGuid(),
            project.Id, "Blocked", null, Now), TestContext.Current.CancellationToken);

        var service = new PortableExchangeService(database.Factory);
        var first = Path.Combine(database.Root, "first.json");
        var second = Path.Combine(database.Root, "second.json");
        await service.ExportJsonAsync(first, "0.4.0", Now, TestContext.Current.CancellationToken);
        await service.ExportJsonAsync(second, "0.4.0", Now, TestContext.Current.CancellationToken);
        await service.ExportJsonAsync(first, "0.4.0", Now, TestContext.Current.CancellationToken);
        Assert.Equal(await File.ReadAllBytesAsync(first, TestContext.Current.CancellationToken),
            await File.ReadAllBytesAsync(second, TestContext.Current.CancellationToken));
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(first,
            TestContext.Current.CancellationToken));
        var root = document.RootElement;
        Assert.Equal("sasd-pims-exchange", root.GetProperty("formatId").GetString());
        Assert.Equal("1.0", root.GetProperty("formatVersion").GetString());
        Assert.Equal(project.Id, root.GetProperty("projects")[0].GetProperty("id").GetGuid());
        Assert.Equal(requirement.Id, root.GetProperty("requirements")[0].GetProperty("id").GetGuid());
        var reference = root.GetProperty("externalReferences")[0];
        Assert.Equal(localPath, reference.GetProperty("target").GetString());
        Assert.Equal("machineLocal", reference.GetProperty("locationScope").GetString());
        Assert.Single(root.GetProperty("changeEvents").EnumerateArray());
        Assert.DoesNotContain("FTS", await File.ReadAllTextAsync(first, TestContext.Current.CancellationToken),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task MarkdownIsDeterministicAndTraceabilityUsesExistingRelationships()
    {
        await using var database = await TestDatabase.CreateAsync();
        var project = Project.Create(Guid.NewGuid(), "TRACE", "Trace", null, Now);
        await new SqliteProjectRepository(database.Factory).AddAsync(project, TestContext.Current.CancellationToken);
        var references = new SqliteExternalReferenceRepository(database.Factory);
        var source = ExternalReference.Create(Guid.NewGuid(), project.Id, null, ExternalReferenceType.Document,
            "Source", "https://example.test/source");
        await references.AddAsync(source, TestContext.Current.CancellationToken);
        var requirementId = Guid.NewGuid();
        var requirement = Requirement.Create(requirementId, project.Id, "REQ-001", "Traceability", null, null,
            RequirementPriority.Must, RequirementDecisionStatus.Approved, null, RequirementSourceType.External,
            null, null, source.Id,
            [AcceptanceCriterion.Create(Guid.NewGuid(), requirementId, 1, "Verified", source.Id)]);
        await new SqliteRequirementRepository(database.Factory).AddAsync(requirement,
            TestContext.Current.CancellationToken);
        var service = new PortableExchangeService(database.Factory);
        var first = Path.Combine(database.Root, "first.md");
        var second = Path.Combine(database.Root, "second.md");
        await service.ExportProjectMarkdownAsync(project.Id, first, Now, TestContext.Current.CancellationToken);
        await service.ExportProjectMarkdownAsync(project.Id, second, Now, TestContext.Current.CancellationToken);
        Assert.Equal(await File.ReadAllTextAsync(first, TestContext.Current.CancellationToken),
            await File.ReadAllTextAsync(second, TestContext.Current.CancellationToken));
        var markdown = await File.ReadAllTextAsync(first, TestContext.Current.CancellationToken);
        Assert.Contains("REQ-001", markdown, StringComparison.Ordinal);
        Assert.Contains("Akzeptanzkriterien", markdown, StringComparison.Ordinal);
        var tree = await new SqliteTraceabilityReader(database.Factory).GetProjectAsync(project.Id,
            TestContext.Current.CancellationToken);
        var requirementNode = Assert.Single(tree!.Children, item => item.Type == TraceabilityNodeType.Requirement);
        Assert.Contains(requirementNode.Children, item => item.Label.StartsWith("Quelle:", StringComparison.Ordinal));
        Assert.Contains(requirementNode.Children.SelectMany(item => item.Children),
            item => item.Label.StartsWith("Nachweis:", StringComparison.Ordinal));
    }

    [Fact]
    public void ExchangeSchemaIsVersionedAndSyntacticallyValid()
    {
        var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..",
            "docs", "exchange", "sasd-pims-exchange-1.0.schema.json"));
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal("SASD PIMS Exchange 1.0", document.RootElement.GetProperty("title").GetString());
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private TestDatabase(string root) { Root = root; Factory = new(Path.Combine(root, "pims.db"), pooling: false); }
        public string Root { get; }
        public PimsDbContextFactory Factory { get; }
        public static async Task<TestDatabase> CreateAsync()
        {
            var root = Path.Combine(Path.GetTempPath(), "SASD-PIMS", "exchange-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            var value = new TestDatabase(root);
            await new DatabaseMigrator(value.Factory).MigrateAsync(cancellationToken: TestContext.Current.CancellationToken);
            return value;
        }
        public ValueTask DisposeAsync() { if (Directory.Exists(Root)) Directory.Delete(Root, true); return ValueTask.CompletedTask; }
    }
}
