using Microsoft.EntityFrameworkCore;

namespace Sasd.Pims.Infrastructure.Persistence;

public sealed class DatabaseMigrator(IDbContextFactory<PimsDbContext> contextFactory)
{
    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var connection = context.Database.GetDbConnection();
        var databasePath = connection.DataSource;
        var directory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await context.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
    }
}
