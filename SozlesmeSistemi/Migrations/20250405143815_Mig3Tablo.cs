using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SozlesmeSistemi.Migrations
{
    /// <inheritdoc />
    public partial class Mig3Tablo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArsivKlasorNumarasi",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BuroKategoriNo",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EnflasyonArtisTarihi",
                table: "Contracts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FesihDisCozelSart",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FesihDurumu",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IlgiliIsBirimi",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ImzaOnayTarihi",
                table: "Contracts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KarsiTaraf",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OdemeVadeleri",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ParaBirimi",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SorumluGruplar",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SozlesmeKonusu",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SozlesmeSuresi",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SozlesmeTuru",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "SozlesmeTutari",
                table: "Contracts",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Tarafimiz",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArsivKlasorNumarasi",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "BuroKategoriNo",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "EnflasyonArtisTarihi",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "FesihDisCozelSart",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "FesihDurumu",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "IlgiliIsBirimi",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ImzaOnayTarihi",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "KarsiTaraf",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "OdemeVadeleri",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ParaBirimi",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SorumluGruplar",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SozlesmeKonusu",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SozlesmeSuresi",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SozlesmeTuru",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SozlesmeTutari",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "Tarafimiz",
                table: "Contracts");
        }
    }
}
