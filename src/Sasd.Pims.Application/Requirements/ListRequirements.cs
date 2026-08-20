using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Application.Projects;

namespace Sasd.Pims.Application.Requirements;

/// <summary>Lists Requirements in stable project-local key order.</summary>
public sealed class ListRequirements(IRequirementRepository requirements, OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<IReadOnlyList<RequirementDto>>> ExecuteAsync(Guid projectId,
        CancellationToken cancellationToken = default)
    {
        try { return ProjectOperationResult.Success<IReadOnlyList<RequirementDto>>(
            (await requirements.ListByProjectAsync(projectId, cancellationToken).ConfigureAwait(false))
                .Select(RequirementDto.FromDomain).ToArray()); }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        { return failureHandler.Report<IReadOnlyList<RequirementDto>>("ListRequirements", exception); }
    }
}
