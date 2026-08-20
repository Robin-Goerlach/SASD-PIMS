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
            .Include(project => project.Tags)
            .SingleOrDefaultAsync(project => project.Id == id, cancellationToken)
            .ConfigureAwait(false);

        return record is null ? null : ToDomain(record);
    }

    public async Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var records = await context.Projects
            .AsNoTracking()
            .Include(project => project.Tags)
            .OrderBy(project => project.Key)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return records.Select(ToDomain).ToArray();
    }

    public async Task<ProjectWriteResult> UpdateAsync(
        Project project,
        int expectedRevision,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
        var record = await context.Projects.Include(item => item.Tags)
            .SingleOrDefaultAsync(item => item.Id == project.Id, cancellationToken).ConfigureAwait(false);
        if (record is null || record.Revision != expectedRevision)
        {
            return ProjectWriteResult.ConcurrencyConflict;
        }

        CopyEditableValues(project, record);
        // Replacing this small owned classification set keeps persistence straightforward while the project
        // revision remains the single aggregate-level concurrency boundary.
        context.ProjectTags.RemoveRange(record.Tags);
        record.Tags = project.Tags.Select(value => new ProjectTagRecord { ProjectId = project.Id, Value = value }).ToList();
        try
        {
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
            return ProjectWriteResult.Saved;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
            return ProjectWriteResult.ConcurrencyConflict;
        }
    }

    private static ProjectRecord ToRecord(Project project) =>
        new()
        {
            Id = project.Id,
            Key = project.Key.Value,
            Name = project.Name,
            ShortDescription = project.ShortDescription,
            Goal = project.Goal,
            Benefit = project.Benefit,
            ProjectType = project.ProjectType,
            ProjectArea = project.ProjectArea,
            Responsibility = project.Responsibility,
            IsArchived = project.IsArchived,
            CreatedAtUtc = project.CreatedAtUtc,
            ModifiedAtUtc = project.ModifiedAtUtc,
            Revision = project.Revision,
            Tags = project.Tags.Select(value => new ProjectTagRecord { ProjectId = project.Id, Value = value }).ToList(),
        };

    private static void CopyEditableValues(Project project, ProjectRecord record)
    {
        record.Name = project.Name;
        record.ShortDescription = project.ShortDescription;
        record.Goal = project.Goal;
        record.Benefit = project.Benefit;
        record.ProjectType = project.ProjectType;
        record.ProjectArea = project.ProjectArea;
        record.Responsibility = project.Responsibility;
        record.IsArchived = project.IsArchived;
        record.ModifiedAtUtc = project.ModifiedAtUtc;
        record.Revision = project.Revision;
    }

    private static Project ToDomain(ProjectRecord record) =>
        Project.Reconstitute(
            record.Id,
            record.Key,
            record.Name,
            record.ShortDescription,
            record.Goal,
            record.Benefit,
            record.ProjectType,
            record.ProjectArea,
            record.Responsibility,
            record.Tags.Select(tag => tag.Value),
            record.IsArchived,
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
