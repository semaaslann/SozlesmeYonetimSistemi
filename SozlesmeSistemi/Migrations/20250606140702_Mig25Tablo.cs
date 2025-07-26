using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SozlesmeSistemi.Migrations
{
    /// <inheritdoc />
    public partial class Mig25Tablo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Users_ImzalayanKisiId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ImzalayanKisiId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ImzalayanKisiId",
                table: "Contracts");

            migrationBuilder.CreateTable(
                name: "ContractSigners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractSigners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractSigners_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractSigners_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractSigners_ContractId",
                table: "ContractSigners",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractSigners_UserId",
                table: "ContractSigners",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractSigners");

            migrationBuilder.AddColumn<int>(
                name: "ImzalayanKisiId",
                table: "Contracts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ImzalayanKisiId",
                table: "Contracts",
                column: "ImzalayanKisiId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Users_ImzalayanKisiId",
                table: "Contracts",
                column: "ImzalayanKisiId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
