using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Projects;

/// <summary>Represents one lightweight row in the project catalog.</summary>
public sealed record ProjectSummaryDto(
    Guid Id,
    string Key,
    string Name,
    string? ProjectType,
    string? ProjectArea,
    ProjectPhase Phase,
    ActivityState ActivityState,
    DateOnly? TargetDate,
    ReviewFreshness ReviewFreshness,
    DueDateIndication DueDateIndication,
    IReadOnlyList<AttentionReason> AttentionReasons,
    bool IsArchived)
{
    public bool NeedsAttention => AttentionReasons.Count > 0;
    public override string ToString() => $"{Key} — {Name}";
}
