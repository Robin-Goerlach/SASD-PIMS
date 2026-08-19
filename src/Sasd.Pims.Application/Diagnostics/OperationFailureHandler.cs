using Microsoft.Extensions.Logging;
using Sasd.Pims.Application.Projects;

namespace Sasd.Pims.Application.Diagnostics;

public sealed class OperationFailureHandler(ILogger<OperationFailureHandler> logger)
{
    private static readonly Action<ILogger, string, string, Exception?> LogOperationFailed =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(1000, "OperationFailed"),
            "Operation {Operation} failed. ErrorId {ErrorId}");

    public ProjectOperationResult<T> Report<T>(string operation, Exception exception)
    {
        var errorId = Guid.NewGuid().ToString("N");
        LogOperationFailed(logger, operation, errorId, exception);
        return ProjectOperationResult.InfrastructureFailure<T>(errorId);
    }
}
