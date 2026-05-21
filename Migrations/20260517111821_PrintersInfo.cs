using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarisApp.Migrations
{
    /// <inheritdoc />
    public partial class PrintersInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "kleur",
                table: "Infos",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "nietjes",
                table: "Infos",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "toner",
                table: "Infos",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "wachtwoord",
                table: "Infos",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$trmcJN44Hg2T6rOxGTv7oO.r.IU.AEZsX0nWGaN2pe6oVehBfXdbC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "kleur",
                table: "Infos");

            migrationBuilder.DropColumn(
                name: "nietjes",
                table: "Infos");

            migrationBuilder.DropColumn(
                name: "toner",
                table: "Infos");

            migrationBuilder.DropColumn(
                name: "wachtwoord",
                table: "Infos");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$PvpjYYC97f/m/YhWHquU6uF8v9iLuionEc0xtFSmZSdh7v8zgDw9.");
        }
    }
}
