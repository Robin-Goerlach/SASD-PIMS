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
        var affected = await context.ExternalReferences.Where(item => item.Id == reference.Id && item.Revision == expectedRevision)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.Type, reference.Type)
                .SetProperty(item => item.Title, reference.Title).SetProperty(item => item.Target, reference.Target)
                .SetProperty(item => item.Revision, reference.Revision), cancellationToken).ConfigureAwait(false);
        return affected == 1 ? RequirementWriteResult.Saved : RequirementWriteResult.ConcurrencyConflict;
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
