using Microsoft.EntityFrameworkCore;

namespace Sasd.Pims.Infrastructure.Persistence;

public sealed class PimsDbContext(DbContextOptions<PimsDbContext> options) : DbContext(options)
{
    internal DbSet<ProjectRecord> Projects => Set<ProjectRecord>();

    internal DbSet<ProjectTagRecord> ProjectTags => Set<ProjectTagRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var project = modelBuilder.Entity<ProjectRecord>();
        project.ToTable("Projects");
        project.HasKey(item => item.Id);
        project.Property(item => item.Id).ValueGeneratedNever();
        project.Property(item => item.Key).HasMaxLength(64).IsRequired();
        project.HasIndex(item => item.Key).IsUnique();
        project.Property(item => item.Name).IsRequired();
        project.Property(item => item.ProjectType).HasMaxLength(32);
        project.Property(item => item.ProjectArea).HasMaxLength(32);
        project.Property(item => item.Revision).IsConcurrencyToken();
        project.Property(item => item.CreatedAtUtc).IsRequired();
        project.Property(item => item.ModifiedAtUtc).IsRequired();

        var tag = modelBuilder.Entity<ProjectTagRecord>();
        tag.ToTable("ProjectTags");
        tag.HasKey(item => new { item.ProjectId, item.Value });
        tag.Property(item => item.Value).HasMaxLength(32).UseCollation("NOCASE").IsRequired();
        tag.HasOne(item => item.Project).WithMany(item => item.Tags).HasForeignKey(item => item.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
