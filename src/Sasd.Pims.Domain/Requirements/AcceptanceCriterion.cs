namespace Sasd.Pims.Domain.Requirements;

/// <summary>Represents one persistently ordered, testable statement without execution state.</summary>
public sealed class AcceptanceCriterion
{
    private AcceptanceCriterion(Guid id, Guid requirementId, int sequence, string text,
        Guid? verificationReferenceId)
    {
        Id = id;
        RequirementId = requirementId;
        Sequence = sequence;
        Text = text;
        VerificationReferenceId = verificationReferenceId;
    }

    public Guid Id { get; }
    public Guid RequirementId { get; }
    public int Sequence { get; }
    public string Text { get; }
    public Guid? VerificationReferenceId { get; }

    /// <summary>Creates a criterion with a positive persistent sequence.</summary>
    public static AcceptanceCriterion Create(Guid id, Guid requirementId, int sequence, string? text,
        Guid? verificationReferenceId = null)
    {
        if (id == Guid.Empty) throw Requirement.Error(nameof(Id), "CriterionIdRequired", "Criterion ID is required.");
        if (requirementId == Guid.Empty) throw Requirement.Error(nameof(RequirementId), "CriterionRequirementRequired", "Requirement ID is required.");
        if (sequence < 1) throw Requirement.Error(nameof(Sequence), "CriterionSequenceInvalid", "Criterion sequence must be positive.");
        if (string.IsNullOrWhiteSpace(text)) throw Requirement.Error(nameof(Text), "CriterionTextRequired", "Criterion text is required.");
        return new(id, requirementId, sequence, text.Trim(), verificationReferenceId);
    }
}
