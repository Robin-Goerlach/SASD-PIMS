using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Projects;

/// <summary>Transfers a complete project master record without persistence or UI dependencies.</summary>
public sealed record ProjectDto(
    Guid Id,
    string Key,
    string Name,
    string? ShortDescription,
    string? Goal,
    string? Benefit,
    string? ProjectType,
    string? ProjectArea,
    string? Responsibility,
    IReadOnlyList<string> Tags,
    bool IsArchived,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ModifiedAtUtc,
    int Revision)
{
    public static ProjectDto FromDomain(Project project) =>
        new(
            project.Id,
            project.Key.Value,
            project.Name,
            project.ShortDescription,
            project.Goal,
            project.Benefit,
            project.ProjectType,
            project.ProjectArea,
            project.Responsibility,
            project.Tags,
            project.IsArchived,
            project.CreatedAtUtc,
            project.ModifiedAtUtc,
            project.Revision);
}
