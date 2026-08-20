namespace Sasd.Pims.Infrastructure.Persistence;

internal sealed class ProjectTagRecord
{
    public Guid ProjectId { get; set; }
    public string Value { get; set; } = string.Empty;
    public ProjectRecord Project { get; set; } = null!;
}
