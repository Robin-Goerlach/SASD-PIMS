namespace Sasd.Pims.Domain.Projects;

/// <summary>Identifies whether and how project work is currently taking place, independently of phase and archive.</summary>
public enum ActivityState
{
    NotStarted,
    Active,
    Paused,
    Completed,
    Cancelled,
}
