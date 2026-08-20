using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sasd.Pims.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PimsDbContext))]
[Migration("202608200004_SearchTraceabilityAndExchange")]
public sealed class SearchTraceabilityAndExchange : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable("ChangeEvents", table => new
        {
            Id = table.Column<Guid>("TEXT", nullable: false),
            ProjectId = table.Column<Guid>("TEXT", nullable: false),
            EntityType = table.Column<string>("TEXT", maxLength: 32, nullable: false),
            EntityId = table.Column<Guid>("TEXT", nullable: false),
            EventType = table.Column<string>("TEXT", maxLength: 64, nullable: false),
            OccurredAtUtc = table.Column<DateTimeOffset>("TEXT", nullable: false),
            OldValue = table.Column<string>("TEXT", maxLength: 256, nullable: true),
            NewValue = table.Column<string>("TEXT", maxLength: 256, nullable: true),
        }, constraints: table =>
        {
            table.PrimaryKey("PK_ChangeEvents", item => item.Id);
            table.ForeignKey("FK_ChangeEvents_Projects_ProjectId", item => item.ProjectId,
                "Projects", "Id", onDelete: ReferentialAction.Cascade);
        });
        migrationBuilder.CreateIndex("IX_ChangeEvents_ProjectId_OccurredAtUtc", "ChangeEvents",
            ["ProjectId", "OccurredAtUtc"]);
        migrationBuilder.CreateIndex("IX_ChangeEvents_EntityType_EntityId", "ChangeEvents",
            ["EntityType", "EntityId"]);
        migrationBuilder.CreateIndex("IX_Projects_Phase_ActivityState", "Projects", ["Phase", "ActivityState"]);
        migrationBuilder.CreateIndex("IX_Projects_ProjectType_ProjectArea", "Projects", ["ProjectType", "ProjectArea"]);
        migrationBuilder.CreateIndex("IX_Requirements_Priority_DecisionStatus_SourceType", "Requirements",
            ["Priority", "DecisionStatus", "SourceType"]);
        migrationBuilder.CreateIndex("IX_ExternalReferences_Type_ProjectId", "ExternalReferences", ["Type", "ProjectId"]);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ChangeEvents");
        migrationBuilder.DropIndex("IX_Projects_Phase_ActivityState", "Projects");
        migrationBuilder.DropIndex("IX_Projects_ProjectType_ProjectArea", "Projects");
        migrationBuilder.DropIndex("IX_Requirements_Priority_DecisionStatus_SourceType", "Requirements");
        migrationBuilder.DropIndex("IX_ExternalReferences_Type_ProjectId", "ExternalReferences");
    }
}
