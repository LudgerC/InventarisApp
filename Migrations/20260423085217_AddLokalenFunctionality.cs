using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarisApp.Migrations
{
    /// <inheritdoc />
    public partial class AddLokalenFunctionality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LokaalId",
                table: "Infos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Lokalen",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Naam = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Beschrijving = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lokalen", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Materialen",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Naam = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Aantal = table.Column<int>(type: "int", nullable: false),
                    LokaalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materialen", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Materialen_Lokalen_LokaalId",
                        column: x => x.LokaalId,
                        principalTable: "Lokalen",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Infos_LokaalId",
                table: "Infos",
                column: "LokaalId");

            migrationBuilder.CreateIndex(
                name: "IX_Materialen_LokaalId",
                table: "Materialen",
                column: "LokaalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Infos_Lokalen_LokaalId",
                table: "Infos",
                column: "LokaalId",
                principalTable: "Lokalen",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infos_Lokalen_LokaalId",
                table: "Infos");

            migrationBuilder.DropTable(
                name: "Materialen");

            migrationBuilder.DropTable(
                name: "Lokalen");

            migrationBuilder.DropIndex(
                name: "IX_Infos_LokaalId",
                table: "Infos");

            migrationBuilder.DropColumn(
                name: "LokaalId",
                table: "Infos");
        }
    }
}
