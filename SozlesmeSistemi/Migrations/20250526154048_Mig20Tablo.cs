using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SozlesmeSistemi.Migrations
{
    /// <inheritdoc />
    public partial class Mig20Tablo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "ContractRequests");

            

            migrationBuilder.CreateIndex(
                name: "IX_ContractRequests_UnitId",
                table: "ContractRequests",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContractRequests_Units_UnitId",
                table: "ContractRequests",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractRequests_Units_UnitId",
                table: "ContractRequests");

            migrationBuilder.DropIndex(
                name: "IX_ContractRequests_UnitId",
                table: "ContractRequests");

         

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "ContractRequests",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
