namespace Sasd.Pims.Domain.Projects;

/// <summary>Represents a concrete obstacle and retains its resolution evidence instead of behaving like a task.</summary>
public sealed class ProjectBlocker
{
    private ProjectBlocker(Guid id, Guid projectId, string summary, string? details, string? cause,
        string? impact, string? affectedObject, string? nextAction, Guid? externalTaskReferenceId,
        DateTimeOffset createdAtUtc, DateTimeOffset? resolvedAtUtc, string? resolutionNote)
    {
        Id = id;
        ProjectId = projectId;
        Summary = summary;
        Details = details;
        Cause = cause;
        Impact = impact;
        AffectedObject = affectedObject;
        NextAction = nextAction;
        ExternalTaskReferenceId = externalTaskReferenceId;
        CreatedAtUtc = createdAtUtc;
        ResolvedAtUtc = resolvedAtUtc;
        ResolutionNote = resolutionNote;
    }

    public Guid Id { get; }
    public Guid ProjectId { get; }
    public string Summary { get; }
    public string? Details { get; }
    public string? Cause { get; }
    public string? Impact { get; }
    public string? AffectedObject { get; }
    public string? NextAction { get; }
    public Guid? ExternalTaskReferenceId { get; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset? ResolvedAtUtc { get; private set; }
    public string? ResolutionNote { get; private set; }
    public bool IsOpen => ResolvedAtUtc is null;

    /// <summary>Creates an open blocker for an existing project identity.</summary>
    public static ProjectBlocker Create(Guid id, Guid projectId, string? summary, string? details, string? cause,
        string? impact, string? affectedObject, string? nextAction, Guid? externalTaskReferenceId,
        DateTimeOffset createdAtUtc)
    {
        ValidateIdentityAndTimestamp(id, projectId, createdAtUtc);
        return new(id, projectId, RequiredSummary(summary), OptionalText(details), OptionalText(cause),
            RequiredText(impact, nameof(Impact), "BlockerImpactRequired", "Blocker impact is required."),
            OptionalText(affectedObject),
            RequiredText(nextAction, nameof(NextAction), "BlockerNextActionRequired", "An open blocker requires a next action."),
            OptionalIdentity(externalTaskReferenceId), createdAtUtc, null, null);
    }

    /// <summary>Reconstitutes persisted blocker facts, including retained resolution history.</summary>
    public static ProjectBlocker Reconstitute(Guid id, Guid projectId, string? summary, string? details,
        string? cause, string? impact, string? affectedObject, string? nextAction, Guid? externalTaskReferenceId,
        DateTimeOffset createdAtUtc, DateTimeOffset? resolvedAtUtc, string? resolutionNote)
    {
        ValidateIdentityAndTimestamp(id, projectId, createdAtUtc);
        if (resolvedAtUtc is not null && (resolvedAtUtc.Value.Offset != TimeSpan.Zero || resolvedAtUtc < createdAtUtc))
            throw Error(nameof(ResolvedAtUtc), "BlockerResolvedAtInvalid", "Resolution timestamp must be UTC and not precede creation.");
        if (resolvedAtUtc is null && !string.IsNullOrWhiteSpace(resolutionNote))
            throw Error(nameof(ResolutionNote), "BlockerResolutionInvalid", "An open blocker cannot contain a resolution note.");
        // Null impact/next-action values are accepted only while reconstituting rows created before 0.5.0.
        // Inventing historical business facts during migration would be misleading and irreversible.
        return new(id, projectId, RequiredSummary(summary), OptionalText(details), OptionalText(cause),
            OptionalText(impact), OptionalText(affectedObject), OptionalText(nextAction),
            OptionalIdentity(externalTaskReferenceId), createdAtUtc, resolvedAtUtc, OptionalText(resolutionNote));
    }

    /// <summary>Resolves the blocker once and preserves the optional explanation for later project context.</summary>
    public void Resolve(DateTimeOffset resolvedAtUtc, string? resolutionNote)
    {
        if (!IsOpen) throw Error(nameof(ResolvedAtUtc), "BlockerAlreadyResolved", "The blocker is already resolved.");
        if (resolvedAtUtc.Offset != TimeSpan.Zero || resolvedAtUtc < CreatedAtUtc)
            throw Error(nameof(ResolvedAtUtc), "BlockerResolvedAtInvalid", "Resolution timestamp must be UTC and not precede creation.");
        ResolvedAtUtc = resolvedAtUtc;
        ResolutionNote = OptionalText(resolutionNote);
    }

    private static void ValidateIdentityAndTimestamp(Guid id, Guid projectId, DateTimeOffset createdAtUtc)
    {
        if (id == Guid.Empty) throw Error(nameof(Id), "BlockerIdRequired", "Blocker ID is required.");
        if (projectId == Guid.Empty) throw Error(nameof(ProjectId), "BlockerProjectRequired", "Project ID is required.");
        if (createdAtUtc.Offset != TimeSpan.Zero)
            throw Error(nameof(CreatedAtUtc), "BlockerCreatedAtInvalid", "Creation timestamp must use UTC.");
    }

    private static string RequiredSummary(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw Error(nameof(Summary), "BlockerSummaryRequired", "Blocker summary is required.");
        return value.Trim();
    }

    private static string? OptionalText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string RequiredText(string? value, string field, string code, string message) =>
        OptionalText(value) ?? throw Error(field, code, message);
    private static Guid? OptionalIdentity(Guid? value) => value == Guid.Empty
        ? throw Error(nameof(ExternalTaskReferenceId), "BlockerExternalTaskReferenceInvalid", "External task reference ID cannot be empty.")
        : value;
    private static DomainValidationException Error(string field, string code, string message) => new([new(field, code, message)]);
}
