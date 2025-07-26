using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SozlesmeSistemi.Migrations
{
    /// <inheritdoc />
    public partial class Mig39Tablo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ContractRequests_ContractRequestId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Contracts_ContractId",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ContractRequests_ContractRequestId",
                table: "Notifications",
                column: "ContractRequestId",
                principalTable: "ContractRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Contracts_ContractId",
                table: "Notifications",
                column: "ContractId",
                principalTable: "Contracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
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
    }
}
