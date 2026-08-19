namespace Sasd.Pims.Application.Recovery;

public interface IDatabaseRestoreService
{
    Task<string> RestoreAsync(
        string databasePath,
        string packagePath,
        string rollbackDirectory,
        string applicationVersion,
        CancellationToken cancellationToken = default);
}
