namespace Sasd.Pims.Application.Recovery;

/// <summary>Stages and verifies a backup before replacing the active database.</summary>
public interface IDatabaseRestoreService
{
    /// <summary>
    /// Restores a compatible package and returns the rollback-backup path. Validation failures leave active data untouched.
    /// </summary>
    Task<string> RestoreAsync(
        string databasePath,
        string packagePath,
        string rollbackDirectory,
        string applicationVersion,
        CancellationToken cancellationToken = default);
}
