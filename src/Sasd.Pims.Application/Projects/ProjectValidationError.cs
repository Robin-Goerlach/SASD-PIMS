namespace Sasd.Pims.Application.Projects;

public sealed record ProjectValidationError(string Field, string Code, string Message);
