using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sasd.Pims.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PimsDbContext))]
[Migration("202608210005_FullMustMvp")]
public sealed class FullMustMvp : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Historical blockers cannot be truthfully backfilled. Nullable columns preserve every 0.4 fact;
        // domain creation enforces the new required facts for all 0.5 blockers.
        migrationBuilder.AddColumn<string>("Cause", "ProjectBlockers", "TEXT", nullable: true);
        migrationBuilder.AddColumn<string>("Impact", "ProjectBlockers", "TEXT", nullable: true);
        migrationBuilder.AddColumn<string>("AffectedObject", "ProjectBlockers", "TEXT", nullable: true);
        migrationBuilder.AddColumn<string>("NextAction", "ProjectBlockers", "TEXT", nullable: true);
        migrationBuilder.AddColumn<Guid>("ExternalTaskReferenceId", "ProjectBlockers", "TEXT", nullable: true);
        migrationBuilder.CreateIndex("IX_ProjectBlockers_ExternalTaskReferenceId", "ProjectBlockers", "ExternalTaskReferenceId");
        // A conventional FK cannot express both same-Project ownership and the controlled ExternalTask type.
        // SQLite triggers enforce the complete invariant for every writer, including direct database access.
        migrationBuilder.Sql("""
            CREATE TRIGGER TR_ProjectBlockers_ExternalTask_Insert
            BEFORE INSERT ON ProjectBlockers
            WHEN NEW.ExternalTaskReferenceId IS NOT NULL AND NOT EXISTS (
                SELECT 1 FROM ExternalReferences r
                WHERE r.Id = NEW.ExternalTaskReferenceId AND r.ProjectId = NEW.ProjectId AND r.Type = 'ExternalTask')
            BEGIN SELECT RAISE(ABORT, 'invalid blocker external task reference'); END;
            CREATE TRIGGER TR_ProjectBlockers_ExternalTask_Update
            BEFORE UPDATE OF ExternalTaskReferenceId, ProjectId ON ProjectBlockers
            WHEN NEW.ExternalTaskReferenceId IS NOT NULL AND NOT EXISTS (
                SELECT 1 FROM ExternalReferences r
                WHERE r.Id = NEW.ExternalTaskReferenceId AND r.ProjectId = NEW.ProjectId AND r.Type = 'ExternalTask')
            BEGIN SELECT RAISE(ABORT, 'invalid blocker external task reference'); END;
            CREATE TRIGGER TR_ExternalReferences_BlockerTask_Delete
            BEFORE DELETE ON ExternalReferences
            WHEN EXISTS (SELECT 1 FROM ProjectBlockers b WHERE b.ExternalTaskReferenceId = OLD.Id)
            BEGIN SELECT RAISE(ABORT, 'external task reference is used by a blocker'); END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TRIGGER IF EXISTS TR_ProjectBlockers_ExternalTask_Insert; DROP TRIGGER IF EXISTS TR_ProjectBlockers_ExternalTask_Update; DROP TRIGGER IF EXISTS TR_ExternalReferences_BlockerTask_Delete;");
        migrationBuilder.DropIndex("IX_ProjectBlockers_ExternalTaskReferenceId", "ProjectBlockers");
        migrationBuilder.DropColumn("Cause", "ProjectBlockers");
        migrationBuilder.DropColumn("Impact", "ProjectBlockers");
        migrationBuilder.DropColumn("AffectedObject", "ProjectBlockers");
        migrationBuilder.DropColumn("NextAction", "ProjectBlockers");
        migrationBuilder.DropColumn("ExternalTaskReferenceId", "ProjectBlockers");
    }
}
