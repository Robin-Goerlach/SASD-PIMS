using Sasd.Pims.Domain.Projects;
using Sasd.Pims.Application.Diagnostics;

namespace Sasd.Pims.Application.Projects;

public sealed class CreateProject(
    IProjectRepository repository,
    TimeProvider timeProvider,
    OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<ProjectDto>> ExecuteAsync(
        CreateProjectCommand command,
        CancellationToken cancellationToken = default)
    {
        Project project;
        try
        {
            project = Project.Create(
                Guid.NewGuid(),
                command.Key,
                command.Name,
                command.ShortDescription,
                timeProvider.GetUtcNow());
        }
        catch (DomainValidationException exception)
        {
            return ProjectOperationResult.ValidationFailed<ProjectDto>(
                exception.Errors
                    .Select(error => new ProjectValidationError(error.Field, error.Code, error.Message))
                    .ToArray());
        }

        ProjectWriteResult writeResult;
        try
        {
            writeResult = await repository.AddAsync(project, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            return failureHandler.Report<ProjectDto>("CreateProject", exception);
        }
        return writeResult switch
        {
            ProjectWriteResult.Saved => ProjectOperationResult.Success(ProjectDto.FromDomain(project)),
            ProjectWriteResult.DuplicateKey => ProjectOperationResult.Conflict<ProjectDto>(
                new(nameof(CreateProjectCommand.Key), "ProjectKeyDuplicate", "A project with this key already exists.")),
            ProjectWriteResult.ConcurrencyConflict => ProjectOperationResult.Conflict<ProjectDto>(
                new(string.Empty, "ProjectConcurrencyConflict", "The project was changed by another operation.")),
            _ => throw new InvalidOperationException($"Unsupported write result: {writeResult}."),
        };
    }
}
