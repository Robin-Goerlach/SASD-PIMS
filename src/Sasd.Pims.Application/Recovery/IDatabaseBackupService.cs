namespace Sasd.Pims.Application.Recovery;

public interface IDatabaseBackupService
{
    Task CreateAsync(
        string databasePath,
        string packagePath,
        string applicationVersion,
        CancellationToken cancellationToken = default);
}
