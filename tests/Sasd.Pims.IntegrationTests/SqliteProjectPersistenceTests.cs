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
        Assert.Contains("202608200001_ProjectCatalog", appliedMigrations);
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
    public async Task CompleteProjectMasterDataAndTagsSurviveRoundTrip()
    {
        await using var database = await SqliteTestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var project = Project.Create(Guid.NewGuid(), "CATALOG", "Catalog", "Short", "Goal", "Benefit",
            "SOFTWARE", "INTERNAL", "Team", ["desktop", "local"], Now);
        var repository = new SqliteProjectRepository(database.Factory);

        Assert.Equal(ProjectWriteResult.Saved,
            await repository.AddAsync(project, TestContext.Current.CancellationToken));
        var loaded = await repository.GetByIdAsync(project.Id, TestContext.Current.CancellationToken);

        Assert.Equal("Goal", loaded?.Goal);
        Assert.Equal("SOFTWARE", loaded?.ProjectType);
        Assert.Equal(["desktop", "local"], loaded?.Tags);
    }

    [Fact]
    public async Task StaleRepositoryUpdateCannotOverwriteNewerRevision()
    {
        await using var database = await SqliteTestDatabase.CreateAsync(TestContext.Current.CancellationToken);
        var repository = new SqliteProjectRepository(database.Factory);
        var original = Project.Create(Guid.NewGuid(), "CONFLICT", "Original", null, Now);
        await repository.AddAsync(original, TestContext.Current.CancellationToken);
        var firstEditor = await repository.GetByIdAsync(original.Id, TestContext.Current.CancellationToken);
        var staleEditor = await repository.GetByIdAsync(original.Id, TestContext.Current.CancellationToken);
        firstEditor!.UpdateDetails("First", null, Now.AddMinutes(1));
        staleEditor!.UpdateDetails("Stale", null, Now.AddMinutes(2));

        var firstResult = await repository.UpdateAsync(firstEditor, 1, TestContext.Current.CancellationToken);
        var staleResult = await repository.UpdateAsync(staleEditor, 1, TestContext.Current.CancellationToken);
        var loaded = await repository.GetByIdAsync(original.Id, TestContext.Current.CancellationToken);

        Assert.Equal(ProjectWriteResult.Saved, firstResult);
        Assert.Equal(ProjectWriteResult.ConcurrencyConflict, staleResult);
        Assert.Equal("First", loaded?.Name);
        Assert.Equal(2, loaded?.Revision);
    }

    [Fact]
    public async Task Version001DatabaseMigratesWithoutProjectLossAndAcceptsNewDataAfterReopen()
    {
        await using var database = await SqliteTestDatabase.CreateUnmigratedAsync();
        var existingId = Guid.NewGuid();
        await using (var context = database.Factory.CreateDbContext())
        {
            // Recreate the accepted 0.0.1 physical schema and migration marker exactly. Using SQL here avoids
            // asking the current model to create columns that did not exist in the historical application.
            await context.Database.ExecuteSqlRawAsync("""
                CREATE TABLE "__EFMigrationsHistory" (
                    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                    "ProductVersion" TEXT NOT NULL
                );
                INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                    VALUES ('202608190001_InitialProject', '10.0.10');
                CREATE TABLE "Projects" (
                    "Id" TEXT NOT NULL CONSTRAINT "PK_Projects" PRIMARY KEY,
                    "Key" TEXT NOT NULL,
                    "Name" TEXT NOT NULL,
                    "ShortDescription" TEXT NULL,
                    "CreatedAtUtc" TEXT NOT NULL,
                    "ModifiedAtUtc" TEXT NOT NULL,
                    "Revision" INTEGER NOT NULL
                );
                CREATE UNIQUE INDEX "IX_Projects_Key" ON "Projects" ("Key");
                """, TestContext.Current.CancellationToken);
            await context.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO Projects (Id, Key, Name, ShortDescription, CreatedAtUtc, ModifiedAtUtc, Revision)
                VALUES ({existingId}, {"P1-EXISTING"}, {"Existing"}, {"P1 data"}, {Now}, {Now}, {1})
                """, TestContext.Current.CancellationToken);
        }

        var backupDirectory = Path.Combine(database.RootPath, "migration-backups");
        await new DatabaseMigrator(database.Factory).MigrateAsync(backupDirectory, "0.1.0",
            TestContext.Current.CancellationToken);
        var repository = new SqliteProjectRepository(database.Factory);
        var existing = await repository.GetByIdAsync(existingId, TestContext.Current.CancellationToken);
        var added = Project.Create(Guid.NewGuid(), "NEW-01", "New", null, "Goal", null,
            null, null, null, ["new"], Now.AddMinutes(1));
        await repository.AddAsync(added, TestContext.Current.CancellationToken);

        var reopened = new SqliteProjectRepository(new PimsDbContextFactory(database.DatabasePath, pooling: false));
        var all = await reopened.ListAsync(TestContext.Current.CancellationToken);
        Assert.Equal("P1 data", existing?.ShortDescription);
        Assert.False(existing?.IsArchived);
        Assert.Equal(2, all.Count);
        Assert.Contains(all, project => project.Id == added.Id && project.Tags.SequenceEqual(["new"]));
        Assert.Single(Directory.GetFiles(backupDirectory, "*.zip"));
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
            var database = await CreateUnmigratedAsync();
            await new DatabaseMigrator(database.Factory).MigrateAsync(cancellationToken: cancellationToken);
            return database;
        }

        public static Task<SqliteTestDatabase> CreateUnmigratedAsync()
        {
            var root = Path.Combine(Path.GetTempPath(), "SASD-PIMS", "tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "data"));
            return Task.FromResult(new SqliteTestDatabase(root));
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
