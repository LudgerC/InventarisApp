using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarisApp.Migrations
{
    /// <inheritdoc />
    public partial class ExpandLokalenDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AantalPlaatsen",
                table: "Lokalen",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsExtern",
                table: "Lokalen",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LocatieId",
                table: "Lokalen",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Locaties",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Naam = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locaties", x => x.ID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Lokalen_LocatieId",
                table: "Lokalen",
                column: "LocatieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lokalen_Locaties_LocatieId",
                table: "Lokalen",
                column: "LocatieId",
                principalTable: "Locaties",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lokalen_Locaties_LocatieId",
                table: "Lokalen");

            migrationBuilder.DropTable(
                name: "Locaties");

            migrationBuilder.DropIndex(
                name: "IX_Lokalen_LocatieId",
                table: "Lokalen");

            migrationBuilder.DropColumn(
                name: "AantalPlaatsen",
                table: "Lokalen");

            migrationBuilder.DropColumn(
                name: "IsExtern",
                table: "Lokalen");

            migrationBuilder.DropColumn(
                name: "LocatieId",
                table: "Lokalen");
        }
    }
}
