namespace Sasd.Pims.Application.Projects;

/// <summary>Writes a versioned, portable representation of one complete project.</summary>
public interface IProjectExportWriter
{
    /// <summary>
    /// Writes <paramref name="project"/> to <paramref name="targetPath"/> without exposing persistence details.
    /// Implementations must not leave a partially written target after failure.
    /// </summary>
    Task WriteAsync(
        ProjectDto project,
        string targetPath,
        DateTimeOffset exportedAtUtc,
        string applicationVersion,
        CancellationToken cancellationToken);
}
