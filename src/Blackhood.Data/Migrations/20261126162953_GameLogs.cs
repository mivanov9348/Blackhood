using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blackhood.Data.Migrations
{
    /// <inheritdoc />
    public partial class GameLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Game = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Seq = table.Column<int>(type: "int", nullable: false),
                    At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Round = table.Column<int>(type: "int", nullable: false),
                    Phase = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameLogs_At",
                table: "GameLogs",
                column: "At");

            migrationBuilder.CreateIndex(
                name: "IX_GameLogs_MatchId_Seq",
                table: "GameLogs",
                columns: new[] { "MatchId", "Seq" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameLogs");
        }
    }
}
