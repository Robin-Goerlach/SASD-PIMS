using System.Text.Json;
using System.Text.Json.Serialization;
using Sasd.Pims.Application.Projects;

namespace Sasd.Pims.Infrastructure.Export;

public sealed class JsonProjectExportWriter : IProjectExportWriter
{
    public const string SchemaVersion = "1.0-internal";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task WriteAsync(
        ProjectDto project,
        string targetPath,
        DateTimeOffset exportedAtUtc,
        string applicationVersion,
        CancellationToken cancellationToken)
    {
        var fullTargetPath = Path.GetFullPath(targetPath);
        var directory = Path.GetDirectoryName(fullTargetPath)
            ?? throw new ArgumentException("Export target must have a parent directory.", nameof(targetPath));
        Directory.CreateDirectory(directory);

        var envelope = new ProjectExportEnvelope(
            SchemaVersion,
            exportedAtUtc,
            applicationVersion,
            new ProjectExportData(
                project.Id,
                project.Key,
                project.Name,
                project.ShortDescription,
                project.CreatedAtUtc,
                project.ModifiedAtUtc,
                project.Revision));
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullTargetPath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                FileOptions.Asynchronous | FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, envelope, SerializerOptions, cancellationToken)
                    .ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            File.Move(temporaryPath, fullTargetPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private sealed record ProjectExportEnvelope(
        string SchemaVersion,
        DateTimeOffset ExportedAtUtc,
        string ApplicationVersion,
        ProjectExportData Project);

    private sealed record ProjectExportData(
        Guid Id,
        string Key,
        string Name,
        string? ShortDescription,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset ModifiedAtUtc,
        int Revision);
}
