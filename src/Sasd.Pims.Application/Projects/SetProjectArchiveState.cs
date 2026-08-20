using Sasd.Pims.Application.Diagnostics;

namespace Sasd.Pims.Application.Projects;

/// <summary>Archives or reactivates a project using optimistic concurrency and never deletes it.</summary>
public sealed class SetProjectArchiveState(
    IProjectRepository repository,
    TimeProvider timeProvider,
    OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<ProjectDto>> ExecuteAsync(
        Guid id,
        int expectedRevision,
        bool archive,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (project is null) return ProjectOperationResult.NotFound<ProjectDto>();
            if (project.Revision != expectedRevision) return Conflict();

            var now = timeProvider.GetUtcNow();
            var modifiedAt = now > project.ModifiedAtUtc ? now : project.ModifiedAtUtc.AddTicks(1);
            if (archive) project.Archive(modifiedAt); else project.Reactivate(modifiedAt);
            var result = await repository.UpdateAsync(project, expectedRevision, cancellationToken).ConfigureAwait(false);
            return result == ProjectWriteResult.Saved
                ? ProjectOperationResult.Success(ProjectDto.FromDomain(project))
                : Conflict();
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            return failureHandler.Report<ProjectDto>("SetProjectArchiveState", exception);
        }
    }

    private static ProjectOperationResult<ProjectDto> Conflict() =>
        ProjectOperationResult.Conflict<ProjectDto>(new(string.Empty, "ProjectConcurrencyConflict",
            "The project was changed by another operation. Reload it before changing its archive state."));
}
