namespace Sasd.Pims.Domain.Projects;

/// <summary>Represents review freshness derived from review facts and the current date; it is never persisted.</summary>
public enum ReviewFreshness
{
    NotScheduled,
    NotReviewed,
    Current,
    DueToday,
    Overdue,
}
