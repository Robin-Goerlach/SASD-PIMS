namespace Sasd.Pims.Application.Recovery;

/// <summary>Creates a verified, versioned package from the active local database.</summary>
public interface IDatabaseBackupService
{
    /// <summary>Creates a consistent package and returns only after checksum and database validation succeed.</summary>
    Task CreateAsync(
        string databasePath,
        string packagePath,
        string applicationVersion,
        CancellationToken cancellationToken = default);
}
