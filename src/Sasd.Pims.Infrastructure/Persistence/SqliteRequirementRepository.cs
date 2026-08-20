using Microsoft.EntityFrameworkCore;
using Sasd.Pims.Application.Requirements;
using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.Infrastructure.Persistence;

/// <summary>Persists Requirements and ordered criteria with explicit optimistic concurrency.</summary>
public sealed class SqliteRequirementRepository(IDbContextFactory<PimsDbContext> contextFactory) : IRequirementRepository
{
    public async Task<string> GetNextKeyAsync(Guid projectId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var keys = await context.Requirements.AsNoTracking().Where(item => item.ProjectId == projectId)
            .Select(item => item.Key).ToListAsync(cancellationToken).ConfigureAwait(false);
        // No Requirement is deleted in 0.3, so the maximum allocated number is never reused.
        var next = keys.Select(ParseNumber).DefaultIfEmpty(0).Max() + 1;
        return $"REQ-{next:000}";
    }

    public async Task<RequirementWriteResult> AddAsync(Requirement requirement, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        context.Requirements.Add(ToRecord(requirement));
        try
        {
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return RequirementWriteResult.Saved;
        }
        catch (DbUpdateException)
        {
            await using var check = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
            if (await check.Requirements.AsNoTracking().AnyAsync(item => item.ProjectId == requirement.ProjectId && item.Key == requirement.Key, cancellationToken).ConfigureAwait(false))
                return RequirementWriteResult.DuplicateKey;
            throw;
        }
    }

    public async Task<RequirementWriteResult> UpdateAsync(Requirement requirement, int expectedRevision,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var previous = await context.Requirements.AsNoTracking().SingleOrDefaultAsync(item => item.Id == requirement.Id,
            cancellationToken).ConfigureAwait(false);
        if (previous is null || previous.Revision != expectedRevision) return RequirementWriteResult.ConcurrencyConflict;
        var affected = await context.Requirements.Where(item => item.Id == requirement.Id && item.Revision == expectedRevision)
            .ExecuteUpdateAsync(setters => setters.SetProperty(item => item.Title, requirement.Title)
                .SetProperty(item => item.Description, requirement.Description)
                .SetProperty(item => item.Rationale, requirement.Rationale)
                .SetProperty(item => item.Priority, requirement.Priority)
                .SetProperty(item => item.DecisionStatus, requirement.DecisionStatus)
                .SetProperty(item => item.DecisionReason, requirement.DecisionReason)
                .SetProperty(item => item.SourceType, requirement.SourceType)
                .SetProperty(item => item.SourceDate, requirement.SourceDate)
                .SetProperty(item => item.SourceSummary, requirement.SourceSummary)
                .SetProperty(item => item.SourceReferenceId, requirement.SourceReferenceId)
                .SetProperty(item => item.Revision, requirement.Revision), cancellationToken).ConfigureAwait(false);
        if (affected == 0) return RequirementWriteResult.ConcurrencyConflict;
        AddRequirementChangeEvents(context, previous, requirement);
        await context.AcceptanceCriteria.Where(item => item.RequirementId == requirement.Id)
            .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        context.AcceptanceCriteria.AddRange(requirement.AcceptanceCriteria.Select(ToRecord));
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        return RequirementWriteResult.Saved;
    }

    private static void AddRequirementChangeEvents(PimsDbContext context, RequirementRecord old, Requirement current)
    {
        Add(nameof(Requirement.Priority), old.Priority.ToString(), current.Priority.ToString());
        Add(nameof(Requirement.DecisionStatus), old.DecisionStatus.ToString(), current.DecisionStatus.ToString());
        // Decision reasons may contain sensitive prose. Audit only presence/absence, never the content.
        Add(nameof(Requirement.DecisionReason), Presence(old.DecisionReason), Presence(current.DecisionReason));

        void Add(string eventType, string? oldValue, string? newValue)
        {
            if (StringComparer.Ordinal.Equals(oldValue, newValue)) return;
            context.ChangeEvents.Add(new ChangeEventRecord
            {
                Id = Guid.NewGuid(), ProjectId = current.ProjectId, EntityType = "Requirement", EntityId = current.Id,
                EventType = eventType, OccurredAtUtc = DateTimeOffset.UtcNow,
                OldValue = oldValue, NewValue = newValue,
            });
        }

        static string Presence(string? value) => string.IsNullOrWhiteSpace(value) ? "Absent" : "Present";
    }

    public async Task<Requirement?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var record = await context.Requirements.AsNoTracking().Include(item => item.AcceptanceCriteria)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken).ConfigureAwait(false);
        return record is null ? null : ToDomain(record);
    }

    public async Task<IReadOnlyList<Requirement>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var records = await context.Requirements.AsNoTracking().Include(item => item.AcceptanceCriteria)
            .Where(item => item.ProjectId == projectId).OrderBy(item => item.Key)
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return records.Select(ToDomain).ToArray();
    }

    private static int ParseNumber(string key) => int.TryParse(key.AsSpan(4), out var value) ? value : 0;
    private static RequirementRecord ToRecord(Requirement item) => new()
    {
        Id = item.Id, ProjectId = item.ProjectId, Key = item.Key, Title = item.Title,
        Description = item.Description, Rationale = item.Rationale, Priority = item.Priority,
        DecisionStatus = item.DecisionStatus, DecisionReason = item.DecisionReason, SourceType = item.SourceType,
        SourceDate = item.SourceDate, SourceSummary = item.SourceSummary, SourceReferenceId = item.SourceReferenceId,
        Revision = item.Revision, AcceptanceCriteria = item.AcceptanceCriteria.Select(ToRecord).ToList(),
    };
    private static AcceptanceCriterionRecord ToRecord(AcceptanceCriterion item) => new()
    { Id = item.Id, RequirementId = item.RequirementId, Sequence = item.Sequence, Text = item.Text,
        VerificationReferenceId = item.VerificationReferenceId };
    private static Requirement ToDomain(RequirementRecord item) => Requirement.Reconstitute(item.Id, item.ProjectId,
        item.Key, item.Title, item.Description, item.Rationale, item.Priority, item.DecisionStatus, item.DecisionReason,
        item.SourceType, item.SourceDate, item.SourceSummary, item.SourceReferenceId,
        item.AcceptanceCriteria.Select(value => AcceptanceCriterion.Create(value.Id, value.RequirementId,
            value.Sequence, value.Text, value.VerificationReferenceId)), item.Revision);
}
