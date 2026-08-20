using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Application.Projects;
using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Requirements;

/// <summary>Updates editable Requirement facts while preserving identity, Project and key.</summary>
public sealed class UpdateRequirement(IRequirementRepository requirements, IExternalReferenceRepository references,
    OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<RequirementDto>> ExecuteAsync(Guid id, int expectedRevision,
        SaveRequirementCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await requirements.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (existing is null) return ProjectOperationResult.NotFound<RequirementDto>();
            var criteria = RequirementUseCaseSupport.BuildCriteria(id, command.AcceptanceCriteria);
            var referenceFailure = await RequirementUseCaseSupport.ValidateReferencesAsync<RequirementDto>(existing.ProjectId,
                command.SourceReferenceId, criteria, references, cancellationToken).ConfigureAwait(false);
            if (referenceFailure is not null) return referenceFailure;
            var updated = existing.Update(command.Title, command.Description, command.Rationale, command.Priority,
                command.DecisionStatus, command.DecisionReason, command.SourceType, command.SourceDate,
                command.SourceSummary, command.SourceReferenceId, criteria);
            var write = await requirements.UpdateAsync(updated, expectedRevision, cancellationToken).ConfigureAwait(false);
            return write == RequirementWriteResult.Saved ? ProjectOperationResult.Success(RequirementDto.FromDomain(updated))
                : ProjectOperationResult.Conflict<RequirementDto>(new("Revision", "RequirementConflict", "Die Anforderung wurde zwischenzeitlich geändert."));
        }
        catch (DomainValidationException exception) { return RequirementUseCaseSupport.DomainFailure<RequirementDto>(exception); }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        { return failureHandler.Report<RequirementDto>("UpdateRequirement", exception); }
    }
}
