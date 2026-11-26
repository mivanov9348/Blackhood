using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blackhood.Data.Migrations
{
    /// <inheritdoc />
    public partial class GiftsAndGold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BuyIn",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Game",
                table: "Matches",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Legacy");

            migrationBuilder.AddColumn<int>(
                name: "Payout",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Settled",
                table: "Matches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Gold",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyIn",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Game",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Payout",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Settled",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Gold",
                table: "AspNetUsers");
        }
    }
}
