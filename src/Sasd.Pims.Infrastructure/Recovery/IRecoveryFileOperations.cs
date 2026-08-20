namespace Sasd.Pims.Infrastructure.Recovery;

internal interface IRecoveryFileOperations
{
    void Move(string sourcePath, string destinationPath);

    void Replace(string sourcePath, string destinationPath, string backupPath);
}

internal sealed class RecoveryFileOperations : IRecoveryFileOperations
{
    public void Move(string sourcePath, string destinationPath) =>
        File.Move(sourcePath, destinationPath);

    public void Replace(string sourcePath, string destinationPath, string backupPath) =>
        File.Replace(sourcePath, destinationPath, backupPath);
}
