using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sasd.Pims.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PimsDbContext))]
[Migration("202608200001_ProjectCatalog")]
public sealed class ProjectCatalog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // All columns are nullable/defaulted so accepted 0.0.1 rows survive unchanged.
        migrationBuilder.AddColumn<string>(name: "Benefit", table: "Projects", type: "TEXT", nullable: true);
        migrationBuilder.AddColumn<string>(name: "Goal", table: "Projects", type: "TEXT", nullable: true);
        migrationBuilder.AddColumn<bool>(name: "IsArchived", table: "Projects", type: "INTEGER", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<string>(name: "ProjectArea", table: "Projects", type: "TEXT", maxLength: 32, nullable: true);
        migrationBuilder.AddColumn<string>(name: "ProjectType", table: "Projects", type: "TEXT", maxLength: 32, nullable: true);
        migrationBuilder.AddColumn<string>(name: "Responsibility", table: "Projects", type: "TEXT", nullable: true);
        migrationBuilder.CreateTable(
            name: "ProjectTags",
            columns: table => new
            {
                ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                Value = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false, collation: "NOCASE"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ProjectTags", item => new { item.ProjectId, item.Value });
                table.ForeignKey("FK_ProjectTags_Projects_ProjectId", item => item.ProjectId, "Projects", "Id", onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ProjectTags");
        migrationBuilder.DropColumn(name: "Benefit", table: "Projects");
        migrationBuilder.DropColumn(name: "Goal", table: "Projects");
        migrationBuilder.DropColumn(name: "IsArchived", table: "Projects");
        migrationBuilder.DropColumn(name: "ProjectArea", table: "Projects");
        migrationBuilder.DropColumn(name: "ProjectType", table: "Projects");
        migrationBuilder.DropColumn(name: "Responsibility", table: "Projects");
    }
}
