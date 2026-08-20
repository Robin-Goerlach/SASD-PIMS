using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Domain.Requirements;

/// <summary>Captures a project-bound requirement and its ordered acceptance information.</summary>
public sealed class Requirement
{
    private readonly AcceptanceCriterion[] _criteria;

    private Requirement(Guid id, Guid projectId, string key, string title, string? description,
        string? rationale, RequirementPriority priority, RequirementDecisionStatus decisionStatus,
        string? decisionReason, RequirementSourceType sourceType, DateOnly? sourceDate,
        string? sourceSummary, Guid? sourceReferenceId, IEnumerable<AcceptanceCriterion> criteria, int revision)
    {
        Id = id; ProjectId = projectId; Key = key; Title = title; Description = description;
        Rationale = rationale; Priority = priority; DecisionStatus = decisionStatus;
        DecisionReason = decisionReason; SourceType = sourceType; SourceDate = sourceDate;
        SourceSummary = sourceSummary; SourceReferenceId = sourceReferenceId;
        _criteria = criteria.OrderBy(item => item.Sequence).ToArray(); Revision = revision;
    }

    public Guid Id { get; }
    public Guid ProjectId { get; }
    public string Key { get; }
    public string Title { get; }
    public string? Description { get; }
    public string? Rationale { get; }
    public RequirementPriority Priority { get; }
    public RequirementDecisionStatus DecisionStatus { get; }
    public string? DecisionReason { get; }
    public RequirementSourceType SourceType { get; }
    public DateOnly? SourceDate { get; }
    public string? SourceSummary { get; }
    public Guid? SourceReferenceId { get; }
    public IReadOnlyList<AcceptanceCriterion> AcceptanceCriteria => _criteria;
    public int Revision { get; }

    /// <summary>Creates a Requirement with its already allocated immutable project-local key.</summary>
    public static Requirement Create(Guid id, Guid projectId, string? key, string? title, string? description,
        string? rationale, RequirementPriority priority, RequirementDecisionStatus decisionStatus,
        string? decisionReason, RequirementSourceType sourceType, DateOnly? sourceDate, string? sourceSummary,
        Guid? sourceReferenceId, IEnumerable<AcceptanceCriterion>? criteria) => Reconstitute(id, projectId, key,
            title, description, rationale, priority, decisionStatus, decisionReason, sourceType, sourceDate,
            sourceSummary, sourceReferenceId, criteria, 1);

    /// <summary>Reconstitutes persisted Requirement facts and validates all aggregate invariants.</summary>
    public static Requirement Reconstitute(Guid id, Guid projectId, string? key, string? title,
        string? description, string? rationale, RequirementPriority priority,
        RequirementDecisionStatus decisionStatus, string? decisionReason, RequirementSourceType sourceType,
        DateOnly? sourceDate, string? sourceSummary, Guid? sourceReferenceId,
        IEnumerable<AcceptanceCriterion>? criteria, int revision)
    {
        if (id == Guid.Empty) throw Error(nameof(Id), "RequirementIdRequired", "Requirement ID is required.");
        if (projectId == Guid.Empty) throw Error(nameof(ProjectId), "RequirementProjectRequired", "Project ID is required.");
        var normalizedKey = RequiredKey(key);
        if (string.IsNullOrWhiteSpace(title)) throw Error(nameof(Title), "RequirementTitleRequired", "Requirement title is required.");
        if (!Enum.IsDefined(priority)) throw Error(nameof(Priority), "RequirementPriorityInvalid", "Requirement priority is invalid.");
        if (!Enum.IsDefined(decisionStatus)) throw Error(nameof(DecisionStatus), "RequirementDecisionStatusInvalid", "Requirement decision status is invalid.");
        if (!Enum.IsDefined(sourceType)) throw Error(nameof(SourceType), "RequirementSourceTypeInvalid", "Requirement source type is invalid.");
        var normalizedReason = Optional(decisionReason);
        if (decisionStatus is RequirementDecisionStatus.Deferred or RequirementDecisionStatus.Rejected && normalizedReason is null)
            throw Error(nameof(DecisionReason), "RequirementDecisionReasonRequired", "Deferred and rejected Requirements require a reason.");
        var normalizedCriteria = (criteria ?? []).OrderBy(item => item.Sequence).ToArray();
        if (normalizedCriteria.Any(item => item.RequirementId != id))
            throw Error(nameof(AcceptanceCriteria), "CriterionRequirementMismatch", "All criteria must belong to the Requirement.");
        if (normalizedCriteria.Select(item => item.Sequence).Distinct().Count() != normalizedCriteria.Length)
            throw Error(nameof(AcceptanceCriteria), "CriterionSequenceDuplicate", "Criterion sequences must be unique.");
        if (priority == RequirementPriority.Must && normalizedCriteria.Length == 0)
            throw Error(nameof(AcceptanceCriteria), "MustRequirementNeedsCriterion", "A Must Requirement needs an acceptance criterion.");
        if (revision < 1) throw Error(nameof(Revision), "RequirementRevisionInvalid", "Requirement revision must be positive.");
        return new(id, projectId, normalizedKey, title.Trim(), Optional(description), Optional(rationale), priority,
            decisionStatus, normalizedReason, sourceType, sourceDate, Optional(sourceSummary), sourceReferenceId,
            normalizedCriteria, revision);
    }

    /// <summary>Returns an edited copy while preserving the stable identity, Project and key.</summary>
    public Requirement Update(string? title, string? description, string? rationale, RequirementPriority priority,
        RequirementDecisionStatus decisionStatus, string? decisionReason, RequirementSourceType sourceType,
        DateOnly? sourceDate, string? sourceSummary, Guid? sourceReferenceId,
        IEnumerable<AcceptanceCriterion>? criteria) => Reconstitute(Id, ProjectId, Key, title, description,
            rationale, priority, decisionStatus, decisionReason, sourceType, sourceDate, sourceSummary,
            sourceReferenceId, criteria, checked(Revision + 1));

    private static string RequiredKey(string? key)
    {
        var value = key?.Trim().ToUpperInvariant();
        if (value is null || value.Length < 7 || !value.StartsWith("REQ-", StringComparison.Ordinal) ||
            value.AsSpan(4).Length < 3 || !int.TryParse(value.AsSpan(4), out var number) || number < 1)
            throw Error(nameof(Key), "RequirementKeyInvalid", "Requirement key must use REQ-001 format.");
        return value;
    }

    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    internal static DomainValidationException Error(string field, string code, string message) => new([new DomainValidationError(field, code, message)]);
}
