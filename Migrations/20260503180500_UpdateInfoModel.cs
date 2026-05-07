using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventarisApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInfoModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "garantie",
                table: "Infos",
                newName: "aantal");

            migrationBuilder.AddColumn<int>(
                name: "PersoonId",
                table: "Infos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "opmerkingen",
                table: "Infos",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "staat",
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
                value: "$2a$11$PvpjYYC97f/m/YhWHquU6uF8v9iLuionEc0xtFSmZSdh7v8zgDw9.");

            migrationBuilder.CreateIndex(
                name: "IX_Infos_PersoonId",
                table: "Infos",
                column: "PersoonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Infos_Personen_PersoonId",
                table: "Infos",
                column: "PersoonId",
                principalTable: "Personen",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infos_Personen_PersoonId",
                table: "Infos");

            migrationBuilder.DropIndex(
                name: "IX_Infos_PersoonId",
                table: "Infos");

            migrationBuilder.DropColumn(
                name: "PersoonId",
                table: "Infos");

            migrationBuilder.DropColumn(
                name: "opmerkingen",
                table: "Infos");

            migrationBuilder.DropColumn(
                name: "staat",
                table: "Infos");

            migrationBuilder.RenameColumn(
                name: "aantal",
                table: "Infos",
                newName: "garantie");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$eNfFxyOQ/7o76hXwE/18/.M9v2vDkY6m8tP51BwEZyV44Qx74o77G");
        }
    }
}
