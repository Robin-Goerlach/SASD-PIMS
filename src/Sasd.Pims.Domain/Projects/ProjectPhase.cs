namespace Sasd.Pims.Domain.Projects;

/// <summary>Identifies the controlled lifecycle position of a project, independently of current activity.</summary>
public enum ProjectPhase
{
    Idea,
    Preparation,
    Execution,
    Validation,
    Closure,
}
