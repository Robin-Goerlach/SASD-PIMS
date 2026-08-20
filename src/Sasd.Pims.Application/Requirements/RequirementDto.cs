using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.Application.Requirements;

/// <summary>Neutral Requirement result model including ordered acceptance criteria.</summary>
public sealed record RequirementDto(Guid Id, Guid ProjectId, string Key, string Title, string? Description,
    string? Rationale, RequirementPriority Priority, RequirementDecisionStatus DecisionStatus,
    string? DecisionReason, RequirementSourceType SourceType, DateOnly? SourceDate, string? SourceSummary,
    Guid? SourceReferenceId, IReadOnlyList<AcceptanceCriterionDto> AcceptanceCriteria, int Revision)
{
    public static RequirementDto FromDomain(Requirement item) => new(item.Id, item.ProjectId, item.Key, item.Title,
        item.Description, item.Rationale, item.Priority, item.DecisionStatus, item.DecisionReason, item.SourceType,
        item.SourceDate, item.SourceSummary, item.SourceReferenceId,
        item.AcceptanceCriteria.Select(AcceptanceCriterionDto.FromDomain).ToArray(), item.Revision);
}

public sealed record AcceptanceCriterionDto(Guid Id, int Sequence, string Text, Guid? VerificationReferenceId)
{
    public static AcceptanceCriterionDto FromDomain(AcceptanceCriterion item) => new(item.Id, item.Sequence, item.Text, item.VerificationReferenceId);
}
