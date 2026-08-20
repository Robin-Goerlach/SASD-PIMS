using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Application.Projects;

namespace Sasd.Pims.Application.Requirements;

/// <summary>Lists Project references and optionally narrows them to one Requirement owner.</summary>
public sealed class ListExternalReferences(IExternalReferenceRepository references, OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<IReadOnlyList<ExternalReferenceDto>>> ExecuteAsync(Guid projectId,
        Guid? requirementId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await references.ListByProjectAsync(projectId, cancellationToken).ConfigureAwait(false);
            return ProjectOperationResult.Success<IReadOnlyList<ExternalReferenceDto>>(items
                .Where(item => requirementId is null || item.RequirementId == requirementId)
                .Select(ExternalReferenceDto.FromDomain).ToArray());
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        { return failureHandler.Report<IReadOnlyList<ExternalReferenceDto>>("ListExternalReferences", exception); }
    }
}
