namespace Sasd.Pims.Infrastructure.Recovery;

public sealed record BackupManifest(
    string BackupFormatVersion,
    string ApplicationVersion,
    string SchemaVersion,
    DateTimeOffset CreatedAtUtc,
    string DatabaseFileName,
    string DatabaseSha256);
