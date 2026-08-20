namespace Sasd.Pims.Application.Projects;

/// <summary>Represents one lightweight row in the project catalog.</summary>
public sealed record ProjectSummaryDto(
    Guid Id,
    string Key,
    string Name,
    string? ProjectType,
    string? ProjectArea,
    bool IsArchived)
{
    public override string ToString() => $"{Key} — {Name}";
}
