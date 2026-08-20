namespace Sasd.Pims.Application.Traceability;

public interface ITraceabilityReader
{
    Task<TraceabilityNode?> GetProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}
