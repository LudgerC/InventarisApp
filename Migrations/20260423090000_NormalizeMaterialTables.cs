using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarisApp.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeMaterialTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Naam",
                table: "Materialen");

            migrationBuilder.AddColumn<int>(
                name: "MateriaalTypeId",
                table: "Materialen",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MateriaalTypes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Naam = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MateriaalTypes", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Materialen_MateriaalTypeId",
                table: "Materialen",
                column: "MateriaalTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Materialen_MateriaalTypes_MateriaalTypeId",
                table: "Materialen",
                column: "MateriaalTypeId",
                principalTable: "MateriaalTypes",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materialen_MateriaalTypes_MateriaalTypeId",
                table: "Materialen");

            migrationBuilder.DropTable(
                name: "MateriaalTypes");

            migrationBuilder.DropIndex(
                name: "IX_Materialen_MateriaalTypeId",
                table: "Materialen");

            migrationBuilder.DropColumn(
                name: "MateriaalTypeId",
                table: "Materialen");

            migrationBuilder.AddColumn<string>(
                name: "Naam",
                table: "Materialen",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
