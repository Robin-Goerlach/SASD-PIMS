namespace Sasd.Pims.Application.Projects;

using Sasd.Pims.Application.Diagnostics;

/// <summary>Returns project catalog rows with current, derived steering indicators.</summary>
public sealed class ListProjects
{
    private readonly IProjectRepository repository;
    private readonly IProjectBlockerRepository? blockerRepository;
    private readonly TimeProvider timeProvider;
    private readonly OperationFailureHandler failureHandler;

    public ListProjects(IProjectRepository repository, OperationFailureHandler failureHandler)
        : this(repository, null, TimeProvider.System, failureHandler) { }

    public ListProjects(IProjectRepository repository, IProjectBlockerRepository? blockerRepository,
        TimeProvider timeProvider, OperationFailureHandler failureHandler)
    {
        this.repository = repository;
        this.blockerRepository = blockerRepository;
        this.timeProvider = timeProvider;
        this.failureHandler = failureHandler;
    }

    public Task<ProjectOperationResult<IReadOnlyList<ProjectSummaryDto>>> ExecuteAsync(
        CancellationToken cancellationToken = default) => ExecuteAsync(null, cancellationToken);

    public async Task<ProjectOperationResult<IReadOnlyList<ProjectSummaryDto>>> ExecuteAsync(
        ProjectCatalogFilter? filter,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Sasd.Pims.Domain.Projects.Project> projects;
        IReadOnlySet<Guid> projectsWithOpenBlockers;
        try
        {
            projects = await repository.ListAsync(cancellationToken).ConfigureAwait(false);
            projectsWithOpenBlockers = blockerRepository is null
                ? new HashSet<Guid>()
                : await blockerRepository.GetProjectIdsWithOpenBlockersAsync(cancellationToken).ConfigureAwait(false);
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
            .Where(project => filter.Phase is null || project.Phase == filter.Phase)
            .Where(project => filter.ActivityState is null || project.ActivityState == filter.ActivityState)
            .Select(project => CreateSummary(project, projectsWithOpenBlockers.Contains(project.Id)))
            .Where(summary => filter.ReviewFreshness is null || summary.ReviewFreshness == filter.ReviewFreshness)
            .Where(summary => !filter.NeedsAttentionOnly || summary.NeedsAttention)
            .ToArray();
        return ProjectOperationResult.Success<IReadOnlyList<ProjectSummaryDto>>(summaries);
    }

    private static bool Matches(string value, string? filter) =>
        string.IsNullOrWhiteSpace(filter) || value.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase);

    private static bool MatchesCode(string? value, string? filter) =>
        string.IsNullOrWhiteSpace(filter) || string.Equals(value, filter.Trim(), StringComparison.OrdinalIgnoreCase);

    private ProjectSummaryDto CreateSummary(Sasd.Pims.Domain.Projects.Project project, bool hasOpenBlocker)
    {
        var now = timeProvider.GetUtcNow();
        var freshness = Sasd.Pims.Domain.Projects.ProjectSteering.GetReviewFreshness(
            project.LastReviewedAtUtc, project.NextReviewDueAtUtc, now);
        var due = Sasd.Pims.Domain.Projects.ProjectSteering.GetDueDateIndication(
            project.TargetDate, project.ActivityState, DateOnly.FromDateTime(now.UtcDateTime));
        var reasons = Sasd.Pims.Domain.Projects.ProjectSteering.GetAttentionReasons(freshness, due, hasOpenBlocker);
        return new(project.Id, project.Key.Value, project.Name, project.ProjectType, project.ProjectArea,
            project.Phase, project.ActivityState, project.TargetDate, freshness, due, reasons, project.IsArchived);
    }
}
