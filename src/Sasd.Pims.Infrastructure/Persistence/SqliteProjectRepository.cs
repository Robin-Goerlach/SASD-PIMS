using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sasd.Pims.Application.Projects;
using Sasd.Pims.Domain.Projects;

namespace Sasd.Pims.Infrastructure.Persistence;

public sealed class SqliteProjectRepository(IDbContextFactory<PimsDbContext> contextFactory) : IProjectRepository
{
    public async Task<ProjectWriteResult> AddAsync(Project project, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

        context.Projects.Add(ToRecord(project));
        try
        {
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return ProjectWriteResult.Saved;
        }
        catch (DbUpdateException exception) when (IsUniqueConstraint(exception))
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return ProjectWriteResult.DuplicateKey;
        }
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var record = await context.Projects
            .AsNoTracking()
            .SingleOrDefaultAsync(project => project.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return record is null ? null : ToDomain(record);
    }

    private static ProjectRecord ToRecord(Project project) =>
        new()
        {
            Id = project.Id,
            Key = project.Key.Value,
            Name = project.Name,
            ShortDescription = project.ShortDescription,
            CreatedAtUtc = project.CreatedAtUtc,
            ModifiedAtUtc = project.ModifiedAtUtc,
            Revision = project.Revision,
        };

    private static Project ToDomain(ProjectRecord record) =>
        Project.Reconstitute(
            record.Id,
            record.Key,
            record.Name,
            record.ShortDescription,
            record.CreatedAtUtc,
            record.ModifiedAtUtc,
            record.Revision);

    private static bool IsUniqueConstraint(DbUpdateException exception)
    {
        return exception.InnerException is SqliteException
        {
            SqliteErrorCode: 19,
            SqliteExtendedErrorCode: 2067,
        };
    }
}
