using System.Diagnostics;
using Sasd.Pims.Application.Requirements;

namespace Sasd.Pims.Infrastructure.References;

/// <summary>Uses the Windows shell only after Application-level validation has succeeded.</summary>
public sealed class WindowsExternalReferenceOpener : IExternalReferenceOpener
{
    public Task OpenAsync(string target, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
        return Task.CompletedTask;
    }
}
