namespace Sasd.Pims.Application.Projects;

public sealed record CreateProjectCommand(string? Key, string? Name, string? ShortDescription);
