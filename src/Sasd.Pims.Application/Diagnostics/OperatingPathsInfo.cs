namespace Sasd.Pims.Application.Diagnostics;

/// <summary>Describes fixed local operating paths and user-selected output semantics for read-only display.</summary>
public sealed record OperatingPathsInfo(string DatabaseFile, string LogDirectory, string ApplicationDirectory,
    string RecoveryDirectory)
{
    public const string UserSelectedBackupDestination = "Wird beim Sicherungsvorgang vom Benutzer gewählt.";
    public const string UserSelectedExportDestination = "Wird beim Exportvorgang vom Benutzer gewählt.";
}
