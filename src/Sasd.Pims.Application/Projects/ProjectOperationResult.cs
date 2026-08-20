namespace Sasd.Pims.Application.Projects;

public sealed record ProjectOperationResult<T>(
    ProjectOperationStatus Status,
    T? Value,
    IReadOnlyList<ProjectValidationError> Errors,
    string? ErrorId = null);

public static class ProjectOperationResult
{
    public static ProjectOperationResult<T> Success<T>(T value) =>
        new(ProjectOperationStatus.Success, value, []);

    public static ProjectOperationResult<T> ValidationFailed<T>(IReadOnlyList<ProjectValidationError> errors) =>
        new(ProjectOperationStatus.ValidationFailed, default, errors);

    public static ProjectOperationResult<T> Conflict<T>(ProjectValidationError error) =>
        new(ProjectOperationStatus.Conflict, default, [error]);

    public static ProjectOperationResult<T> NotFound<T>() =>
        new(ProjectOperationStatus.NotFound, default, []);

    public static ProjectOperationResult<T> InfrastructureFailure<T>(string errorId) =>
        new(ProjectOperationStatus.InfrastructureFailure, default, [], errorId);
}
