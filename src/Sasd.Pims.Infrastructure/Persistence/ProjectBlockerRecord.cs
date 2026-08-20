namespace Sasd.Pims.Infrastructure.Persistence;

internal sealed class ProjectBlockerRecord
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ResolvedAtUtc { get; set; }
    public string? ResolutionNote { get; set; }
    public ProjectRecord Project { get; set; } = null!;
}
