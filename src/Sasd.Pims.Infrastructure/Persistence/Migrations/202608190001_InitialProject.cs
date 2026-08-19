using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sasd.Pims.Infrastructure.Persistence.Migrations;

[DbContext(typeof(PimsDbContext))]
[Migration("202608190001_InitialProject")]
public sealed class InitialProject : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Projects",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Key = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: false),
                ShortDescription = table.Column<string>(type: "TEXT", nullable: true),
                CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                ModifiedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                Revision = table.Column<int>(type: "INTEGER", nullable: false),
            },
            constraints: table => table.PrimaryKey("PK_Projects", item => item.Id));

        migrationBuilder.CreateIndex(
            name: "IX_Projects_Key",
            table: "Projects",
            column: "Key",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Projects");
    }
}
