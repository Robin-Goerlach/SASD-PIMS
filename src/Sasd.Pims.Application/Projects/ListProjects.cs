namespace Sasd.Pims.Application.Projects;

using Sasd.Pims.Application.Diagnostics;

/// <summary>Returns the project-catalog projection with optional filters over implemented 0.1 fields.</summary>
public sealed class ListProjects(IProjectRepository repository, OperationFailureHandler failureHandler)
{
    public Task<ProjectOperationResult<IReadOnlyList<ProjectSummaryDto>>> ExecuteAsync(
        CancellationToken cancellationToken = default) => ExecuteAsync(null, cancellationToken);

    public async Task<ProjectOperationResult<IReadOnlyList<ProjectSummaryDto>>> ExecuteAsync(
        ProjectCatalogFilter? filter,
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

        filter ??= new();
        var summaries = projects
            .Where(project => filter.IncludeArchived || !project.IsArchived)
            .Where(project => Matches(project.Key.Value, filter.SearchText) || Matches(project.Name, filter.SearchText))
            .Where(project => MatchesCode(project.ProjectType, filter.ProjectType))
            .Where(project => MatchesCode(project.ProjectArea, filter.ProjectArea))
            .Where(project => string.IsNullOrWhiteSpace(filter.Tag) || project.Tags.Contains(filter.Tag.Trim(), StringComparer.OrdinalIgnoreCase))
            .Select(project => new ProjectSummaryDto(project.Id, project.Key.Value, project.Name,
                project.ProjectType, project.ProjectArea, project.IsArchived))
            .ToArray();
        return ProjectOperationResult.Success<IReadOnlyList<ProjectSummaryDto>>(summaries);
    }

    private static bool Matches(string value, string? filter) =>
        string.IsNullOrWhiteSpace(filter) || value.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);

    private static bool MatchesCode(string? value, string? filter) =>
        string.IsNullOrWhiteSpace(filter) || string.Equals(value, filter.Trim(), StringComparison.OrdinalIgnoreCase);
}
