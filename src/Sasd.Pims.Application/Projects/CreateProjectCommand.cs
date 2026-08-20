namespace Sasd.Pims.Application.Projects;

/// <summary>Contains user-supplied values for a new project master record.</summary>
public sealed record CreateProjectCommand(
    string? Key,
    string? Name,
    string? ShortDescription,
    string? Goal = null,
    string? Benefit = null,
    string? ProjectType = null,
    string? ProjectArea = null,
    string? Responsibility = null,
    IReadOnlyList<string?>? Tags = null);
