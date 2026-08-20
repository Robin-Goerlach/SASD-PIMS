namespace Sasd.Pims.Infrastructure.Persistence;

internal sealed class ProjectRecord
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string? Goal { get; set; }

    public string? Benefit { get; set; }

    public string? ProjectType { get; set; }

    public string? ProjectArea { get; set; }

    public string? Responsibility { get; set; }

    public bool IsArchived { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset ModifiedAtUtc { get; set; }

    public int Revision { get; set; }

    public List<ProjectTagRecord> Tags { get; set; } = [];
}
