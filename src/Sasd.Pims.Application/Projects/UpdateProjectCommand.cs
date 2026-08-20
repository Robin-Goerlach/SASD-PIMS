namespace Sasd.Pims.Application.Projects;

/// <summary>Contains editable master data and the revision observed when editing began.</summary>
public sealed record UpdateProjectCommand(
    Guid Id,
    int ExpectedRevision,
    string? Name,
    string? ShortDescription,
    string? Goal,
    string? Benefit,
    string? ProjectType,
    string? ProjectArea,
    string? Responsibility,
    IReadOnlyList<string?>? Tags);
