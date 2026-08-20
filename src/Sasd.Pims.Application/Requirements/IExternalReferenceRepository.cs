using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.Application.Requirements;

/// <summary>Persists manually maintained Project and Requirement references.</summary>
public interface IExternalReferenceRepository
{
    Task<RequirementWriteResult> AddAsync(ExternalReference reference, CancellationToken cancellationToken);
    Task<RequirementWriteResult> UpdateAsync(ExternalReference reference, int expectedRevision, CancellationToken cancellationToken);
    Task<ExternalReference?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ExternalReference>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken);
}
