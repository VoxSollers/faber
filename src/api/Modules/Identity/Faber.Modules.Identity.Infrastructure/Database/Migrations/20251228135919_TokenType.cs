using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faber.Modules.Identity.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class TokenType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                schema: "identity",
                table: "action_tokens",
                type: "varchar(15)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                schema: "identity",
                table: "action_tokens");
        }
    }
}
