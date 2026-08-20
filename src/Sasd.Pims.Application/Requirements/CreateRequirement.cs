using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Application.Projects;
using Sasd.Pims.Domain.Projects;
using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.Application.Requirements;

/// <summary>Creates a project-bound Requirement with an automatically allocated immutable key.</summary>
public sealed class CreateRequirement(IProjectRepository projects, IRequirementRepository requirements,
    IExternalReferenceRepository references, OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<RequirementDto>> ExecuteAsync(Guid projectId,
        SaveRequirementCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await projects.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false) is null)
                return ProjectOperationResult.NotFound<RequirementDto>();
            if (command.DecisionStatus != RequirementDecisionStatus.Proposed)
                return ProjectOperationResult.ValidationFailed<RequirementDto>([new(nameof(command.DecisionStatus),
                    "NewRequirementMustBeProposed", "New Requirements must start as Proposed.")]);
            var id = Guid.NewGuid();
            var criteria = RequirementUseCaseSupport.BuildCriteria(id, command.AcceptanceCriteria);
            var referenceFailure = await RequirementUseCaseSupport.ValidateReferencesAsync<RequirementDto>(projectId,
                command.SourceReferenceId, criteria, references, cancellationToken).ConfigureAwait(false);
            if (referenceFailure is not null) return referenceFailure;
            var key = await requirements.GetNextKeyAsync(projectId, cancellationToken).ConfigureAwait(false);
            var requirement = Requirement.Create(id, projectId, key, command.Title, command.Description,
                command.Rationale, command.Priority, command.DecisionStatus, command.DecisionReason,
                command.SourceType, command.SourceDate, command.SourceSummary, command.SourceReferenceId, criteria);
            var write = await requirements.AddAsync(requirement, cancellationToken).ConfigureAwait(false);
            return write == RequirementWriteResult.Saved
                ? ProjectOperationResult.Success(RequirementDto.FromDomain(requirement))
                : ProjectOperationResult.ValidationFailed<RequirementDto>([new(nameof(Requirement.Key),
                    "RequirementKeyConflict", "Requirement key allocation conflicted; retry the operation.")]);
        }
        catch (DomainValidationException exception) { return RequirementUseCaseSupport.DomainFailure<RequirementDto>(exception); }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        { return failureHandler.Report<RequirementDto>("CreateRequirement", exception); }
    }
}
