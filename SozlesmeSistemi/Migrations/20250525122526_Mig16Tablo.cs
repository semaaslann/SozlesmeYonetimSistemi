using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SozlesmeSistemi.Migrations
{
    /// <inheritdoc />
    public partial class Mig16Tablo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContractRequestId",
                table: "Contracts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContractStatusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractStatusHistories_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractStatusHistories_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ContractRequestId",
                table: "Contracts",
                column: "ContractRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractStatusHistories_ChangedByUserId",
                table: "ContractStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractStatusHistories_ContractId",
                table: "ContractStatusHistories",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_ContractRequests_ContractRequestId",
                table: "Contracts",
                column: "ContractRequestId",
                principalTable: "ContractRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_ContractRequests_ContractRequestId",
                table: "Contracts");

            migrationBuilder.DropTable(
                name: "ContractStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ContractRequestId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ContractRequestId",
                table: "Contracts");
        }
    }
}
