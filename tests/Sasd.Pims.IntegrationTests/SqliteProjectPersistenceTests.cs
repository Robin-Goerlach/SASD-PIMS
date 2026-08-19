using Microsoft.EntityFrameworkCore;
using Sasd.Pims.Application.Projects;
using Sasd.Pims.Domain.Projects;
using Sasd.Pims.Infrastructure.Persistence;
using Xunit;

namespace Sasd.Pims.IntegrationTests;

public sealed class SqliteProjectPersistenceTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 19, 14, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CleanDatabaseMigratesToCurrentSchema()
    {
        await using var database = await SqliteTestDatabase.CreateAsync(TestContext.Current.CancellationToken);

        await using var context = database.Factory.CreateDbContext();
        var appliedMigrations = await context.Database
            .GetAppliedMigrationsAsync(TestContext.Current.CancellationToken);

        Assert.Contains("202608190001_InitialProject", appliedMigrations);
        Assert.True(File.Exists(database.DatabasePath));
    }

    [Fact]
    public async Task ProjectRoundTripSurvivesCompleteContextReconstruction()
    {
        await using var database = await SqliteTestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var project = Project.Create(Guid.NewGuid(), "SQLITE-DEMO", "SQLite demo", "Synthetic", Now);

        var writeRepository = new SqliteProjectRepository(database.Factory);
        var writeResult = await writeRepository.AddAsync(project, TestContext.Current.CancellationToken);

        var reconstructedFactory = new PimsDbContextFactory(database.DatabasePath, pooling: false);
        var readRepository = new SqliteProjectRepository(reconstructedFactory);
        var loaded = await readRepository.GetByIdAsync(project.Id, TestContext.Current.CancellationToken);

        Assert.Equal(ProjectWriteResult.Saved, writeResult);
        Assert.NotNull(loaded);
        Assert.Equal(project.Id, loaded.Id);
        Assert.Equal(project.Key, loaded.Key);
        Assert.Equal(project.Name, loaded.Name);
        Assert.Equal(project.ShortDescription, loaded.ShortDescription);
        Assert.Equal(project.CreatedAtUtc, loaded.CreatedAtUtc);
        Assert.Equal(project.ModifiedAtUtc, loaded.ModifiedAtUtc);
        Assert.Equal(project.Revision, loaded.Revision);
    }

    [Fact]
    public async Task DuplicateNormalisedKeyIsRejected()
    {
        await using var database = await SqliteTestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var repository = new SqliteProjectRepository(database.Factory);
        var first = Project.Create(Guid.NewGuid(), "DUPLICATE", "First", null, Now);
        var second = Project.Create(Guid.NewGuid(), "duplicate", "Second", null, Now.AddMinutes(1));

        var firstResult = await repository.AddAsync(first, TestContext.Current.CancellationToken);
        var secondResult = await repository.AddAsync(second, TestContext.Current.CancellationToken);

        Assert.Equal(ProjectWriteResult.Saved, firstResult);
        Assert.Equal(ProjectWriteResult.DuplicateKey, secondResult);
    }

    [Fact]
    public async Task MigratingCurrentDatabaseIsANonDestructiveNoOp()
    {
        await using var database = await SqliteTestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var repository = new SqliteProjectRepository(database.Factory);
        var project = Project.Create(Guid.NewGuid(), "MIGRATION-NOOP", "Migration no-op", null, Now);
        Assert.Equal(
            ProjectWriteResult.Saved,
            await repository.AddAsync(project, TestContext.Current.CancellationToken));

        await new DatabaseMigrator(database.Factory).MigrateAsync(cancellationToken: TestContext.Current.CancellationToken);
        var loaded = await repository.GetByIdAsync(project.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(loaded);
        Assert.Equal(project.Id, loaded.Id);
    }

    private sealed class SqliteTestDatabase : IAsyncDisposable
    {
        private SqliteTestDatabase(string rootPath)
        {
            RootPath = rootPath;
            DatabasePath = Path.Combine(rootPath, "data", "pims.db");
            Factory = new PimsDbContextFactory(DatabasePath, pooling: false);
        }

        public string RootPath { get; }

        public string DatabasePath { get; }

        public PimsDbContextFactory Factory { get; }

        public static async Task<SqliteTestDatabase> CreateAsync(CancellationToken cancellationToken)
        {
            var root = Path.Combine(Path.GetTempPath(), "SASD-PIMS", "tests", Guid.NewGuid().ToString("N"));
            var database = new SqliteTestDatabase(root);
            await new DatabaseMigrator(database.Factory).MigrateAsync(cancellationToken: cancellationToken);
            return database;
        }

        public ValueTask DisposeAsync()
        {
            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, recursive: true);
            }

            return ValueTask.CompletedTask;
        }
    }
}
