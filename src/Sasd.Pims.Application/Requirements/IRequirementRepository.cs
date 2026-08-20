using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.Application.Requirements;

/// <summary>Persists Requirement aggregates with project-local key allocation.</summary>
public interface IRequirementRepository
{
    Task<string> GetNextKeyAsync(Guid projectId, CancellationToken cancellationToken);
    Task<RequirementWriteResult> AddAsync(Requirement requirement, CancellationToken cancellationToken);
    Task<RequirementWriteResult> UpdateAsync(Requirement requirement, int expectedRevision, CancellationToken cancellationToken);
    Task<Requirement?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Requirement>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken);
}
