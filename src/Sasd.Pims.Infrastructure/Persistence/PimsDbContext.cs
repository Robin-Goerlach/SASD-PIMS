using Microsoft.EntityFrameworkCore;

namespace Sasd.Pims.Infrastructure.Persistence;

public sealed class PimsDbContext(DbContextOptions<PimsDbContext> options) : DbContext(options)
{
    internal DbSet<ProjectRecord> Projects => Set<ProjectRecord>();

    internal DbSet<ProjectTagRecord> ProjectTags => Set<ProjectTagRecord>();

    internal DbSet<ProjectBlockerRecord> ProjectBlockers => Set<ProjectBlockerRecord>();

    internal DbSet<RequirementRecord> Requirements => Set<RequirementRecord>();

    internal DbSet<AcceptanceCriterionRecord> AcceptanceCriteria => Set<AcceptanceCriterionRecord>();

    internal DbSet<ExternalReferenceRecord> ExternalReferences => Set<ExternalReferenceRecord>();

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
        project.Property(item => item.Phase).HasConversion<string>().HasMaxLength(32).IsRequired();
        project.Property(item => item.ActivityState).HasConversion<string>().HasMaxLength(32).IsRequired();

        var tag = modelBuilder.Entity<ProjectTagRecord>();
        tag.ToTable("ProjectTags");
        tag.HasKey(item => new { item.ProjectId, item.Value });
        tag.Property(item => item.Value).HasMaxLength(32).UseCollation("NOCASE").IsRequired();
        tag.HasOne(item => item.Project).WithMany(item => item.Tags).HasForeignKey(item => item.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        var blocker = modelBuilder.Entity<ProjectBlockerRecord>();
        blocker.ToTable("ProjectBlockers");
        blocker.HasKey(item => item.Id);
        blocker.Property(item => item.Id).ValueGeneratedNever();
        blocker.Property(item => item.Summary).IsRequired();
        blocker.Property(item => item.CreatedAtUtc).IsRequired();
        blocker.HasIndex(item => new { item.ProjectId, item.ResolvedAtUtc });
        blocker.HasOne(item => item.Project).WithMany(item => item.Blockers).HasForeignKey(item => item.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        var requirement = modelBuilder.Entity<RequirementRecord>();
        requirement.ToTable("Requirements");
        requirement.HasKey(item => item.Id);
        requirement.Property(item => item.Id).ValueGeneratedNever();
        requirement.Property(item => item.Key).HasMaxLength(32).IsRequired();
        requirement.Property(item => item.Title).IsRequired();
        requirement.Property(item => item.Priority).HasConversion<string>().HasMaxLength(16).IsRequired();
        requirement.Property(item => item.DecisionStatus).HasConversion<string>().HasMaxLength(16).IsRequired();
        requirement.Property(item => item.SourceType).HasConversion<string>().HasMaxLength(32).IsRequired();
        requirement.Property(item => item.Revision).IsConcurrencyToken();
        requirement.HasIndex(item => new { item.ProjectId, item.Key }).IsUnique();
        requirement.HasOne(item => item.Project).WithMany(item => item.Requirements).HasForeignKey(item => item.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        requirement.HasOne<ExternalReferenceRecord>().WithMany().HasForeignKey(item => item.SourceReferenceId)
            .OnDelete(DeleteBehavior.Restrict);

        var criterion = modelBuilder.Entity<AcceptanceCriterionRecord>();
        criterion.ToTable("AcceptanceCriteria");
        criterion.HasKey(item => item.Id);
        criterion.Property(item => item.Id).ValueGeneratedNever();
        criterion.Property(item => item.Text).IsRequired();
        criterion.HasIndex(item => new { item.RequirementId, item.Sequence }).IsUnique();
        criterion.HasOne(item => item.Requirement).WithMany(item => item.AcceptanceCriteria)
            .HasForeignKey(item => item.RequirementId).OnDelete(DeleteBehavior.Cascade);
        criterion.HasOne<ExternalReferenceRecord>().WithMany().HasForeignKey(item => item.VerificationReferenceId)
            .OnDelete(DeleteBehavior.Restrict);

        var reference = modelBuilder.Entity<ExternalReferenceRecord>();
        reference.ToTable("ExternalReferences");
        reference.HasKey(item => item.Id);
        reference.Property(item => item.Id).ValueGeneratedNever();
        reference.Property(item => item.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        reference.Property(item => item.Title).IsRequired();
        reference.Property(item => item.Target).IsRequired();
        reference.Property(item => item.Revision).IsConcurrencyToken();
        reference.HasIndex(item => item.ProjectId);
        reference.HasIndex(item => item.RequirementId);
        reference.HasOne(item => item.Project).WithMany(item => item.ExternalReferences)
            .HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Cascade);
        reference.HasOne<RequirementRecord>().WithMany().HasForeignKey(item => item.RequirementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
