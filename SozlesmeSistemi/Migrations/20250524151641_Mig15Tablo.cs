using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SozlesmeSistemi.Migrations
{
    /// <inheritdoc />
    public partial class Mig15Tablo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mevcut kolonları kaldır
            migrationBuilder.DropColumn(
                name: "KarsiTaraf",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Tarafimiz",
                table: "Contracts");

            // Units tablosuna geçici bir kayıt ekle
            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.Id);
                });

            migrationBuilder.Sql("INSERT INTO Units (Name, IsActive, CreatedDate, UpdatedDate) VALUES ('Geçici Birim', 1, GETDATE(), GETDATE())");

            // Yeni kolonları ekle
            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CounterUnitId",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CurrentStatus",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OurUnitId",
                table: "Contracts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Contracts tablosunda CounterUnitId ve OurUnitId için varsayılan değerleri ayarla
            migrationBuilder.Sql("UPDATE Contracts SET CounterUnitId = (SELECT TOP 1 Id FROM Units WHERE Name = 'Geçici Birim') WHERE CounterUnitId = 0");
            migrationBuilder.Sql("UPDATE Contracts SET OurUnitId = (SELECT TOP 1 Id FROM Units WHERE Name = 'Geçici Birim') WHERE OurUnitId = 0");

            // ContractRequests tablosunu oluştur
            migrationBuilder.CreateTable(
                name: "ContractRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestedById = table.Column<int>(type: "int", nullable: false),
                    RequestedToId = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Justification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstimatedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractRequests_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractRequests_Users_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractRequests_Users_RequestedToId",
                        column: x => x.RequestedToId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // UserHierarchies tablosunu oluştur
            migrationBuilder.CreateTable(
                name: "UserHierarchies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManagerId = table.Column<int>(type: "int", nullable: false),
                    SubordinateId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHierarchies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserHierarchies_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserHierarchies_Users_SubordinateId",
                        column: x => x.SubordinateId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // İndeksleri oluştur
            migrationBuilder.CreateIndex(
                name: "IX_Users_UnitId",
                table: "Users",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CounterUnitId",
                table: "Contracts",
                column: "CounterUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_OurUnitId",
                table: "Contracts",
                column: "OurUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractRequests_AssignedToUserId",
                table: "ContractRequests",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractRequests_RequestedById",
                table: "ContractRequests",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_ContractRequests_RequestedToId",
                table: "ContractRequests",
                column: "RequestedToId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHierarchies_ManagerId",
                table: "UserHierarchies",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHierarchies_SubordinateId",
                table: "UserHierarchies",
                column: "SubordinateId");

            // Yabancı anahtarları ekle
            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Units_CounterUnitId",
                table: "Contracts",
                column: "CounterUnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Units_OurUnitId",
                table: "Contracts",
                column: "OurUnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Units_UnitId",
                table: "Users",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Units_CounterUnitId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Units_OurUnitId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Units_UnitId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "ContractRequests");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "UserHierarchies");

            migrationBuilder.DropIndex(
                name: "IX_Users_UnitId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_CounterUnitId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_OurUnitId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CounterUnitId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "CurrentStatus",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "OurUnitId",
                table: "Contracts");

            migrationBuilder.AddColumn<string>(
                name: "KarsiTaraf",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Tarafimiz",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}