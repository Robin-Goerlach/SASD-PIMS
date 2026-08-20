using Sasd.Pims.Domain.Auditing;

namespace Sasd.Pims.Application.Auditing;

public interface IChangeEventReader
{
    Task<IReadOnlyList<ChangeEvent>> ListByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}
