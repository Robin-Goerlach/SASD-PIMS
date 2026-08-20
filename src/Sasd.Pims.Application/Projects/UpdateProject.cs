using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Projects;

/// <summary>Updates project master data and reports stale-editor conflicts without overwriting data.</summary>
public sealed class UpdateProject(
    IProjectRepository repository,
    TimeProvider timeProvider,
    OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<ProjectDto>> ExecuteAsync(
        UpdateProjectCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await repository.GetByIdAsync(command.Id, cancellationToken).ConfigureAwait(false);
            if (project is null) return ProjectOperationResult.NotFound<ProjectDto>();
            if (project.Revision != command.ExpectedRevision) return ConcurrencyConflict();

            project.UpdateDetails(command.Name, command.ShortDescription, command.Goal, command.Benefit,
                command.ProjectType, command.ProjectArea, command.Responsibility, command.Tags,
                NextTimestamp(project.ModifiedAtUtc));
            var result = await repository.UpdateAsync(project, command.ExpectedRevision, cancellationToken).ConfigureAwait(false);
            return result == ProjectWriteResult.Saved
                ? ProjectOperationResult.Success(ProjectDto.FromDomain(project))
                : ConcurrencyConflict();
        }
        catch (DomainValidationException exception)
        {
            return ProjectOperationResult.ValidationFailed<ProjectDto>(exception.Errors
                .Select(error => new ProjectValidationError(error.Field, error.Code, error.Message)).ToArray());
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            return failureHandler.Report<ProjectDto>("UpdateProject", exception);
        }
    }

    private DateTimeOffset NextTimestamp(DateTimeOffset previous)
    {
        var now = timeProvider.GetUtcNow();
        return now > previous ? now : previous.AddTicks(1);
    }

    private static ProjectOperationResult<ProjectDto> ConcurrencyConflict() =>
        ProjectOperationResult.Conflict<ProjectDto>(new(string.Empty, "ProjectConcurrencyConflict",
            "The project was changed by another operation. Reload it before saving."));
}
