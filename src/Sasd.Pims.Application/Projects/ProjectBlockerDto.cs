using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Projects;

/// <summary>Transfers an obstacle together with its retained resolution history.</summary>
public sealed record ProjectBlockerDto(Guid Id, Guid ProjectId, string Summary, string? Details, string? Cause,
    string? Impact, string? AffectedObject, string? NextAction, Guid? ExternalTaskReferenceId,
    DateTimeOffset CreatedAtUtc, DateTimeOffset? ResolvedAtUtc, string? ResolutionNote)
{
    public bool IsOpen => ResolvedAtUtc is null;

    public static ProjectBlockerDto FromDomain(ProjectBlocker blocker) => new(blocker.Id, blocker.ProjectId,
        blocker.Summary, blocker.Details, blocker.Cause, blocker.Impact, blocker.AffectedObject, blocker.NextAction,
        blocker.ExternalTaskReferenceId, blocker.CreatedAtUtc, blocker.ResolvedAtUtc, blocker.ResolutionNote);
}
