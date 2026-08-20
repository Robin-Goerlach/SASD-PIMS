namespace Sasd.Pims.Domain.Requirements;

/// <summary>Represents a typed pointer while the referenced external system remains authoritative.</summary>
public sealed class ExternalReference
{
    private ExternalReference(Guid id, Guid projectId, Guid? requirementId, ExternalReferenceType type,
        string title, string target, int revision)
    {
        Id = id; ProjectId = projectId; RequirementId = requirementId; Type = type;
        Title = title; Target = target; Revision = revision;
    }

    public Guid Id { get; }
    public Guid ProjectId { get; }
    public Guid? RequirementId { get; }
    public ExternalReferenceType Type { get; }
    public string Title { get; }
    public string Target { get; }
    public int Revision { get; }

    /// <summary>Creates a Project- or Requirement-owned reference after central target validation.</summary>
    public static ExternalReference Create(Guid id, Guid projectId, Guid? requirementId,
        ExternalReferenceType type, string? title, string? target) =>
        Reconstitute(id, projectId, requirementId, type, title, target, 1);

    /// <summary>Reconstitutes persisted facts while enforcing identity and text invariants.</summary>
    public static ExternalReference Reconstitute(Guid id, Guid projectId, Guid? requirementId,
        ExternalReferenceType type, string? title, string? target, int revision)
    {
        if (id == Guid.Empty) throw Requirement.Error(nameof(Id), "ReferenceIdRequired", "Reference ID is required.");
        if (projectId == Guid.Empty) throw Requirement.Error(nameof(ProjectId), "ReferenceProjectRequired", "Project ID is required.");
        if (requirementId == Guid.Empty) throw Requirement.Error(nameof(RequirementId), "ReferenceRequirementInvalid", "Requirement ID cannot be empty.");
        if (!Enum.IsDefined(type)) throw Requirement.Error(nameof(Type), "ReferenceTypeInvalid", "Reference type is invalid.");
        if (string.IsNullOrWhiteSpace(title)) throw Requirement.Error(nameof(Title), "ReferenceTitleRequired", "Reference title is required.");
        if (string.IsNullOrWhiteSpace(target)) throw Requirement.Error(nameof(Target), "ReferenceTargetRequired", "Reference target is required.");
        if (revision < 1) throw Requirement.Error(nameof(Revision), "ReferenceRevisionInvalid", "Reference revision must be positive.");
        return new(id, projectId, requirementId, type, title.Trim(), target.Trim(), revision);
    }

    /// <summary>Returns an edited copy while preserving identity and ownership.</summary>
    public ExternalReference Update(ExternalReferenceType type, string? title, string? target) =>
        Reconstitute(Id, ProjectId, RequirementId, type, title, target, checked(Revision + 1));
}
