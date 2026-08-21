using System.IO.Compression;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Sasd.Pims.Application.Recovery;

namespace Sasd.Pims.Infrastructure.Recovery;

public sealed class SqliteRecoveryService : IDatabaseBackupService, IDatabaseRestoreService
{
    public const string BackupFormatVersion = "1.0-internal";
    public const string CurrentSchemaVersion = "202608210005_FullMustMvp";
    private const string DatabaseEntryName = "pims.db";
    private const string ManifestEntryName = "manifest.json";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };
    private readonly IRecoveryFileOperations _fileOperations;

    public SqliteRecoveryService()
        : this(new RecoveryFileOperations())
    {
    }

    internal SqliteRecoveryService(IRecoveryFileOperations fileOperations)
    {
        _fileOperations = fileOperations;
    }

    public static async Task ValidateCurrentDatabaseAsync(
        string databasePath,
        CancellationToken cancellationToken = default)
    {
        await EnsureIntegrityAsync(databasePath, cancellationToken).ConfigureAwait(false);
        if (!string.Equals(
                await ReadSchemaVersionAsync(databasePath, cancellationToken).ConfigureAwait(false),
                CurrentSchemaVersion,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException("The database schema is not current.");
        }

        await SmokeReadAsync(databasePath, cancellationToken).ConfigureAwait(false);
    }

    public async Task CreateAsync(
        string databasePath,
        string packagePath,
        string applicationVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(packagePath);
        if (!File.Exists(databasePath))
        {
            throw new FileNotFoundException("The database to back up does not exist.", databasePath);
        }

        var packageFullPath = Path.GetFullPath(packagePath);
        Directory.CreateDirectory(Path.GetDirectoryName(packageFullPath)!);
        var stagingDirectory = CreateStagingDirectory("backup");
        var stagedDatabase = Path.Combine(stagingDirectory, DatabaseEntryName);
        var temporaryPackage = packageFullPath + ".tmp-" + Guid.NewGuid().ToString("N");

        try
        {
            await BackupDatabaseAsync(databasePath, stagedDatabase, cancellationToken).ConfigureAwait(false);
            await EnsureIntegrityAsync(stagedDatabase, cancellationToken).ConfigureAwait(false);
            var schemaVersion = await ReadSchemaVersionAsync(stagedDatabase, cancellationToken).ConfigureAwait(false);
            var checksum = await ComputeSha256Async(stagedDatabase, cancellationToken).ConfigureAwait(false);
            var manifest = new BackupManifest(
                BackupFormatVersion,
                applicationVersion,
                schemaVersion,
                DateTimeOffset.UtcNow,
                DatabaseEntryName,
                checksum);

            await using (var output = new FileStream(temporaryPackage, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var archive = new ZipArchive(output, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(stagedDatabase, DatabaseEntryName, CompressionLevel.Optimal);
                var manifestEntry = archive.CreateEntry(ManifestEntryName, CompressionLevel.Optimal);
                await using var manifestStream = manifestEntry.Open();
                await JsonSerializer.SerializeAsync(manifestStream, manifest, JsonOptions, cancellationToken)
                    .ConfigureAwait(false);
            }

            File.Move(temporaryPackage, packageFullPath, true);
        }
        finally
        {
            TryDeleteFile(temporaryPackage);
            TryDeleteDirectory(stagingDirectory);
        }
    }

    public async Task<string> RestoreAsync(
        string databasePath,
        string packagePath,
        string rollbackDirectory,
        string applicationVersion,
        CancellationToken cancellationToken = default)
    {
        var activeDatabase = Path.GetFullPath(databasePath);
        var stagingDirectory = CreateRestoreStagingDirectory(activeDatabase);
        var stagedDatabase = Path.Combine(stagingDirectory, DatabaseEntryName);
        var displacedDatabase = activeDatabase + ".restore-previous-" + Guid.NewGuid().ToString("N");

        try
        {
            var manifest = await ExtractAndValidatePackageAsync(packagePath, stagingDirectory, cancellationToken)
                .ConfigureAwait(false);
            var actualHash = await ComputeSha256Async(stagedDatabase, cancellationToken).ConfigureAwait(false);
            if (!CryptographicOperations.FixedTimeEquals(
                    Convert.FromHexString(actualHash),
                    Convert.FromHexString(manifest.DatabaseSha256)))
            {
                throw new InvalidDataException("The database checksum does not match the backup manifest.");
            }

            if (!string.Equals(manifest.SchemaVersion, CurrentSchemaVersion, StringComparison.Ordinal))
            {
                throw new InvalidDataException("The backup schema is not compatible with this application version.");
            }

            await EnsureIntegrityAsync(stagedDatabase, cancellationToken).ConfigureAwait(false);
            if (!string.Equals(
                    await ReadSchemaVersionAsync(stagedDatabase, cancellationToken).ConfigureAwait(false),
                    manifest.SchemaVersion,
                    StringComparison.Ordinal))
            {
                throw new InvalidDataException("The staged database schema does not match the backup manifest.");
            }

            Directory.CreateDirectory(rollbackDirectory);
            var rollbackPackage = Path.Combine(
                Path.GetFullPath(rollbackDirectory),
                $"rollback-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.zip");
            if (File.Exists(activeDatabase))
            {
                await CreateAsync(activeDatabase, rollbackPackage, applicationVersion, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                rollbackPackage = string.Empty;
            }

            SqliteConnection.ClearAllPools();
            Directory.CreateDirectory(Path.GetDirectoryName(activeDatabase)!);
            if (File.Exists(activeDatabase))
            {
                _fileOperations.Replace(stagedDatabase, activeDatabase, displacedDatabase);
            }
            else
            {
                _fileOperations.Move(stagedDatabase, activeDatabase);
            }

            try
            {
                await EnsureIntegrityAsync(activeDatabase, cancellationToken).ConfigureAwait(false);
                await SmokeReadAsync(activeDatabase, cancellationToken).ConfigureAwait(false);
                TryDeleteFile(displacedDatabase);
            }
            catch
            {
                SqliteConnection.ClearAllPools();
                TryDeleteFile(activeDatabase);
                if (File.Exists(displacedDatabase))
                {
                    _fileOperations.Move(displacedDatabase, activeDatabase);
                }

                throw;
            }

            return rollbackPackage;
        }
        finally
        {
            TryDeleteDirectory(stagingDirectory);
        }
    }

    private static async Task<BackupManifest> ExtractAndValidatePackageAsync(
        string packagePath,
        string stagingDirectory,
        CancellationToken cancellationToken)
    {
        using var archive = ZipFile.OpenRead(packagePath);
        var databaseEntries = archive.Entries.Where(entry => entry.FullName == DatabaseEntryName).ToArray();
        var manifestEntries = archive.Entries.Where(entry => entry.FullName == ManifestEntryName).ToArray();
        if (databaseEntries.Length != 1 || manifestEntries.Length != 1)
        {
            throw new InvalidDataException("The backup package must contain exactly one database and one manifest.");
        }

        BackupManifest? manifest;
        await using (var stream = manifestEntries[0].Open())
        {
            manifest = await JsonSerializer.DeserializeAsync<BackupManifest>(stream, JsonOptions, cancellationToken)
                .ConfigureAwait(false);
        }

        if (manifest is null
            || manifest.BackupFormatVersion != BackupFormatVersion
            || manifest.DatabaseFileName != DatabaseEntryName
            || string.IsNullOrWhiteSpace(manifest.ApplicationVersion)
            || string.IsNullOrWhiteSpace(manifest.SchemaVersion)
            || manifest.DatabaseSha256.Length != 64)
        {
            throw new InvalidDataException("The backup manifest is missing required or supported values.");
        }

        try
        {
            _ = Convert.FromHexString(manifest.DatabaseSha256);
        }
        catch (FormatException exception)
        {
            throw new InvalidDataException("The backup checksum is invalid.", exception);
        }

        databaseEntries[0].ExtractToFile(Path.Combine(stagingDirectory, DatabaseEntryName));
        return manifest;
    }

    private static async Task BackupDatabaseAsync(string sourcePath, string targetPath, CancellationToken cancellationToken)
    {
        await using var source = new SqliteConnection(ConnectionString(sourcePath, SqliteOpenMode.ReadOnly));
        await using var target = new SqliteConnection(ConnectionString(targetPath, SqliteOpenMode.ReadWriteCreate));
        await source.OpenAsync(cancellationToken).ConfigureAwait(false);
        await target.OpenAsync(cancellationToken).ConfigureAwait(false);
        source.BackupDatabase(target);
    }

    private static async Task EnsureIntegrityAsync(string databasePath, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(ConnectionString(databasePath, SqliteOpenMode.ReadOnly));
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA integrity_check;";
        var result = Convert.ToString(
            await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false),
            CultureInfo.InvariantCulture);
        if (!string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("SQLite integrity validation failed.");
        }
    }

    private static async Task<string> ReadSchemaVersionAsync(string databasePath, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(ConnectionString(databasePath, SqliteOpenMode.ReadOnly));
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT MigrationId FROM __EFMigrationsHistory ORDER BY MigrationId DESC LIMIT 1;";
        try
        {
            return Convert.ToString(
                    await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false),
                    CultureInfo.InvariantCulture)
                ?? "unversioned";
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 1)
        {
            return "unversioned";
        }
    }

    private static async Task SmokeReadAsync(string databasePath, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(ConnectionString(databasePath, SqliteOpenMode.ReadOnly));
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Projects;";
        _ = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<string> ComputeSha256Async(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false));
    }

    private static string ConnectionString(string path, SqliteOpenMode mode) =>
        new SqliteConnectionStringBuilder { DataSource = Path.GetFullPath(path), Mode = mode, Pooling = false }.ToString();

    private static string CreateStagingDirectory(string operation)
    {
        var path = Path.Combine(Path.GetTempPath(), "SASD-PIMS", operation, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string CreateRestoreStagingDirectory(string activeDatabase)
    {
        var databaseDirectory = Path.GetDirectoryName(activeDatabase)
            ?? throw new InvalidOperationException("The active database has no parent directory.");
        var path = Path.Combine(databaseDirectory, ".restore-staging-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static void TryDeleteFile(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    private static void TryDeleteDirectory(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}
