using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SozlesmeSistemi.Migrations
{
    /// <inheritdoc />
    public partial class Mig30Tablo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "ContractSigners",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ContractSigners_UnitId",
                table: "ContractSigners",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContractSigners_Units_UnitId",
                table: "ContractSigners",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractSigners_Units_UnitId",
                table: "ContractSigners");

            migrationBuilder.DropIndex(
                name: "IX_ContractSigners_UnitId",
                table: "ContractSigners");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "ContractSigners");
        }
    }
}
