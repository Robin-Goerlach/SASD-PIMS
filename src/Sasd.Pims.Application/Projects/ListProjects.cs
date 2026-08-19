namespace Sasd.Pims.Application.Projects;

public sealed class ListProjects(IProjectRepository repository)
{
    public async Task<IReadOnlyList<ProjectSummaryDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var projects = await repository.ListAsync(cancellationToken).ConfigureAwait(false);
        return projects
            .Select(project => new ProjectSummaryDto(project.Id, project.Key.Value, project.Name))
            .ToArray();
    }
}
