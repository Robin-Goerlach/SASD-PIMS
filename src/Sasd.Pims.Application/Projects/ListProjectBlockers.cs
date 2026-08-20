using Sasd.Pims.Application.Diagnostics;

namespace Sasd.Pims.Application.Projects;

/// <summary>Returns open and resolved blockers so resolution history remains visible.</summary>
public sealed class ListProjectBlockers(IProjectBlockerRepository repository, OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<IReadOnlyList<ProjectBlockerDto>>> ExecuteAsync(Guid projectId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var blockers = await repository.ListByProjectAsync(projectId, cancellationToken).ConfigureAwait(false);
            return ProjectOperationResult.Success<IReadOnlyList<ProjectBlockerDto>>(
                blockers.Select(ProjectBlockerDto.FromDomain).ToArray());
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            return failureHandler.Report<IReadOnlyList<ProjectBlockerDto>>("ListProjectBlockers", exception);
        }
    }
}
