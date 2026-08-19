using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Sasd.Pims.Application.Projects;
using Sasd.Pims.Domain.Projects;
using Sasd.Pims.Infrastructure.Persistence;
using Sasd.Pims.Infrastructure.Recovery;
using Xunit;

namespace Sasd.Pims.IntegrationTests;

public sealed class SqliteRecoveryTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 19, 16, 0, 0, TimeSpan.Zero);
    private static readonly JsonSerializerOptions ManifestJsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task BackupContainsManifestAndMatchingDatabaseChecksum()
    {
        await using var fixture = await RecoveryFixture.CreateAsync(TestContext.Current.CancellationToken);
        var packagePath = Path.Combine(fixture.RootPath, "backup.zip");

        await fixture.Service.CreateAsync(
            fixture.DatabasePath,
            packagePath,
            "0.0.1-internal",
            TestContext.Current.CancellationToken);

        using var archive = ZipFile.OpenRead(packagePath);
        var databaseEntry = Assert.Single(archive.Entries, entry => entry.FullName == "pims.db");
        var manifestEntry = Assert.Single(archive.Entries, entry => entry.FullName == "manifest.json");
        await using var manifestStream = manifestEntry.Open();
        var manifest = await JsonSerializer.DeserializeAsync<BackupManifest>(
            manifestStream,
            ManifestJsonOptions,
            cancellationToken: TestContext.Current.CancellationToken);
        await using var databaseStream = databaseEntry.Open();
        var checksum = Convert.ToHexString(await SHA256.HashDataAsync(
            databaseStream,
            TestContext.Current.CancellationToken));

        Assert.NotNull(manifest);
        Assert.Equal(SqliteRecoveryService.BackupFormatVersion, manifest.BackupFormatVersion);
        Assert.Equal(SqliteRecoveryService.CurrentSchemaVersion, manifest.SchemaVersion);
        Assert.Equal("pims.db", manifest.DatabaseFileName);
        Assert.Equal(checksum, manifest.DatabaseSha256);
    }

    [Fact]
    public async Task BackupIsAConsistentReopenableSnapshot()
    {
        await using var fixture = await RecoveryFixture.CreateAsync(TestContext.Current.CancellationToken);
        var expected = await fixture.AddProjectAsync("BACKUP", TestContext.Current.CancellationToken);
        var packagePath = Path.Combine(fixture.RootPath, "backup.zip");
        await fixture.Service.CreateAsync(
            fixture.DatabasePath,
            packagePath,
            "0.0.1-internal",
            TestContext.Current.CancellationToken);

        var extractedDatabase = Path.Combine(fixture.RootPath, "snapshot.db");
        using (var archive = ZipFile.OpenRead(packagePath))
        {
            archive.GetEntry("pims.db")!.ExtractToFile(extractedDatabase);
        }

        var repository = new SqliteProjectRepository(new PimsDbContextFactory(extractedDatabase, pooling: false));
        var restored = await repository.GetByIdAsync(expected.Id, TestContext.Current.CancellationToken);
        Assert.NotNull(restored);
        Assert.Equal(expected.Key, restored.Key);
    }

    [Fact]
    public async Task ValidRestoreReplacesDatabaseAndCreatesVerifiableRollbackBackup()
    {
        await using var fixture = await RecoveryFixture.CreateAsync(TestContext.Current.CancellationToken);
        var backedUp = await fixture.AddProjectAsync("BEFORE", TestContext.Current.CancellationToken);
        var packagePath = Path.Combine(fixture.RootPath, "backup.zip");
        await fixture.Service.CreateAsync(
            fixture.DatabasePath,
            packagePath,
            "0.0.1-internal",
            TestContext.Current.CancellationToken);
        var replaced = await fixture.AddProjectAsync("AFTER", TestContext.Current.CancellationToken);

        var rollbackPackage = await fixture.Service.RestoreAsync(
            fixture.DatabasePath,
            packagePath,
            Path.Combine(fixture.RootPath, "rollback"),
            "0.0.1-internal",
            TestContext.Current.CancellationToken);

        var repository = new SqliteProjectRepository(
            new PimsDbContextFactory(fixture.DatabasePath, pooling: false));
        Assert.NotNull(await repository.GetByIdAsync(backedUp.Id, TestContext.Current.CancellationToken));
        Assert.Null(await repository.GetByIdAsync(replaced.Id, TestContext.Current.CancellationToken));
        Assert.True(File.Exists(rollbackPackage));

        var rollbackDatabase = Path.Combine(fixture.RootPath, "rollback.db");
        using (var archive = ZipFile.OpenRead(rollbackPackage))
        {
            archive.GetEntry("pims.db")!.ExtractToFile(rollbackDatabase);
        }

        var rollbackRepository = new SqliteProjectRepository(
            new PimsDbContextFactory(rollbackDatabase, pooling: false));
        Assert.NotNull(await rollbackRepository.GetByIdAsync(replaced.Id, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ChecksumMismatchIsRejectedBeforeActiveDatabaseChanges()
    {
        await using var fixture = await RecoveryFixture.CreateAsync(TestContext.Current.CancellationToken);
        var active = await fixture.AddProjectAsync("ACTIVE-CHECKSUM", TestContext.Current.CancellationToken);
        var packagePath = await fixture.CreateBackupAsync(TestContext.Current.CancellationToken);
        await RewritePackageAsync(
            packagePath,
            (database, manifest) => (database, manifest with { DatabaseSha256 = new string('0', 64) }));

        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Service.RestoreAsync(
            fixture.DatabasePath,
            packagePath,
            fixture.RollbackPath,
            "0.0.1-internal",
            TestContext.Current.CancellationToken));

        await fixture.AssertProjectExistsAsync(active.Id, TestContext.Current.CancellationToken);
        AssertRollbackDirectoryEmpty(fixture.RollbackPath);
    }

    [Fact]
    public async Task MissingDatabasePayloadIsRejectedBeforeActiveDatabaseChanges()
    {
        await using var fixture = await RecoveryFixture.CreateAsync(TestContext.Current.CancellationToken);
        var active = await fixture.AddProjectAsync("ACTIVE-MISSING", TestContext.Current.CancellationToken);
        var packagePath = await fixture.CreateBackupAsync(TestContext.Current.CancellationToken);
        using (var archive = ZipFile.Open(packagePath, ZipArchiveMode.Update))
        {
            archive.GetEntry("pims.db")!.Delete();
        }

        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Service.RestoreAsync(
            fixture.DatabasePath,
            packagePath,
            fixture.RollbackPath,
            "0.0.1-internal",
            TestContext.Current.CancellationToken));

        await fixture.AssertProjectExistsAsync(active.Id, TestContext.Current.CancellationToken);
        AssertRollbackDirectoryEmpty(fixture.RollbackPath);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IncompatibleManifestOrSchemaIsRejectedBeforeActiveDatabaseChanges(bool changeFormat)
    {
        await using var fixture = await RecoveryFixture.CreateAsync(TestContext.Current.CancellationToken);
        var active = await fixture.AddProjectAsync("ACTIVE-INCOMPATIBLE", TestContext.Current.CancellationToken);
        var packagePath = await fixture.CreateBackupAsync(TestContext.Current.CancellationToken);
        await RewritePackageAsync(
            packagePath,
            (database, manifest) => (
                database,
                changeFormat
                    ? manifest with { BackupFormatVersion = "99.0" }
                    : manifest with { SchemaVersion = "209901010001_Future" }));

        await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Service.RestoreAsync(
            fixture.DatabasePath,
            packagePath,
            fixture.RollbackPath,
            "0.0.1-internal",
            TestContext.Current.CancellationToken));

        await fixture.AssertProjectExistsAsync(active.Id, TestContext.Current.CancellationToken);
        AssertRollbackDirectoryEmpty(fixture.RollbackPath);
    }

    [Fact]
    public async Task CorruptSqlitePayloadIsNeverPromoted()
    {
        await using var fixture = await RecoveryFixture.CreateAsync(TestContext.Current.CancellationToken);
        var active = await fixture.AddProjectAsync("ACTIVE-INTEGRITY", TestContext.Current.CancellationToken);
        var packagePath = await fixture.CreateBackupAsync(TestContext.Current.CancellationToken);
        await RewritePackageAsync(
            packagePath,
            (database, manifest) =>
            {
                var corrupt = database[..Math.Min(database.Length, 128)];
                Random.Shared.NextBytes(corrupt);
                return (corrupt, manifest with { DatabaseSha256 = Convert.ToHexString(SHA256.HashData(corrupt)) });
            });

        await Assert.ThrowsAsync<SqliteException>(() => fixture.Service.RestoreAsync(
            fixture.DatabasePath,
            packagePath,
            fixture.RollbackPath,
            "0.0.1-internal",
            TestContext.Current.CancellationToken));

        await fixture.AssertProjectExistsAsync(active.Id, TestContext.Current.CancellationToken);
        AssertRollbackDirectoryEmpty(fixture.RollbackPath);
    }

    [Fact]
    public async Task ReplacementFailureRestoresPreviousDatabaseAndKeepsRollbackEvidence()
    {
        await using var fixture = await RecoveryFixture.CreateAsync(TestContext.Current.CancellationToken);
        var active = await fixture.AddProjectAsync("ACTIVE-REPLACE", TestContext.Current.CancellationToken);
        var packagePath = await fixture.CreateBackupAsync(TestContext.Current.CancellationToken);
        var failingService = new SqliteRecoveryService(new FailingCopyFileOperations());

        await Assert.ThrowsAsync<IOException>(() => failingService.RestoreAsync(
            fixture.DatabasePath,
            packagePath,
            fixture.RollbackPath,
            "0.0.1-internal",
            TestContext.Current.CancellationToken));

        await fixture.AssertProjectExistsAsync(active.Id, TestContext.Current.CancellationToken);
        Assert.Single(Directory.GetFiles(fixture.RollbackPath, "rollback-*.zip"));
    }

    private static async Task RewritePackageAsync(
        string packagePath,
        Func<byte[], BackupManifest, (byte[] Database, BackupManifest Manifest)> transform)
    {
        byte[] database;
        BackupManifest manifest;
        using (var archive = ZipFile.OpenRead(packagePath))
        {
            await using var databaseStream = archive.GetEntry("pims.db")!.Open();
            using var memory = new MemoryStream();
            await databaseStream.CopyToAsync(memory, TestContext.Current.CancellationToken);
            database = memory.ToArray();
            await using var manifestStream = archive.GetEntry("manifest.json")!.Open();
            manifest = (await JsonSerializer.DeserializeAsync<BackupManifest>(
                manifestStream,
                ManifestJsonOptions,
                TestContext.Current.CancellationToken))!;
        }

        var transformed = transform(database, manifest);
        File.Delete(packagePath);
        await using var output = new FileStream(packagePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var newArchive = new ZipArchive(output, ZipArchiveMode.Create);
        var databaseEntry = newArchive.CreateEntry("pims.db");
        await using (var databaseStream = databaseEntry.Open())
        {
            await databaseStream.WriteAsync(transformed.Database, TestContext.Current.CancellationToken);
        }

        var manifestEntry = newArchive.CreateEntry("manifest.json");
        await using var transformedManifestStream = manifestEntry.Open();
        await JsonSerializer.SerializeAsync(
            transformedManifestStream,
            transformed.Manifest,
            ManifestJsonOptions,
            TestContext.Current.CancellationToken);
    }

    private static void AssertRollbackDirectoryEmpty(string rollbackPath)
    {
        Assert.False(Directory.Exists(rollbackPath) && Directory.EnumerateFiles(rollbackPath).Any());
    }

    private sealed class RecoveryFixture : IAsyncDisposable
    {
        private RecoveryFixture(string rootPath)
        {
            RootPath = rootPath;
            DatabasePath = Path.Combine(rootPath, "data", "pims.db");
        }

        public string RootPath { get; }
        public string DatabasePath { get; }
        public string RollbackPath => Path.Combine(RootPath, "rollback");
        public SqliteRecoveryService Service { get; } = new();

        public static async Task<RecoveryFixture> CreateAsync(CancellationToken cancellationToken)
        {
            var fixture = new RecoveryFixture(Path.Combine(
                Path.GetTempPath(),
                "SASD-PIMS",
                "tests",
                Guid.NewGuid().ToString("N")));
            await new DatabaseMigrator(new PimsDbContextFactory(fixture.DatabasePath, pooling: false))
                .MigrateAsync(cancellationToken: cancellationToken);
            return fixture;
        }

        public async Task<Project> AddProjectAsync(string key, CancellationToken cancellationToken)
        {
            var project = Project.Create(Guid.NewGuid(), key, key + " project", null, Now);
            var repository = new SqliteProjectRepository(
                new PimsDbContextFactory(DatabasePath, pooling: false));
            Assert.Equal(ProjectWriteResult.Saved, await repository.AddAsync(project, cancellationToken));
            return project;
        }

        public async Task<string> CreateBackupAsync(CancellationToken cancellationToken)
        {
            var packagePath = Path.Combine(RootPath, $"backup-{Guid.NewGuid():N}.zip");
            await Service.CreateAsync(DatabasePath, packagePath, "0.0.1-internal", cancellationToken);
            return packagePath;
        }

        public async Task AssertProjectExistsAsync(Guid projectId, CancellationToken cancellationToken)
        {
            var repository = new SqliteProjectRepository(
                new PimsDbContextFactory(DatabasePath, pooling: false));
            Assert.NotNull(await repository.GetByIdAsync(projectId, cancellationToken));
        }

        public ValueTask DisposeAsync()
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            if (Directory.Exists(RootPath))
            {
                Directory.Delete(RootPath, true);
            }

            return ValueTask.CompletedTask;
        }
    }

    private sealed class FailingCopyFileOperations : IRecoveryFileOperations
    {
        public void Copy(string sourcePath, string destinationPath) =>
            throw new IOException("Synthetic replacement failure.");

        public void Move(string sourcePath, string destinationPath) =>
            File.Move(sourcePath, destinationPath);
    }
}
