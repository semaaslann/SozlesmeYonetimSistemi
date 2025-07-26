using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SozlesmeSistemi.Migrations
{
    /// <inheritdoc />
    public partial class Mig6Tablo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Users_UserId",
                table: "Contracts");

            migrationBuilder.AddColumn<DateTime>(
                name: "FinisDate",
                table: "Contracts",
                type: "datetime2",
                nullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Users_UserId",
                table: "Contracts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Users_ImzalayanKisiId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Users_UserId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ImzalayanKisiId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "FinisDate",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ImzalayanKisiId",
                table: "Contracts");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Users_UserId",
                table: "Contracts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
