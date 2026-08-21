namespace Sasd.Pims.Infrastructure.Persistence;

internal sealed class ProjectBlockerRecord
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? Cause { get; set; }
    public string? Impact { get; set; }
    public string? AffectedObject { get; set; }
    public string? NextAction { get; set; }
    public Guid? ExternalTaskReferenceId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ResolvedAtUtc { get; set; }
    public string? ResolutionNote { get; set; }
    public ProjectRecord Project { get; set; } = null!;
}
