using Sasd.Pims.Domain.Requirements;
namespace Sasd.Pims.Infrastructure.Persistence;
internal sealed class RequirementRecord
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Rationale { get; set; }
    public RequirementPriority Priority { get; set; }
    public RequirementDecisionStatus DecisionStatus { get; set; }
    public string? DecisionReason { get; set; }
    public RequirementSourceType SourceType { get; set; }
    public DateOnly? SourceDate { get; set; }
    public string? SourceSummary { get; set; }
    public Guid? SourceReferenceId { get; set; }
    public int Revision { get; set; }
    public ProjectRecord Project { get; set; } = null!;
    public List<AcceptanceCriterionRecord> AcceptanceCriteria { get; set; } = [];
}
