namespace Sasd.Pims.Application.Auditing;

/// <summary>Represents one safe, read-only row in a Project's business change history.</summary>
public sealed record ProjectChangeEventRow(Guid Id, Guid ProjectId, string EntityType, Guid EntityId,
    string ObjectIdentifier, string EventType, DateTimeOffset OccurredAtUtc, string? OldValue, string? NewValue);
