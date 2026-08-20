using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Projects;

/// <summary>Transfers an obstacle together with its retained resolution history.</summary>
public sealed record ProjectBlockerDto(Guid Id, Guid ProjectId, string Summary, string? Details,
    DateTimeOffset CreatedAtUtc, DateTimeOffset? ResolvedAtUtc, string? ResolutionNote)
{
    public bool IsOpen => ResolvedAtUtc is null;

    public static ProjectBlockerDto FromDomain(ProjectBlocker blocker) => new(blocker.Id, blocker.ProjectId,
        blocker.Summary, blocker.Details, blocker.CreatedAtUtc, blocker.ResolvedAtUtc, blocker.ResolutionNote);
}
