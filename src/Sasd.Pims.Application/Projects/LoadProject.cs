namespace Sasd.Pims.Application.Projects;

using Sasd.Pims.Application.Diagnostics;

public sealed class LoadProject(IProjectRepository repository, OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<ProjectDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        Sasd.Pims.Domain.Projects.Project? project;
        try
        {
            project = await repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            return failureHandler.Report<ProjectDto>("LoadProject", exception);
        }
        return project is null
            ? ProjectOperationResult.NotFound<ProjectDto>()
            : ProjectOperationResult.Success(ProjectDto.FromDomain(project));
    }
}
