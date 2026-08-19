namespace Sasd.Pims.Application.Projects;

public sealed class LoadProject(IProjectRepository repository)
{
    public async Task<ProjectOperationResult<ProjectDto>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var project = await repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
        return project is null
            ? ProjectOperationResult.NotFound<ProjectDto>()
            : ProjectOperationResult.Success(ProjectDto.FromDomain(project));
    }
}
