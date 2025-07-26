using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SozlesmeSistemi.Migrations
{
    /// <inheritdoc />
    public partial class Mig35Tablo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Contracts_ContractId",
                table: "Notifications");

            migrationBuilder.AlterColumn<int>(
                name: "ContractId",
                table: "Notifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ContractRequestId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "ContractRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ContractRequestId",
                table: "Notifications",
                column: "ContractRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ContractRequests_ContractRequestId",
                table: "Notifications",
                column: "ContractRequestId",
                principalTable: "ContractRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Contracts_ContractId",
                table: "Notifications",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ContractRequests_ContractRequestId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Contracts_ContractId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ContractRequestId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ContractRequestId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "ContractRequests");

            migrationBuilder.AlterColumn<int>(
                name: "ContractId",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Contracts_ContractId",
                table: "Notifications",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
