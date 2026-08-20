using Microsoft.EntityFrameworkCore;
using Sasd.Pims.Application.Projects;
using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Infrastructure.Persistence;

/// <summary>Stores blocker history in SQLite with a conditional resolve update for concurrent editors.</summary>
public sealed class SqliteProjectBlockerRepository(IDbContextFactory<PimsDbContext> contextFactory)
    : IProjectBlockerRepository
{
    public async Task AddAsync(ProjectBlocker blocker, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        context.ProjectBlockers.Add(ToRecord(blocker));
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<ProjectBlocker?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var record = await context.ProjectBlockers.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken).ConfigureAwait(false);
        return record is null ? null : ToDomain(record);
    }

    public async Task<IReadOnlyList<ProjectBlocker>> ListByProjectAsync(Guid projectId,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var records = await context.ProjectBlockers.AsNoTracking().Where(item => item.ProjectId == projectId)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        // SQLite cannot order DateTimeOffset expressions; the per-project history is intentionally small in 0.2.
        return records.OrderBy(item => item.ResolvedAtUtc is not null).ThenByDescending(item => item.CreatedAtUtc)
            .Select(ToDomain).ToArray();
    }

    public async Task<IReadOnlySet<Guid>> GetProjectIdsWithOpenBlockersAsync(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        return (await context.ProjectBlockers.AsNoTracking().Where(item => item.ResolvedAtUtc == null)
            .Select(item => item.ProjectId).Distinct().ToListAsync(cancellationToken).ConfigureAwait(false)).ToHashSet();
    }

    public async Task<bool> ResolveAsync(ProjectBlocker blocker, CancellationToken cancellationToken)
    {
        // The predicate makes resolution a one-way atomic transition instead of silently overwriting history.
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var affected = await context.ProjectBlockers.Where(item => item.Id == blocker.Id && item.ResolvedAtUtc == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.ResolvedAtUtc, blocker.ResolvedAtUtc)
                .SetProperty(item => item.ResolutionNote, blocker.ResolutionNote), cancellationToken)
            .ConfigureAwait(false);
        return affected == 1;
    }

    private static ProjectBlockerRecord ToRecord(ProjectBlocker blocker) => new()
    {
        Id = blocker.Id, ProjectId = blocker.ProjectId, Summary = blocker.Summary, Details = blocker.Details,
        CreatedAtUtc = blocker.CreatedAtUtc, ResolvedAtUtc = blocker.ResolvedAtUtc,
        ResolutionNote = blocker.ResolutionNote,
    };

    private static ProjectBlocker ToDomain(ProjectBlockerRecord record) => ProjectBlocker.Reconstitute(record.Id,
        record.ProjectId, record.Summary, record.Details, record.CreatedAtUtc, record.ResolvedAtUtc,
        record.ResolutionNote);
}
