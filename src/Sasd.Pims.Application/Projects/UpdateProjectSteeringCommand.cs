using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Projects;

/// <summary>Supplies controlled project steering facts for an optimistic-concurrency update.</summary>
public sealed record UpdateProjectSteeringCommand(Guid Id, int ExpectedRevision, ProjectPhase Phase,
    ActivityState ActivityState, DateOnly? TargetDate, DateTimeOffset? NextReviewDueAtUtc);
