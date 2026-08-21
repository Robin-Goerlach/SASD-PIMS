namespace Sasd.Pims.Application.Auditing;

/// <summary>Reads the append-only, redacted business history without exposing mutation operations.</summary>
public interface IChangeEventReader
{
    Task<IReadOnlyList<ProjectChangeEventRow>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}
