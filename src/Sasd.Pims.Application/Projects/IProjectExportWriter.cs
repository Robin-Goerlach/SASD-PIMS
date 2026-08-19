namespace Sasd.Pims.Application.Projects;

public interface IProjectExportWriter
{
    Task WriteAsync(
        ProjectDto project,
        string targetPath,
        DateTimeOffset exportedAtUtc,
        string applicationVersion,
        CancellationToken cancellationToken);
}
