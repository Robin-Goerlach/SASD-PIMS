using Sasd.Pims.Application.Projects;
using Sasd.Pims.Domain.Projects;
using Sasd.Pims.Domain.Requirements;

namespace Sasd.Pims.Application.Requirements;

internal static class RequirementUseCaseSupport
{
    public static AcceptanceCriterion[] BuildCriteria(Guid requirementId,
        IReadOnlyList<SaveAcceptanceCriterionCommand> commands) => commands.Select((item, index) =>
        AcceptanceCriterion.Create(item.Id ?? Guid.NewGuid(), requirementId, index + 1, item.Text,
            item.VerificationReferenceId)).ToArray();

    public static async Task<ProjectOperationResult<T>?> ValidateReferencesAsync<T>(Guid projectId,
        Guid? sourceReferenceId, IEnumerable<AcceptanceCriterion> criteria,
        IExternalReferenceRepository references, CancellationToken cancellationToken)
    {
        var ids = criteria.Select(item => item.VerificationReferenceId).Append(sourceReferenceId)
            .Where(item => item.HasValue).Select(item => item!.Value).Distinct();
        foreach (var id in ids)
        {
            var reference = await references.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (reference is null || reference.ProjectId != projectId)
                return ProjectOperationResult.ValidationFailed<T>([new("Reference", "CrossProjectReference",
                    "Reference must exist in the same Project.")]);
        }
        return null;
    }

    public static ProjectOperationResult<T> DomainFailure<T>(DomainValidationException exception) =>
        ProjectOperationResult.ValidationFailed<T>(exception.Errors
            .Select(item => new ProjectValidationError(item.Field, item.Code, item.Message)).ToArray());
}
