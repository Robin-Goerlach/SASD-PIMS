using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Application.Projects;
using Sasd.Pims.Domain.Projects;
using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.Application.Requirements;

/// <summary>Creates or edits a reference after validating ownership, target shape and target-string secrets.</summary>
public sealed class SaveExternalReference(IProjectRepository projects, IRequirementRepository requirements,
    IExternalReferenceRepository references, OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<ExternalReferenceDto>> CreateAsync(Guid projectId, Guid? requirementId,
        SaveExternalReferenceCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var ownershipFailure = await ValidateOwnershipAsync(projectId, requirementId, cancellationToken).ConfigureAwait(false);
            if (ownershipFailure is not null) return ownershipFailure;
            var target = ReferenceTargetValidator.Validate(command.Type, command.Target);
            if (!target.IsValid) return Validation(target);
            var reference = ExternalReference.Create(Guid.NewGuid(), projectId, requirementId, command.Type,
                command.Title, target.NormalizedTarget);
            var write = await references.AddAsync(reference, cancellationToken).ConfigureAwait(false);
            return write == RequirementWriteResult.Saved ? ProjectOperationResult.Success(ExternalReferenceDto.FromDomain(reference))
                : ProjectOperationResult.Conflict<ExternalReferenceDto>(new("Revision", "ReferenceConflict", "Die Referenz wurde zwischenzeitlich geändert."));
        }
        catch (DomainValidationException exception) { return RequirementUseCaseSupport.DomainFailure<ExternalReferenceDto>(exception); }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        { return failureHandler.Report<ExternalReferenceDto>("CreateExternalReference", exception); }
    }

    public async Task<ProjectOperationResult<ExternalReferenceDto>> UpdateAsync(Guid id, int expectedRevision,
        SaveExternalReferenceCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await references.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (existing is null) return ProjectOperationResult.NotFound<ExternalReferenceDto>();
            var target = ReferenceTargetValidator.Validate(command.Type, command.Target);
            if (!target.IsValid) return Validation(target);
            var updated = existing.Update(command.Type, command.Title, target.NormalizedTarget);
            var write = await references.UpdateAsync(updated, expectedRevision, cancellationToken).ConfigureAwait(false);
            return write == RequirementWriteResult.Saved ? ProjectOperationResult.Success(ExternalReferenceDto.FromDomain(updated))
                : ProjectOperationResult.Conflict<ExternalReferenceDto>(new("Revision", "ReferenceConflict", "Die Referenz wurde zwischenzeitlich geändert."));
        }
        catch (DomainValidationException exception) { return RequirementUseCaseSupport.DomainFailure<ExternalReferenceDto>(exception); }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        { return failureHandler.Report<ExternalReferenceDto>("UpdateExternalReference", exception); }
    }

    private async Task<ProjectOperationResult<ExternalReferenceDto>?> ValidateOwnershipAsync(Guid projectId,
        Guid? requirementId, CancellationToken cancellationToken)
    {
        if (await projects.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false) is null)
            return ProjectOperationResult.NotFound<ExternalReferenceDto>();
        if (requirementId is null) return null;
        var requirement = await requirements.GetByIdAsync(requirementId.Value, cancellationToken).ConfigureAwait(false);
        return requirement is not null && requirement.ProjectId == projectId ? null
            : ProjectOperationResult.ValidationFailed<ExternalReferenceDto>([new("RequirementId",
                "ReferenceOwnershipMismatch", "Requirement reference must belong to the same Project.")]);
    }

    private static ProjectOperationResult<ExternalReferenceDto> Validation(ReferenceTargetValidationResult result) =>
        ProjectOperationResult.ValidationFailed<ExternalReferenceDto>([new("Target", result.ErrorCode!, result.ErrorMessage!)]);
}
