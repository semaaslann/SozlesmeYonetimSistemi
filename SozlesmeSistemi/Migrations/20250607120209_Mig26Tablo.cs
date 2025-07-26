using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SozlesmeSistemi.Migrations
{
    /// <inheritdoc />
    public partial class Mig26Tablo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SorumluGruplar",
                table: "Contracts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SorumluGruplar",
                table: "Contracts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
