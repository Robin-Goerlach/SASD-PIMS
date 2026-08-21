using Microsoft.EntityFrameworkCore;
using Sasd.Pims.Application.Auditing;

namespace Sasd.Pims.Infrastructure.Persistence;

public sealed class SqliteChangeEventReader(IDbContextFactory<PimsDbContext> contextFactory) : IChangeEventReader
{
    public async Task<IReadOnlyList<ProjectChangeEventRow>> ListByProjectAsync(Guid projectId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var events = await context.ChangeEvents.AsNoTracking().Where(item => item.ProjectId == projectId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        var projectKey = await context.Projects.AsNoTracking().Where(item => item.Id == projectId)
            .Select(item => item.Key).SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false) ?? projectId.ToString();
        var requirementKeys = await context.Requirements.AsNoTracking().Where(item => item.ProjectId == projectId)
            .ToDictionaryAsync(item => item.Id, item => item.Key, cancellationToken).ConfigureAwait(false);
        var blockerNames = await context.ProjectBlockers.AsNoTracking().Where(item => item.ProjectId == projectId)
            .ToDictionaryAsync(item => item.Id, item => item.Summary, cancellationToken).ConfigureAwait(false);
        var referenceNames = await context.ExternalReferences.AsNoTracking().Where(item => item.ProjectId == projectId)
            .ToDictionaryAsync(item => item.Id, item => item.Title, cancellationToken).ConfigureAwait(false);
        // SQLite has no native DateTimeOffset ordering; the bounded per-project audit stream is sorted after projection.
        return events.Select(item => new ProjectChangeEventRow(item.Id, item.ProjectId, item.EntityType,
                item.EntityId, Identifier(item.EntityType, item.EntityId), item.EventType, item.OccurredAtUtc,
                item.OldValue, item.NewValue))
            .OrderBy(item => item.OccurredAtUtc).ThenBy(item => item.Id).ToArray();

        string Identifier(string entityType, Guid entityId) => entityType switch
        {
            "Project" => projectKey,
            "Requirement" when requirementKeys.TryGetValue(entityId, out var key) => key,
            "Blocker" when blockerNames.TryGetValue(entityId, out var summary) => summary,
            "ExternalReference" when referenceNames.TryGetValue(entityId, out var title) => title,
            _ => entityId.ToString(),
        };
    }
}
