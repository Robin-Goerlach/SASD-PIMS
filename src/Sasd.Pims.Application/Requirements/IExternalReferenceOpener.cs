namespace Sasd.Pims.Application.Requirements;

/// <summary>Delegates a previously validated target to the platform handler.</summary>
public interface IExternalReferenceOpener
{
    Task OpenAsync(string target, CancellationToken cancellationToken);
}
