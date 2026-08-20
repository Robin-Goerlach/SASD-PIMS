using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Projects;

/// <summary>Updates phase, activity, target and review schedule without coupling their meanings.</summary>
public sealed class UpdateProjectSteering(IProjectRepository repository, TimeProvider timeProvider,
    OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<ProjectDto>> ExecuteAsync(UpdateProjectSteeringCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await repository.GetByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
            if (project is null) return ProjectOperationResult.NotFound<ProjectDto>();
            if (project.Revision != command.ExpectedRevision) return Conflict();
            project.UpdateSteering(command.Phase, command.ActivityState, command.TargetDate,
                project.LastReviewedAtUtc, command.NextReviewDueAtUtc, NextTimestamp(project.ModifiedAtUtc));
            var saved = await repository.UpdateAsync(project, command.ExpectedRevision, cancellationToken).ConfigureAwait(false);
            return saved == ProjectWriteResult.Saved ? ProjectOperationResult.Success(ProjectDto.FromDomain(project)) : Conflict();
        }
        catch (DomainValidationException exception)
        {
            return ProjectOperationResult.ValidationFailed<ProjectDto>(exception.Errors
                .Select(error => new ProjectValidationError(error.Field, error.Code, error.Message)).ToArray());
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            return failureHandler.Report<ProjectDto>("UpdateProjectSteering", exception);
        }
    }

    private DateTimeOffset NextTimestamp(DateTimeOffset previous)
    {
        var now = timeProvider.GetUtcNow();
        return now > previous ? now : previous.AddTicks(1);
    }

    private static ProjectOperationResult<ProjectDto> Conflict() => ProjectOperationResult.Conflict<ProjectDto>(
        new(string.Empty, "ProjectConcurrencyConflict", "The project was changed. Reload it before saving steering data."));
}
