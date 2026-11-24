using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blackhood.Data.Migrations
{
    /// <inheritdoc />
    public partial class Characters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Nickname = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Age = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Archetype = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Rarity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Drives_Ambition = table.Column<int>(type: "int", nullable: false),
                    Drives_Boldness = table.Column<int>(type: "int", nullable: false),
                    Drives_FearOfLoss = table.Column<int>(type: "int", nullable: false),
                    Drives_Greed = table.Column<int>(type: "int", nullable: false),
                    Drives_Loyalty = table.Column<int>(type: "int", nullable: false),
                    Drives_Patience = table.Column<int>(type: "int", nullable: false),
                    Drives_Pride = table.Column<int>(type: "int", nullable: false),
                    Drives_Revenge = table.Column<int>(type: "int", nullable: false),
                    Skills_Calculation = table.Column<int>(type: "int", nullable: false),
                    Skills_Charisma = table.Column<int>(type: "int", nullable: false),
                    Skills_Composure = table.Column<int>(type: "int", nullable: false),
                    Skills_Cunning = table.Column<int>(type: "int", nullable: false),
                    Skills_Insight = table.Column<int>(type: "int", nullable: false),
                    Appearance = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Backstory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Behavior = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Host = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Mechanics = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Motives = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Origin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Psychology = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Speech = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CharacterAssets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Expression = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterAssets_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    Situation = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterLines_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromCharacterId = table.Column<int>(type: "int", nullable: false),
                    ToCharacterId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false),
                    Story = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Effect = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterRelations_Characters_FromCharacterId",
                        column: x => x.FromCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterRelations_Characters_ToCharacterId",
                        column: x => x.ToCharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CharacterTells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Fakeable = table.Column<bool>(type: "bit", nullable: false),
                    VisibleWithoutDossier = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterTells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterTells_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerEncounters",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CharacterId = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    FirstMetAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMetAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerEncounters", x => new { x.UserId, x.CharacterId });
                    table.ForeignKey(
                        name: "FK_PlayerEncounters_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerEncounters_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterAssets_CharacterId_Kind_Expression",
                table: "CharacterAssets",
                columns: new[] { "CharacterId", "Kind", "Expression" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterLines_CharacterId_Situation",
                table: "CharacterLines",
                columns: new[] { "CharacterId", "Situation" });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterRelations_FromCharacterId_ToCharacterId",
                table: "CharacterRelations",
                columns: new[] { "FromCharacterId", "ToCharacterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterRelations_ToCharacterId",
                table: "CharacterRelations",
                column: "ToCharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_Code",
                table: "Characters",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CharacterTells_CharacterId",
                table: "CharacterTells",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerEncounters_CharacterId",
                table: "PlayerEncounters",
                column: "CharacterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterAssets");

            migrationBuilder.DropTable(
                name: "CharacterLines");

            migrationBuilder.DropTable(
                name: "CharacterRelations");

            migrationBuilder.DropTable(
                name: "CharacterTells");

            migrationBuilder.DropTable(
                name: "PlayerEncounters");

            migrationBuilder.DropTable(
                name: "Characters");
        }
    }
}
