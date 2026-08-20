namespace Sasd.Pims.Domain.Projects;

public sealed class DomainValidationException : Exception
{
    public DomainValidationException(IReadOnlyList<DomainValidationError> errors)
        : base("One or more domain validation errors occurred.")
    {
        Errors = errors;
    }

    public IReadOnlyList<DomainValidationError> Errors { get; }
}
