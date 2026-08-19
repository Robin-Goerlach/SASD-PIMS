namespace Sasd.Pims.Domain.Projects;

public sealed record DomainValidationError(string Field, string Code, string Message);
