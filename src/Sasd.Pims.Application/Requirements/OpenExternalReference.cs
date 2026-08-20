using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Application.Projects;

namespace Sasd.Pims.Application.Requirements;

/// <summary>Revalidates a reference and opens it without probing HTTPS or deleting unreachable targets.</summary>
public sealed class OpenExternalReference(IExternalReferenceRepository references,
    IExternalReferenceOpener opener, OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<ExternalReferenceDto>> ExecuteAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reference = await references.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (reference is null) return ProjectOperationResult.NotFound<ExternalReferenceDto>();
            var validation = ReferenceTargetValidator.Validate(reference.Type, reference.Target);
            if (!validation.IsValid) return ProjectOperationResult.ValidationFailed<ExternalReferenceDto>(
                [new("Target", validation.ErrorCode!, validation.ErrorMessage!)]);
            if (IsLocal(validation.NormalizedTarget!) && !PathExists(reference.Type, validation.NormalizedTarget!))
                return ProjectOperationResult.ValidationFailed<ExternalReferenceDto>([new("Target",
                    "ReferenceTargetMissing", "Das lokale Referenzziel ist nicht mehr vorhanden; die Referenz bleibt gespeichert.")]);
            await opener.OpenAsync(validation.NormalizedTarget!, cancellationToken).ConfigureAwait(false);
            return ProjectOperationResult.Success(ExternalReferenceDto.FromDomain(reference));
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        { return failureHandler.Report<ExternalReferenceDto>("OpenExternalReference", exception); }
    }

    private static bool IsLocal(string target) => !target.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    private static bool PathExists(Sasd.Pims.Domain.Requirements.ExternalReferenceType type, string target) =>
        type is Sasd.Pims.Domain.Requirements.ExternalReferenceType.LocalDirectory or Sasd.Pims.Domain.Requirements.ExternalReferenceType.Repository
            ? Directory.Exists(target) : File.Exists(target);
}
