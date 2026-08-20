using Microsoft.EntityFrameworkCore;
using Sasd.Pims.Application.Auditing;
using Sasd.Pims.Domain.Auditing;

namespace Sasd.Pims.Infrastructure.Persistence;

public sealed class SqliteChangeEventReader(IDbContextFactory<PimsDbContext> contextFactory) : IChangeEventReader
{
    public async Task<IReadOnlyList<ChangeEvent>> ListByProjectAsync(Guid projectId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var events = await context.ChangeEvents.AsNoTracking().Where(item => item.ProjectId == projectId)
            .Select(item => new ChangeEvent(item.Id, item.ProjectId, item.EntityType, item.EntityId, item.EventType,
                item.OccurredAtUtc, item.OldValue, item.NewValue))
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        // SQLite has no native DateTimeOffset ordering; the bounded per-project audit stream is sorted after projection.
        return events.OrderBy(item => item.OccurredAtUtc).ThenBy(item => item.Id).ToArray();
    }
}
