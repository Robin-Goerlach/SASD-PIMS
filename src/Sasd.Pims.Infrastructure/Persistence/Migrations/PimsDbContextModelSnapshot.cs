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

        modelBuilder.Entity<ExternalReferenceRecord>(entity =>
        {
            entity.Property(item => item.Id).ValueGeneratedNever().HasColumnType("TEXT");
            entity.Property(item => item.ProjectId).HasColumnType("TEXT");
            entity.Property(item => item.RequirementId).HasColumnType("TEXT");
            entity.Property(item => item.Type).HasConversion<string>().HasMaxLength(32).HasColumnType("TEXT");
            entity.Property(item => item.Title).IsRequired().HasColumnType("TEXT");
            entity.Property(item => item.Target).IsRequired().HasColumnType("TEXT");
            entity.Property(item => item.Revision).IsConcurrencyToken().HasColumnType("INTEGER");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.ProjectId);
            entity.HasIndex(item => item.RequirementId);
            entity.ToTable("ExternalReferences");
            entity.HasOne(item => item.Project).WithMany(project => project.ExternalReferences)
                .HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<RequirementRecord>().WithMany().HasForeignKey(item => item.RequirementId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RequirementRecord>(entity =>
        {
            entity.Property(item => item.Id).ValueGeneratedNever().HasColumnType("TEXT");
            entity.Property(item => item.ProjectId).HasColumnType("TEXT");
            entity.Property(item => item.Key).IsRequired().HasMaxLength(32).HasColumnType("TEXT");
            entity.Property(item => item.Title).IsRequired().HasColumnType("TEXT");
            entity.Property(item => item.Description).HasColumnType("TEXT");
            entity.Property(item => item.Rationale).HasColumnType("TEXT");
            entity.Property(item => item.Priority).HasConversion<string>().HasMaxLength(16).HasColumnType("TEXT");
            entity.Property(item => item.DecisionStatus).HasConversion<string>().HasMaxLength(16).HasColumnType("TEXT");
            entity.Property(item => item.DecisionReason).HasColumnType("TEXT");
            entity.Property(item => item.SourceType).HasConversion<string>().HasMaxLength(32).HasColumnType("TEXT");
            entity.Property(item => item.SourceDate).HasColumnType("TEXT");
            entity.Property(item => item.SourceSummary).HasColumnType("TEXT");
            entity.Property(item => item.SourceReferenceId).HasColumnType("TEXT");
            entity.Property(item => item.Revision).IsConcurrencyToken().HasColumnType("INTEGER");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => new { item.ProjectId, item.Key }).IsUnique();
            entity.HasIndex(item => item.SourceReferenceId);
            entity.ToTable("Requirements");
            entity.HasOne(item => item.Project).WithMany(project => project.Requirements)
                .HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ExternalReferenceRecord>().WithMany().HasForeignKey(item => item.SourceReferenceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AcceptanceCriterionRecord>(entity =>
        {
            entity.Property(item => item.Id).ValueGeneratedNever().HasColumnType("TEXT");
            entity.Property(item => item.RequirementId).HasColumnType("TEXT");
            entity.Property(item => item.Sequence).HasColumnType("INTEGER");
            entity.Property(item => item.Text).IsRequired().HasColumnType("TEXT");
            entity.Property(item => item.VerificationReferenceId).HasColumnType("TEXT");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => new { item.RequirementId, item.Sequence }).IsUnique();
            entity.HasIndex(item => item.VerificationReferenceId);
            entity.ToTable("AcceptanceCriteria");
            entity.HasOne(item => item.Requirement).WithMany(requirement => requirement.AcceptanceCriteria)
                .HasForeignKey(item => item.RequirementId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ExternalReferenceRecord>().WithMany().HasForeignKey(item => item.VerificationReferenceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
