using Microsoft.EntityFrameworkCore;
using Sasd.Pims.Infrastructure.Recovery;

namespace Sasd.Pims.Infrastructure.Persistence;

public sealed class DatabaseMigrator(IDbContextFactory<PimsDbContext> contextFactory)
{
    public async Task MigrateAsync(
        string? preMigrationBackupDirectory = null,
        string applicationVersion = "unknown",
        CancellationToken cancellationToken = default)
    {
        var databaseExisted = false;
        string databasePath;
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var connection = context.Database.GetDbConnection();
        databasePath = connection.DataSource;
        databaseExisted = File.Exists(databasePath);
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false);
        if (databaseExisted && pendingMigrations.Any())
        {
            if (string.IsNullOrWhiteSpace(preMigrationBackupDirectory))
            {
                throw new InvalidOperationException("A pre-migration backup directory is required for an existing database.");
            }

            var recovery = new SqliteRecoveryService();
            var backupPath = Path.Combine(
                preMigrationBackupDirectory,
                $"pre-migration-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.zip");
            await recovery.CreateAsync(databasePath, backupPath, applicationVersion, cancellationToken)
                .ConfigureAwait(false);
        }

        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        await SqliteRecoveryService.ValidateCurrentDatabaseAsync(databasePath, cancellationToken)
            .ConfigureAwait(false);
    }
}
