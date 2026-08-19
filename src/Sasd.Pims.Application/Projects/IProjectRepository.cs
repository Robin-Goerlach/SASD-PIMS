using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Projects;

public interface IProjectRepository
{
    Task<ProjectWriteResult> AddAsync(Project project, CancellationToken cancellationToken);

    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
