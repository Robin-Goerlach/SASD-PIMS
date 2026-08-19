namespace Sasd.Pims.Infrastructure.Recovery;

internal interface IRecoveryFileOperations
{
    void Copy(string sourcePath, string destinationPath);

    void Move(string sourcePath, string destinationPath);
}

internal sealed class RecoveryFileOperations : IRecoveryFileOperations
{
    public void Copy(string sourcePath, string destinationPath) =>
        File.Copy(sourcePath, destinationPath, false);

    public void Move(string sourcePath, string destinationPath) =>
        File.Move(sourcePath, destinationPath);
}
