using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarisApp.Migrations
{
    /// <inheritdoc />
    public partial class SupportOpslagRoomAndFixValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "mac_address",
                table: "Wifis",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "local_ip",
                table: "Wifis",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Locaties",
                columns: new[] { "locatie_id", "abbreviation", "naam" },
                values: new object[] { 999, "IO", "Interne Opslag" });

            migrationBuilder.InsertData(
                table: "Lokalen",
                columns: new[] { "locatie_id", "lokaalnr", "plaatsen" },
                values: new object[] { 999, "Opslag", 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Lokalen",
                keyColumns: new[] { "locatie_id", "lokaalnr" },
                keyValues: new object[] { 999, "Opslag" });

            migrationBuilder.DeleteData(
                table: "Locaties",
                keyColumn: "locatie_id",
                keyValue: 999);

            migrationBuilder.UpdateData(
                table: "Wifis",
                keyColumn: "mac_address",
                keyValue: null,
                column: "mac_address",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "mac_address",
                table: "Wifis",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Wifis",
                keyColumn: "local_ip",
                keyValue: null,
                column: "local_ip",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "local_ip",
                table: "Wifis",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
