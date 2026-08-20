namespace Sasd.Pims.Domain.Auditing;

/// <summary>Represents one immutable, selectively captured business change.</summary>
public sealed record ChangeEvent(
    Guid Id,
    Guid ProjectId,
    string EntityType,
    Guid EntityId,
    string EventType,
    DateTimeOffset OccurredAtUtc,
    string? OldValue,
    string? NewValue);
