using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SozlesmeSistemi.Migrations
{
    /// <inheritdoc />
    public partial class Mig37Tablo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractParafs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    ParaflayanUserId = table.Column<int>(type: "int", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: true),
                    IsParaflandi = table.Column<bool>(type: "bit", nullable: false),
                    ParafTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Not = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractParafs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractParafs_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractParafs_Users_ParaflayanUserId",
                        column: x => x.ParaflayanUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractParafs_ContractId",
                table: "ContractParafs",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractParafs_ParaflayanUserId",
                table: "ContractParafs",
                column: "ParaflayanUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractParafs");
        }
    }
}
