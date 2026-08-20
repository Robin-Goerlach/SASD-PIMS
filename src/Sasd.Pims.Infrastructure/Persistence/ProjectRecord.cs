namespace Sasd.Pims.Infrastructure.Persistence;

using Sasd.Pims.Domain.Projects;

internal sealed class ProjectRecord
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string? Goal { get; set; }

    public string? Benefit { get; set; }

    public string? ProjectType { get; set; }

    public string? ProjectArea { get; set; }

    public string? Responsibility { get; set; }

    public ProjectPhase Phase { get; set; }

    public ActivityState ActivityState { get; set; }

    public DateOnly? TargetDate { get; set; }

    public DateTimeOffset? LastReviewedAtUtc { get; set; }

    public DateTimeOffset? NextReviewDueAtUtc { get; set; }

    public bool IsArchived { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset ModifiedAtUtc { get; set; }

    public int Revision { get; set; }

    public List<ProjectTagRecord> Tags { get; set; } = [];

    public List<ProjectBlockerRecord> Blockers { get; set; } = [];

    public List<RequirementRecord> Requirements { get; set; } = [];

    public List<ExternalReferenceRecord> ExternalReferences { get; set; } = [];

    public List<ChangeEventRecord> ChangeEvents { get; set; } = [];
}
