using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Projects;

/// <summary>Adds a concrete obstacle to an existing project without creating task semantics.</summary>
public sealed class AddProjectBlocker(IProjectRepository projects, IProjectBlockerRepository blockers,
    TimeProvider timeProvider, OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<ProjectBlockerDto>> ExecuteAsync(Guid projectId, string? summary,
        string? details, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await projects.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false) is null)
                return ProjectOperationResult.NotFound<ProjectBlockerDto>();
            var blocker = ProjectBlocker.Create(Guid.NewGuid(), projectId, summary, details, timeProvider.GetUtcNow());
            await blockers.AddAsync(blocker, cancellationToken).ConfigureAwait(false);
            return ProjectOperationResult.Success(ProjectBlockerDto.FromDomain(blocker));
        }
        catch (DomainValidationException exception)
        {
            return ProjectOperationResult.ValidationFailed<ProjectBlockerDto>(exception.Errors
                .Select(error => new ProjectValidationError(error.Field, error.Code, error.Message)).ToArray());
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            return failureHandler.Report<ProjectBlockerDto>("AddProjectBlocker", exception);
        }
    }
}
