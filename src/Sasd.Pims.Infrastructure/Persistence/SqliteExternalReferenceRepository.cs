using Microsoft.EntityFrameworkCore;
using Sasd.Pims.Application.Requirements;
using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.Infrastructure.Persistence;

/// <summary>Persists typed references without probing or copying their targets.</summary>
public sealed class SqliteExternalReferenceRepository(IDbContextFactory<PimsDbContext> contextFactory) : IExternalReferenceRepository
{
    public async Task<RequirementWriteResult> AddAsync(ExternalReference reference, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        context.ExternalReferences.Add(ToRecord(reference));
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return RequirementWriteResult.Saved;
    }

    public async Task<RequirementWriteResult> UpdateAsync(ExternalReference reference, int expectedRevision,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var previous = await context.ExternalReferences.AsNoTracking().SingleOrDefaultAsync(item => item.Id == reference.Id,
            cancellationToken).ConfigureAwait(false);
        if (previous is null || previous.Revision != expectedRevision) return RequirementWriteResult.ConcurrencyConflict;
        var affected = await context.ExternalReferences.Where(item => item.Id == reference.Id && item.Revision == expectedRevision)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.Type, reference.Type)
                .SetProperty(item => item.Title, reference.Title).SetProperty(item => item.Target, reference.Target)
                .SetProperty(item => item.Revision, reference.Revision), cancellationToken).ConfigureAwait(false);
        if (affected != 1) return RequirementWriteResult.ConcurrencyConflict;
        Add(nameof(ExternalReference.Type), previous.Type.ToString(), reference.Type.ToString());
        Add(nameof(ExternalReference.Title), Redacted(previous.Title), Redacted(reference.Title));
        if (!StringComparer.Ordinal.Equals(previous.Target, reference.Target)) Add("TargetChanged", null, null);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return RequirementWriteResult.Saved;

        void Add(string eventType, string? oldValue, string? newValue)
        {
            if (eventType != "TargetChanged" && StringComparer.Ordinal.Equals(oldValue, newValue)) return;
            context.ChangeEvents.Add(new ChangeEventRecord
            {
                Id = Guid.NewGuid(), ProjectId = reference.ProjectId, EntityType = "ExternalReference",
                EntityId = reference.Id, EventType = eventType, OccurredAtUtc = DateTimeOffset.UtcNow,
                OldValue = oldValue, NewValue = newValue,
            });
        }

        static string Redacted(string value) => value.Length <= 80 ? value : string.Concat(value.AsSpan(0, 80), "…");
    }

    public async Task<ExternalReference?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var item = await context.ExternalReferences.AsNoTracking().SingleOrDefaultAsync(value => value.Id == id, cancellationToken).ConfigureAwait(false);
        return item is null ? null : ToDomain(item);
    }

    public async Task<IReadOnlyList<ExternalReference>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        return (await context.ExternalReferences.AsNoTracking().Where(item => item.ProjectId == projectId)
            .OrderBy(item => item.Title).ToListAsync(cancellationToken).ConfigureAwait(false)).Select(ToDomain).ToArray();
    }

    private static ExternalReferenceRecord ToRecord(ExternalReference item) => new()
    { Id = item.Id, ProjectId = item.ProjectId, RequirementId = item.RequirementId, Type = item.Type,
        Title = item.Title, Target = item.Target, Revision = item.Revision };
    private static ExternalReference ToDomain(ExternalReferenceRecord item) => ExternalReference.Reconstitute(item.Id,
        item.ProjectId, item.RequirementId, item.Type, item.Title, item.Target, item.Revision);
}
