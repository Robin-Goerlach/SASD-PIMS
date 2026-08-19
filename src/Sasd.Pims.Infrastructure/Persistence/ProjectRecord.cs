namespace Sasd.Pims.Infrastructure.Persistence;

internal sealed class ProjectRecord
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset ModifiedAtUtc { get; set; }

    public int Revision { get; set; }
}
