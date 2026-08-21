using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Domain.Projects;
using Sasd.Pims.Application.Requirements;
using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.Application.Projects;

/// <summary>Adds a concrete obstacle to an existing project without creating task semantics.</summary>
public sealed class AddProjectBlocker(IProjectRepository projects, IProjectBlockerRepository blockers,
    IExternalReferenceRepository references, TimeProvider timeProvider, OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<ProjectBlockerDto>> ExecuteAsync(Guid projectId, string? summary,
        string? details, string? cause, string? impact, string? affectedObject, string? nextAction,
        Guid? externalTaskReferenceId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await projects.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false) is null)
                return ProjectOperationResult.NotFound<ProjectBlockerDto>();
            if (externalTaskReferenceId is not null)
            {
                var reference = await references.GetByIdAsync(externalTaskReferenceId.Value, cancellationToken)
                    .ConfigureAwait(false);
                if (reference is null || reference.ProjectId != projectId || reference.Type != ExternalReferenceType.ExternalTask)
                    return ProjectOperationResult.ValidationFailed<ProjectBlockerDto>(
                        [new(nameof(externalTaskReferenceId), "BlockerExternalTaskReferenceInvalid",
                            "The external task reference must exist, have type ExternalTask and belong to the same Project.")]);
            }
            var blocker = ProjectBlocker.Create(Guid.NewGuid(), projectId, summary, details, cause, impact,
                affectedObject, nextAction, externalTaskReferenceId, timeProvider.GetUtcNow());
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
