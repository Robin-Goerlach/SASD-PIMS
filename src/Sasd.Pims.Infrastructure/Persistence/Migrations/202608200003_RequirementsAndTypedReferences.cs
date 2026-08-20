using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sasd.Pims.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PimsDbContext))]
[Migration("202608200003_RequirementsAndTypedReferences")]
public sealed class RequirementsAndTypedReferences : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // The migration is additive: all 0.2 tables and rows remain unchanged.
        migrationBuilder.CreateTable("ExternalReferences", table => new
        {
            Id = table.Column<Guid>("TEXT", nullable: false),
            ProjectId = table.Column<Guid>("TEXT", nullable: false),
            RequirementId = table.Column<Guid?>("TEXT", nullable: true),
            Type = table.Column<string>("TEXT", maxLength: 32, nullable: false),
            Title = table.Column<string>("TEXT", nullable: false),
            Target = table.Column<string>("TEXT", nullable: false),
            Revision = table.Column<int>("INTEGER", nullable: false),
        }, constraints: table =>
        {
            table.PrimaryKey("PK_ExternalReferences", item => item.Id);
            table.ForeignKey("FK_ExternalReferences_Projects_ProjectId", item => item.ProjectId,
                "Projects", "Id", onDelete: ReferentialAction.Cascade);
        });
        migrationBuilder.CreateTable("Requirements", table => new
        {
            Id = table.Column<Guid>("TEXT", nullable: false),
            ProjectId = table.Column<Guid>("TEXT", nullable: false),
            Key = table.Column<string>("TEXT", maxLength: 32, nullable: false),
            Title = table.Column<string>("TEXT", nullable: false),
            Description = table.Column<string>("TEXT", nullable: true),
            Rationale = table.Column<string>("TEXT", nullable: true),
            Priority = table.Column<string>("TEXT", maxLength: 16, nullable: false),
            DecisionStatus = table.Column<string>("TEXT", maxLength: 16, nullable: false),
            DecisionReason = table.Column<string>("TEXT", nullable: true),
            SourceType = table.Column<string>("TEXT", maxLength: 32, nullable: false),
            SourceDate = table.Column<DateOnly?>("TEXT", nullable: true),
            SourceSummary = table.Column<string>("TEXT", nullable: true),
            SourceReferenceId = table.Column<Guid?>("TEXT", nullable: true),
            Revision = table.Column<int>("INTEGER", nullable: false),
        }, constraints: table =>
        {
            table.PrimaryKey("PK_Requirements", item => item.Id);
            table.ForeignKey("FK_Requirements_Projects_ProjectId", item => item.ProjectId,
                "Projects", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_Requirements_ExternalReferences_SourceReferenceId", item => item.SourceReferenceId,
                "ExternalReferences", "Id", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.AddForeignKey("FK_ExternalReferences_Requirements_RequirementId", "ExternalReferences",
            "RequirementId", "Requirements", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.CreateTable("AcceptanceCriteria", table => new
        {
            Id = table.Column<Guid>("TEXT", nullable: false),
            RequirementId = table.Column<Guid>("TEXT", nullable: false),
            Sequence = table.Column<int>("INTEGER", nullable: false),
            Text = table.Column<string>("TEXT", nullable: false),
            VerificationReferenceId = table.Column<Guid?>("TEXT", nullable: true),
        }, constraints: table =>
        {
            table.PrimaryKey("PK_AcceptanceCriteria", item => item.Id);
            table.ForeignKey("FK_AcceptanceCriteria_Requirements_RequirementId", item => item.RequirementId,
                "Requirements", "Id", onDelete: ReferentialAction.Cascade);
            table.ForeignKey("FK_AcceptanceCriteria_ExternalReferences_VerificationReferenceId",
                item => item.VerificationReferenceId, "ExternalReferences", "Id", onDelete: ReferentialAction.Restrict);
        });
        migrationBuilder.CreateIndex("IX_ExternalReferences_ProjectId", "ExternalReferences", "ProjectId");
        migrationBuilder.CreateIndex("IX_ExternalReferences_RequirementId", "ExternalReferences", "RequirementId");
        migrationBuilder.CreateIndex("IX_Requirements_ProjectId_Key", "Requirements", ["ProjectId", "Key"], unique: true);
        migrationBuilder.CreateIndex("IX_Requirements_SourceReferenceId", "Requirements", "SourceReferenceId");
        migrationBuilder.CreateIndex("IX_AcceptanceCriteria_RequirementId_Sequence", "AcceptanceCriteria",
            ["RequirementId", "Sequence"], unique: true);
        migrationBuilder.CreateIndex("IX_AcceptanceCriteria_VerificationReferenceId", "AcceptanceCriteria",
            "VerificationReferenceId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("AcceptanceCriteria");
        migrationBuilder.DropForeignKey("FK_ExternalReferences_Requirements_RequirementId", "ExternalReferences");
        migrationBuilder.DropTable("Requirements");
        migrationBuilder.DropTable("ExternalReferences");
    }
}
