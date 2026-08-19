namespace Sasd.Pims.Application.Projects;

using Sasd.Pims.Application.Diagnostics;

public sealed class ListProjects(IProjectRepository repository, OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<IReadOnlyList<ProjectSummaryDto>>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Sasd.Pims.Domain.Projects.Project> projects;
        try
        {
            projects = await repository.ListAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            return failureHandler.Report<IReadOnlyList<ProjectSummaryDto>>("ListProjects", exception);
        }

        var summaries = projects
            .Select(project => new ProjectSummaryDto(project.Id, project.Key.Value, project.Name))
            .ToArray();
        return ProjectOperationResult.Success<IReadOnlyList<ProjectSummaryDto>>(summaries);
    }
}
