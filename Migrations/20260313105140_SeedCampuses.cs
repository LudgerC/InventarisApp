using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InventarisApp.Migrations
{
    /// <inheritdoc />
    public partial class SeedCampuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Locaties",
                columns: new[] { "locatie_id", "abbreviation", "naam" },
                values: new object[,]
                {
                    { 1, "CR", "Campus Rouppe" },
                    { 2, "CL", "Campus Landsroem" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Locaties",
                keyColumn: "locatie_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Locaties",
                keyColumn: "locatie_id",
                keyValue: 2);
        }
    }
}
