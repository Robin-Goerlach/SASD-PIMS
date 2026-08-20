namespace Sasd.Pims.Infrastructure.Persistence;
internal sealed class AcceptanceCriterionRecord
{
    public Guid Id { get; set; }
    public Guid RequirementId { get; set; }
    public int Sequence { get; set; }
    public string Text { get; set; } = string.Empty;
    public Guid? VerificationReferenceId { get; set; }
    public RequirementRecord Requirement { get; set; } = null!;
}
