using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sasd.Pims.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PimsDbContext))]
[Migration("202608200002_ProjectSteering")]
public sealed class ProjectSteering : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Defaults preserve every accepted 0.1 project while adding explicit, conservative steering facts.
        migrationBuilder.AddColumn<string>("ActivityState", "Projects", "TEXT", maxLength: 32,
            nullable: false, defaultValue: "NotStarted");
        migrationBuilder.AddColumn<DateTimeOffset?>("LastReviewedAtUtc", "Projects", "TEXT", nullable: true);
        migrationBuilder.AddColumn<DateTimeOffset?>("NextReviewDueAtUtc", "Projects", "TEXT", nullable: true);
        migrationBuilder.AddColumn<string>("Phase", "Projects", "TEXT", maxLength: 32,
            nullable: false, defaultValue: "Idea");
        migrationBuilder.AddColumn<DateOnly?>("TargetDate", "Projects", "TEXT", nullable: true);
        migrationBuilder.CreateTable("ProjectBlockers", table => new
        {
            Id = table.Column<Guid>("TEXT", nullable: false),
            ProjectId = table.Column<Guid>("TEXT", nullable: false),
            Summary = table.Column<string>("TEXT", nullable: false),
            Details = table.Column<string>("TEXT", nullable: true),
            CreatedAtUtc = table.Column<DateTimeOffset>("TEXT", nullable: false),
            ResolvedAtUtc = table.Column<DateTimeOffset?>("TEXT", nullable: true),
            ResolutionNote = table.Column<string>("TEXT", nullable: true),
        }, constraints: table =>
        {
            table.PrimaryKey("PK_ProjectBlockers", item => item.Id);
            table.ForeignKey("FK_ProjectBlockers_Projects_ProjectId", item => item.ProjectId, "Projects", "Id",
                onDelete: ReferentialAction.Cascade);
        });
        migrationBuilder.CreateIndex("IX_ProjectBlockers_ProjectId_ResolvedAtUtc", "ProjectBlockers",
            ["ProjectId", "ResolvedAtUtc"]);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ProjectBlockers");
        migrationBuilder.DropColumn("ActivityState", "Projects");
        migrationBuilder.DropColumn("LastReviewedAtUtc", "Projects");
        migrationBuilder.DropColumn("NextReviewDueAtUtc", "Projects");
        migrationBuilder.DropColumn("Phase", "Projects");
        migrationBuilder.DropColumn("TargetDate", "Projects");
    }
}
