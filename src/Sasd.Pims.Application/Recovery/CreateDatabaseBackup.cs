using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Application.Projects;

namespace Sasd.Pims.Application.Recovery;

public sealed class CreateDatabaseBackup(
    IDatabaseBackupService backupService,
    OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<string>> ExecuteAsync(
        string databasePath,
        string packagePath,
        string applicationVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await backupService.CreateAsync(databasePath, packagePath, applicationVersion, cancellationToken)
                .ConfigureAwait(false);
            return ProjectOperationResult.Success(packagePath);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            return failureHandler.Report<string>("CreateDatabaseBackup", exception);
        }
    }
}
