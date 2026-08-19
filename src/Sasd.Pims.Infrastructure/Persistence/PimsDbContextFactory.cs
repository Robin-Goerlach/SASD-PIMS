using Microsoft.EntityFrameworkCore;

namespace Sasd.Pims.Infrastructure.Persistence;

public sealed class PimsDbContextFactory(string databasePath) : IDbContextFactory<PimsDbContext>
{
    private readonly string _databasePath = Path.GetFullPath(databasePath);

    public PimsDbContext CreateDbContext()
    {
        return new PimsDbContext(CreateOptions());
    }

    public Task<PimsDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CreateDbContext());
    }

    private DbContextOptions<PimsDbContext> CreateOptions()
    {
        var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            ForeignKeys = true,
            DefaultTimeout = 5,
        }.ToString();

        return new DbContextOptionsBuilder<PimsDbContext>()
            .UseSqlite(connectionString)
            .Options;
    }
}
