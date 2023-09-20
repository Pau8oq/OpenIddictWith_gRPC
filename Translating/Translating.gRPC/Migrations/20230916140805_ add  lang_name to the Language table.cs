using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Translating.gRPC.Migrations
{
    /// <inheritdoc />
    public partial class addlang_nametotheLanguagetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LangName",
                schema: "translation",
                table: "Languages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LangName",
                schema: "translation",
                table: "Languages");
        }
    }
}
