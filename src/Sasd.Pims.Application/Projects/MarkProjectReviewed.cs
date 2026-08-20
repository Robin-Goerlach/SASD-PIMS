using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Projects;

/// <summary>Records a review at the current UTC time and optionally schedules the next review.</summary>
public sealed class MarkProjectReviewed(IProjectRepository repository, TimeProvider timeProvider,
    OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<ProjectDto>> ExecuteAsync(Guid id, int expectedRevision,
        DateTimeOffset? nextReviewDueAtUtc, CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (project is null) return ProjectOperationResult.NotFound<ProjectDto>();
            if (project.Revision != expectedRevision) return Conflict();
            var now = timeProvider.GetUtcNow();
            var modified = now > project.ModifiedAtUtc ? now : project.ModifiedAtUtc.AddTicks(1);
            project.MarkReviewed(now, nextReviewDueAtUtc, modified);
            var saved = await repository.UpdateAsync(project, expectedRevision, cancellationToken).ConfigureAwait(false);
            return saved == ProjectWriteResult.Saved ? ProjectOperationResult.Success(ProjectDto.FromDomain(project)) : Conflict();
        }
        catch (DomainValidationException exception)
        {
            return ProjectOperationResult.ValidationFailed<ProjectDto>(exception.Errors
                .Select(error => new ProjectValidationError(error.Field, error.Code, error.Message)).ToArray());
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            return failureHandler.Report<ProjectDto>("MarkProjectReviewed", exception);
        }
    }

    private static ProjectOperationResult<ProjectDto> Conflict() => ProjectOperationResult.Conflict<ProjectDto>(
        new(string.Empty, "ProjectConcurrencyConflict", "The project was changed. Reload it before recording a review."));
}
