using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Translating.gRPC.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "translation");

            migrationBuilder.CreateSequence(
                name: "languageseq",
                schema: "translation",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "translationseq",
                schema: "translation",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "Languages",
                schema: "translation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Translations",
                schema: "translation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    EnglishWord = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TranslationText = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Translations_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalSchema: "translation",
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Translations_LanguageId",
                schema: "translation",
                table: "Translations",
                column: "LanguageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Translations",
                schema: "translation");

            migrationBuilder.DropTable(
                name: "Languages",
                schema: "translation");

            migrationBuilder.DropSequence(
                name: "languageseq",
                schema: "translation");

            migrationBuilder.DropSequence(
                name: "translationseq",
                schema: "translation");
        }
    }
}
