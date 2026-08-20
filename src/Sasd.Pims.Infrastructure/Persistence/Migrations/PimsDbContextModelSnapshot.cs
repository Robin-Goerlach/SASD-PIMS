using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sasd.Pims.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PimsDbContext))]
public sealed class PimsDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.10");

        modelBuilder.Entity<ProjectRecord>(entity =>
        {
            entity.Property(project => project.ActivityState).HasConversion<string>().HasMaxLength(32).HasColumnType("TEXT");
            entity.Property(project => project.Benefit).HasColumnType("TEXT");
            entity.Property(project => project.Id)
                .ValueGeneratedNever()
                .HasColumnType("TEXT");
            entity.Property(project => project.CreatedAtUtc).HasColumnType("TEXT");
            entity.Property(project => project.Key)
                .IsRequired()
                .HasMaxLength(64)
                .HasColumnType("TEXT");
            entity.Property(project => project.ModifiedAtUtc).HasColumnType("TEXT");
            entity.Property(project => project.LastReviewedAtUtc).HasColumnType("TEXT");
            entity.Property(project => project.NextReviewDueAtUtc).HasColumnType("TEXT");
            entity.Property(project => project.Name).IsRequired().HasColumnType("TEXT");
            entity.Property(project => project.Goal).HasColumnType("TEXT");
            entity.Property(project => project.IsArchived).HasColumnType("INTEGER");
            entity.Property(project => project.ProjectArea).HasMaxLength(32).HasColumnType("TEXT");
            entity.Property(project => project.Phase).HasConversion<string>().HasMaxLength(32).HasColumnType("TEXT");
            entity.Property(project => project.ProjectType).HasMaxLength(32).HasColumnType("TEXT");
            entity.Property(project => project.Responsibility).HasColumnType("TEXT");
            entity.Property(project => project.Revision)
                .IsConcurrencyToken()
                .HasColumnType("INTEGER");
            entity.Property(project => project.ShortDescription).HasColumnType("TEXT");
            entity.Property(project => project.TargetDate).HasColumnType("TEXT");
            entity.HasKey(project => project.Id);
            entity.HasIndex(project => project.Key).IsUnique();
            entity.ToTable("Projects");
        });

        modelBuilder.Entity<ProjectBlockerRecord>(entity =>
        {
            entity.Property(blocker => blocker.Id).ValueGeneratedNever().HasColumnType("TEXT");
            entity.Property(blocker => blocker.ProjectId).HasColumnType("TEXT");
            entity.Property(blocker => blocker.Summary).IsRequired().HasColumnType("TEXT");
            entity.Property(blocker => blocker.Details).HasColumnType("TEXT");
            entity.Property(blocker => blocker.CreatedAtUtc).HasColumnType("TEXT");
            entity.Property(blocker => blocker.ResolvedAtUtc).HasColumnType("TEXT");
            entity.Property(blocker => blocker.ResolutionNote).HasColumnType("TEXT");
            entity.HasKey(blocker => blocker.Id);
            entity.HasIndex(blocker => new { blocker.ProjectId, blocker.ResolvedAtUtc });
            entity.ToTable("ProjectBlockers");
            entity.HasOne(blocker => blocker.Project).WithMany(project => project.Blockers)
                .HasForeignKey(blocker => blocker.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectTagRecord>(entity =>
        {
            entity.Property(tag => tag.ProjectId).HasColumnType("TEXT");
            entity.Property(tag => tag.Value).IsRequired().HasMaxLength(32).HasColumnType("TEXT").UseCollation("NOCASE");
            entity.HasKey(tag => new { tag.ProjectId, tag.Value });
            entity.ToTable("ProjectTags");
            entity.HasOne(tag => tag.Project).WithMany(project => project.Tags)
                .HasForeignKey(tag => tag.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
