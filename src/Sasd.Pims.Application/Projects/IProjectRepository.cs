using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Projects;

/// <summary>Persists project aggregates while enforcing identity, uniqueness and revision conflicts.</summary>
public interface IProjectRepository
{
    Task<ProjectWriteResult> AddAsync(Project project, CancellationToken cancellationToken);

    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stores an aggregate mutation only when <paramref name="expectedRevision"/> still matches the database row.
    /// </summary>
    Task<ProjectWriteResult> UpdateAsync(
        Project project,
        int expectedRevision,
        CancellationToken cancellationToken);
}
