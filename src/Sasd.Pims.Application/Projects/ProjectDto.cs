using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Projects;

public sealed record ProjectDto(
    Guid Id,
    string Key,
    string Name,
    string? ShortDescription,
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
            project.CreatedAtUtc,
            project.ModifiedAtUtc,
            project.Revision);
}
