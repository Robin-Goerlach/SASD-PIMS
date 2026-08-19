using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Application.Projects;

namespace Sasd.Pims.Application.Recovery;

public sealed class RestoreDatabaseBackup(
    IDatabaseRestoreService restoreService,
    OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<string>> ExecuteAsync(
        string databasePath,
        string packagePath,
        string rollbackDirectory,
        string applicationVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var rollbackPackage = await restoreService.RestoreAsync(
                    databasePath,
                    packagePath,
                    rollbackDirectory,
                    applicationVersion,
                    cancellationToken)
                .ConfigureAwait(false);
            return ProjectOperationResult.Success(rollbackPackage);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            return failureHandler.Report<string>("RestoreDatabaseBackup", exception);
        }
    }
}
