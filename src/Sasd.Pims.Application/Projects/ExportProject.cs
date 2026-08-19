using Sasd.Pims.Application.Diagnostics;

namespace Sasd.Pims.Application.Projects;

public sealed class ExportProject(
    IProjectRepository repository,
    IProjectExportWriter writer,
    TimeProvider timeProvider,
    OperationFailureHandler failureHandler)
{
    public async Task<ProjectOperationResult<string>> ExecuteAsync(
        Guid projectId,
        string targetPath,
        string applicationVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await repository.GetByIdAsync(projectId, cancellationToken).ConfigureAwait(false);
            if (project is null)
            {
                return ProjectOperationResult.NotFound<string>();
            }

            await writer.WriteAsync(
                    ProjectDto.FromDomain(project),
                    targetPath,
                    timeProvider.GetUtcNow(),
                    applicationVersion,
                    cancellationToken)
                .ConfigureAwait(false);
            return ProjectOperationResult.Success(targetPath);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            return failureHandler.Report<string>("ExportProject", exception);
        }
    }
}
