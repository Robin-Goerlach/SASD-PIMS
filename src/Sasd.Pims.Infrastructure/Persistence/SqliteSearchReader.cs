using Microsoft.EntityFrameworkCore;
using Sasd.Pims.Application.Search;

namespace Sasd.Pims.Infrastructure.Persistence;

/// <summary>Executes bounded, server-side SQLite searches without materializing domain aggregates.</summary>
public sealed class SqliteSearchReader(IDbContextFactory<PimsDbContext> contextFactory) : ISearchReader
{
    public async Task<SearchPage> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var text = query.Text;
        var projects = context.Projects.AsNoTracking();
        if (query.ProjectId is not null) projects = projects.Where(item => item.Id == query.ProjectId);

        var rows = new List<SearchRow>();
        var total = 0;
        var requirementOnlyFilter = query.RequirementPriority is not null || query.DecisionStatus is not null ||
            query.SourceType is not null;
        var referenceOnlyFilter = query.ReferenceType is not null;
        if (!requirementOnlyFilter && !referenceOnlyFilter && query.ObjectType is null or SearchObjectType.Project)
        {
            var source = projects;
            if (text is not null)
                source = source.Where(item => item.Key.Contains(text) || item.Name.Contains(text) ||
                    (item.ShortDescription != null && item.ShortDescription.Contains(text)) ||
                    (item.Goal != null && item.Goal.Contains(text)) ||
                    (item.Benefit != null && item.Benefit.Contains(text)) ||
                    (item.Responsibility != null && item.Responsibility.Contains(text)) ||
                    item.Tags.Any(tag => tag.Value.Contains(text)));
            total += await source.CountAsync(cancellationToken).ConfigureAwait(false);
            rows.AddRange(await source.Take(query.Offset + query.Limit)
                .Select(item => new SearchRow(SearchObjectType.Project, item.Id, item.Id, item.Key,
                    item.Name, item.Key, item.Name, "Treffer in Projektstammdaten oder Tags"))
                .ToListAsync(cancellationToken).ConfigureAwait(false));
        }

        if (!referenceOnlyFilter && query.ObjectType is null or SearchObjectType.Requirement)
        {
            var source = context.Requirements.AsNoTracking();
            if (query.ProjectId is not null) source = source.Where(item => item.ProjectId == query.ProjectId);
            if (query.RequirementPriority is not null) source = source.Where(item => item.Priority == query.RequirementPriority);
            if (query.DecisionStatus is not null) source = source.Where(item => item.DecisionStatus == query.DecisionStatus);
            if (query.SourceType is not null) source = source.Where(item => item.SourceType == query.SourceType);
            if (text is not null)
                source = source.Where(item => item.Key.Contains(text) || item.Title.Contains(text) ||
                    (item.Description != null && item.Description.Contains(text)) ||
                    (item.Rationale != null && item.Rationale.Contains(text)) ||
                    (item.SourceSummary != null && item.SourceSummary.Contains(text)));
            total += await source.CountAsync(cancellationToken).ConfigureAwait(false);
            rows.AddRange(await source.Take(query.Offset + query.Limit)
                .Select(item => new SearchRow(SearchObjectType.Requirement, item.Id, item.ProjectId,
                    item.Project.Key, item.Project.Name, item.Key, item.Title, "Treffer in Anforderungsdaten"))
                .ToListAsync(cancellationToken).ConfigureAwait(false));
        }

        if (!requirementOnlyFilter && !referenceOnlyFilter && query.ObjectType is null or SearchObjectType.Blocker)
        {
            var source = context.ProjectBlockers.AsNoTracking();
            if (query.ProjectId is not null) source = source.Where(item => item.ProjectId == query.ProjectId);
            if (text is not null)
                source = source.Where(item => item.Summary.Contains(text) ||
                    (item.Details != null && item.Details.Contains(text)) ||
                    (item.ResolutionNote != null && item.ResolutionNote.Contains(text)));
            total += await source.CountAsync(cancellationToken).ConfigureAwait(false);
            rows.AddRange(await source.Take(query.Offset + query.Limit)
                .Select(item => new SearchRow(SearchObjectType.Blocker, item.Id, item.ProjectId,
                    item.Project.Key, item.Project.Name, string.Empty, item.Summary, "Treffer in Blockadedaten"))
                .ToListAsync(cancellationToken).ConfigureAwait(false));
        }

        if (!requirementOnlyFilter && query.ObjectType is null or SearchObjectType.ExternalReference)
        {
            var source = context.ExternalReferences.AsNoTracking();
            if (query.ProjectId is not null) source = source.Where(item => item.ProjectId == query.ProjectId);
            if (query.ReferenceType is not null) source = source.Where(item => item.Type == query.ReferenceType);
            if (text is not null) source = source.Where(item => item.Title.Contains(text) || item.Target.Contains(text));
            total += await source.CountAsync(cancellationToken).ConfigureAwait(false);
            rows.AddRange(await source.Take(query.Offset + query.Limit)
                .Select(item => new SearchRow(SearchObjectType.ExternalReference, item.Id,
                    item.ProjectId, item.Project.Key, item.Project.Name, string.Empty, item.Title,
                    "Treffer in Referenztitel oder Ziel"))
                .ToListAsync(cancellationToken).ConfigureAwait(false));
        }

        IEnumerable<SearchRow> ordered = query.Sort switch
        {
            SearchSort.Project => rows.OrderBy(item => item.ProjectKey).ThenBy(item => item.ObjectType).ThenBy(item => item.Title),
            SearchSort.ObjectType => rows.OrderBy(item => item.ObjectType).ThenBy(item => item.ProjectKey).ThenBy(item => item.Title),
            SearchSort.Title => rows.OrderBy(item => item.Title).ThenBy(item => item.ProjectKey).ThenBy(item => item.ObjectType),
            _ => rows.OrderBy(item => item.ObjectType).ThenBy(item => item.ProjectKey).ThenBy(item => item.Title),
        };
        var items = ordered.Skip(query.Offset).Take(query.Limit)
            .Select(item => new SearchResult(item.ObjectType, item.EntityId, item.ProjectId, item.ProjectKey,
                item.ProjectName, item.Key, item.Title, item.MatchHint))
            .ToList();
        return new SearchPage(items, total, query.Offset + items.Count < total);
    }

    private sealed record SearchRow(SearchObjectType ObjectType, Guid EntityId, Guid ProjectId, string ProjectKey,
        string ProjectName, string Key, string Title, string MatchHint);
}
