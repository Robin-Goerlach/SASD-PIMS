using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.Application.Requirements;

/// <summary>Captures editable Requirement facts; identity and key are assigned outside UI code.</summary>
public sealed record SaveRequirementCommand(string? Title, string? Description, string? Rationale,
    RequirementPriority Priority, RequirementDecisionStatus DecisionStatus, string? DecisionReason,
    RequirementSourceType SourceType, DateOnly? SourceDate, string? SourceSummary, Guid? SourceReferenceId,
    IReadOnlyList<SaveAcceptanceCriterionCommand> AcceptanceCriteria);

public sealed record SaveAcceptanceCriterionCommand(Guid? Id, string? Text, Guid? VerificationReferenceId);
