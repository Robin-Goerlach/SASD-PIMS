namespace Sasd.Pims.Application.Projects;

public sealed record ProjectSummaryDto(Guid Id, string Key, string Name)
{
    public override string ToString() => $"{Key} — {Name}";
}
