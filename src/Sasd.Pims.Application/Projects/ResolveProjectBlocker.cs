using Sasd.Pims.Application.Diagnostics;
using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Application.Projects;

/// <summary>Resolves an open blocker once while retaining its evidence for project history.</summary>
public sealed class ResolveProjectBlocker(IProjectBlockerRepository repository, TimeProvider timeProvider,
    OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<ProjectBlockerDto>> ExecuteAsync(Guid blockerId, string? resolutionNote,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var blocker = await repository.GetByIdAsync(blockerId, cancellationToken).ConfigureAwait(false);
            if (blocker is null) return ProjectOperationResult.NotFound<ProjectBlockerDto>();
            blocker.Resolve(timeProvider.GetUtcNow(), resolutionNote);
            return await repository.ResolveAsync(blocker, cancellationToken).ConfigureAwait(false)
                ? ProjectOperationResult.Success(ProjectBlockerDto.FromDomain(blocker))
                : ProjectOperationResult.Conflict<ProjectBlockerDto>(new(string.Empty, "BlockerAlreadyResolved",
                    "The blocker has already been resolved by another operation."));
        }
        catch (DomainValidationException exception)
        {
            return ProjectOperationResult.ValidationFailed<ProjectBlockerDto>(exception.Errors
                .Select(error => new ProjectValidationError(error.Field, error.Code, error.Message)).ToArray());
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            return failureHandler.Report<ProjectBlockerDto>("ResolveProjectBlocker", exception);
        }
    }
}
