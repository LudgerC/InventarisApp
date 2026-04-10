using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventarisApp.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLocatieEnLokaal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infos_Lokalen_locatie_id_lokaalnr",
                table: "Infos");

            migrationBuilder.DropTable(
                name: "Lokalen");

            migrationBuilder.DropTable(
                name: "Locaties");

            migrationBuilder.DropIndex(
                name: "IX_Infos_locatie_id_lokaalnr",
                table: "Infos");

            migrationBuilder.DropColumn(
                name: "locatie_id",
                table: "Infos");

            migrationBuilder.DropColumn(
                name: "lokaalnr",
                table: "Infos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "locatie_id",
                table: "Infos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lokaalnr",
                table: "Infos",
                type: "varchar(50)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Locaties",
                columns: table => new
                {
                    locatie_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    abbreviation = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    naam = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locaties", x => x.locatie_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Lokalen",
                columns: table => new
                {
                    locatie_id = table.Column<int>(type: "int", nullable: false),
                    lokaalnr = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    plaatsen = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lokalen", x => new { x.locatie_id, x.lokaalnr });
                    table.ForeignKey(
                        name: "FK_Lokalen_Locaties_locatie_id",
                        column: x => x.locatie_id,
                        principalTable: "Locaties",
                        principalColumn: "locatie_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Locaties",
                columns: new[] { "locatie_id", "abbreviation", "naam" },
                values: new object[,]
                {
                    { 1, "CR", "Campus Rouppe" },
                    { 2, "CL", "Campus Landsroem" }
                });

            migrationBuilder.InsertData(
                table: "Lokalen",
                columns: new[] { "locatie_id", "lokaalnr", "plaatsen" },
                values: new object[,]
                {
                    { 1, "Opslag", 0 },
                    { 2, "Opslag", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Infos_locatie_id_lokaalnr",
                table: "Infos",
                columns: new[] { "locatie_id", "lokaalnr" });

            migrationBuilder.AddForeignKey(
                name: "FK_Infos_Lokalen_locatie_id_lokaalnr",
                table: "Infos",
                columns: new[] { "locatie_id", "lokaalnr" },
                principalTable: "Lokalen",
                principalColumns: new[] { "locatie_id", "lokaalnr" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
