namespace Sasd.Pims.Domain.Projects;

/// <summary>Represents date proximity only and must not be interpreted as overall project health.</summary>
public enum DueDateIndication
{
    Neutral,
    OnTrack,
    DueSoon,
    DueToday,
    Overdue,
}
