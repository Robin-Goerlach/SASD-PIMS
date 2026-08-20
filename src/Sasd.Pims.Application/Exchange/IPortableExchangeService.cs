namespace Sasd.Pims.Application.Exchange;

/// <summary>Writes public exchange artifacts without exposing persistence or filesystem details to UI code.</summary>
public interface IPortableExchangeService
{
    Task ExportJsonAsync(string targetPath, string productVersion, DateTimeOffset exportedAtUtc,
        CancellationToken cancellationToken = default);

    Task ExportProjectMarkdownAsync(Guid projectId, string targetPath, DateTimeOffset exportedAtUtc,
        CancellationToken cancellationToken = default);
}
