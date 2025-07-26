using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SozlesmeSistemi.Migrations
{
    /// <inheritdoc />
    public partial class Mig9Tablo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_Users_UserId",
                table: "Reminders");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Reminders",
                newName: "ContractId");

            migrationBuilder.RenameIndex(
                name: "IX_Reminders_UserId",
                table: "Reminders",
                newName: "IX_Reminders_ContractId");

            migrationBuilder.AddColumn<bool>(
                name: "IsNew",
                table: "Reminders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_Contracts_ContractId",
                table: "Reminders",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reminders_Contracts_ContractId",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "IsNew",
                table: "Reminders");

            migrationBuilder.RenameColumn(
                name: "ContractId",
                table: "Reminders",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Reminders_ContractId",
                table: "Reminders",
                newName: "IX_Reminders_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reminders_Users_UserId",
                table: "Reminders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
