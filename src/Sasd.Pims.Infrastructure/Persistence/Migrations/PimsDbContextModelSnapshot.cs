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
            entity.Property(project => project.Id)
                .ValueGeneratedNever()
                .HasColumnType("TEXT");
            entity.Property(project => project.CreatedAtUtc).HasColumnType("TEXT");
            entity.Property(project => project.Key)
                .IsRequired()
                .HasMaxLength(64)
                .HasColumnType("TEXT");
            entity.Property(project => project.ModifiedAtUtc).HasColumnType("TEXT");
            entity.Property(project => project.Name).IsRequired().HasColumnType("TEXT");
            entity.Property(project => project.Revision)
                .IsConcurrencyToken()
                .HasColumnType("INTEGER");
            entity.Property(project => project.ShortDescription).HasColumnType("TEXT");
            entity.HasKey(project => project.Id);
            entity.HasIndex(project => project.Key).IsUnique();
            entity.ToTable("Projects");
        });
    }
}
