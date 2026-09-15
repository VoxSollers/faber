using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faber.Modules.Identity.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class SelectorAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_action_tokens_email",
                schema: "identity",
                table: "action_tokens");

            migrationBuilder.AddColumn<string>(
                name: "selector",
                schema: "identity",
                table: "action_tokens",
                type: "character(26)",
                fixedLength: true,
                maxLength: 26,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_action_tokens_selector",
                schema: "identity",
                table: "action_tokens",
                column: "selector");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_action_tokens_selector",
                schema: "identity",
                table: "action_tokens");

            migrationBuilder.DropColumn(
                name: "selector",
                schema: "identity",
                table: "action_tokens");

            migrationBuilder.CreateIndex(
                name: "ix_action_tokens_email",
                schema: "identity",
                table: "action_tokens",
                column: "email");
        }
    }
}
