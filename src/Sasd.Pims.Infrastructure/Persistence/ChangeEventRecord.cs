namespace Sasd.Pims.Infrastructure.Persistence;

internal sealed class ChangeEventRecord
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public ProjectRecord Project { get; set; } = null!;
}
