using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Projects;

/// <summary>Persists project blockers and exposes open-blocker facts needed by the catalog.</summary>
public interface IProjectBlockerRepository
{
    Task AddAsync(ProjectBlocker blocker, CancellationToken cancellationToken);
    Task<ProjectBlocker?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProjectBlocker>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken);
    Task<IReadOnlySet<Guid>> GetProjectIdsWithOpenBlockersAsync(CancellationToken cancellationToken);
    Task<bool> ResolveAsync(ProjectBlocker blocker, CancellationToken cancellationToken);
}
