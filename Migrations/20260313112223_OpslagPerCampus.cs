using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventarisApp.Migrations
{
    /// <inheritdoc />
    public partial class OpslagPerCampus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Insert new rooms first
            migrationBuilder.InsertData(
                table: "Lokalen",
                columns: new[] { "locatie_id", "lokaalnr", "plaatsen" },
                values: new object[,]
                {
                    { 1, "Opslag", 0 },
                    { 2, "Opslag", 0 }
                });

            // 2. Update existing data to point to Campus Rouppe (ID 1)
            migrationBuilder.Sql("UPDATE Infos SET locatie_id = 1 WHERE locatie_id = 999");

            // 3. Delete old data
            migrationBuilder.DeleteData(
                table: "Lokalen",
                keyColumns: new[] { "locatie_id", "lokaalnr" },
                keyValues: new object[] { 999, "Opslag" });

            migrationBuilder.DeleteData(
                table: "Locaties",
                keyColumn: "locatie_id",
                keyValue: 999);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Lokalen",
                keyColumns: new[] { "locatie_id", "lokaalnr" },
                keyValues: new object[] { 1, "Opslag" });

            migrationBuilder.DeleteData(
                table: "Lokalen",
                keyColumns: new[] { "locatie_id", "lokaalnr" },
                keyValues: new object[] { 2, "Opslag" });

            migrationBuilder.InsertData(
                table: "Locaties",
                columns: new[] { "locatie_id", "abbreviation", "naam" },
                values: new object[] { 999, "IO", "Interne Opslag" });

            migrationBuilder.InsertData(
                table: "Lokalen",
                columns: new[] { "locatie_id", "lokaalnr", "plaatsen" },
                values: new object[] { 999, "Opslag", 0 });
        }
    }
}
