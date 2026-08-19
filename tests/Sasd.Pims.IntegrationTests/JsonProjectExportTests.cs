using System.Text.Json;
using Sasd.Pims.Application.Projects;
using Sasd.Pims.Infrastructure.Export;
using Xunit;

namespace Sasd.Pims.IntegrationTests;

public sealed class JsonProjectExportTests
{
    private static readonly DateTimeOffset ExportedAt = new(2026, 8, 19, 16, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExportContainsVersionStableIdentityAndValidJson()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var target = Path.Combine(root, "project.json");
            var project = CreateProjectDto();
            await new JsonProjectExportWriter().WriteAsync(
                project,
                target,
                ExportedAt,
                "0.0.1-internal",
                TestContext.Current.CancellationToken);

            await using var stream = File.OpenRead(target);
            using var document = await JsonDocument.ParseAsync(
                stream,
                cancellationToken: TestContext.Current.CancellationToken);
            var rootElement = document.RootElement;

            Assert.Equal(JsonProjectExportWriter.SchemaVersion, rootElement.GetProperty("schemaVersion").GetString());
            Assert.Equal(project.Id, rootElement.GetProperty("project").GetProperty("id").GetGuid());
            Assert.Equal(project.Key, rootElement.GetProperty("project").GetProperty("key").GetString());
            Assert.Equal(ExportedAt, rootElement.GetProperty("exportedAtUtc").GetDateTimeOffset());
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task EquivalentInputProducesEquivalentJson()
    {
        var root = CreateTemporaryRoot();
        try
        {
            var first = Path.Combine(root, "first.json");
            var second = Path.Combine(root, "second.json");
            var writer = new JsonProjectExportWriter();
            var project = CreateProjectDto();

            await writer.WriteAsync(project, first, ExportedAt, "0.0.1-internal", TestContext.Current.CancellationToken);
            await writer.WriteAsync(project, second, ExportedAt, "0.0.1-internal", TestContext.Current.CancellationToken);

            Assert.Equal(
                await File.ReadAllTextAsync(first, TestContext.Current.CancellationToken),
                await File.ReadAllTextAsync(second, TestContext.Current.CancellationToken));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static ProjectDto CreateProjectDto() =>
        new(
            Guid.Parse("09d8bcc6-a4f3-4d3c-bbbc-70378dd7061a"),
            "EXPORT-DEMO",
            "Export demo",
            "Synthetic",
            ExportedAt.AddDays(-1),
            ExportedAt.AddHours(-1),
            2);

    private static string CreateTemporaryRoot()
    {
        var path = Path.Combine(Path.GetTempPath(), "SASD-PIMS", "tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
