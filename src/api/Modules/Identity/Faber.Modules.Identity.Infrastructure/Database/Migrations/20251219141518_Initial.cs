using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faber.Modules.Identity.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.CreateTable(
                name: "action_tokens",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<string>(type: "character(26)", fixedLength: true, maxLength: 26, nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    hash = table.Column<string>(type: "text", nullable: false),
                    salt = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    consumed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_action_tokens", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_action_tokens_email",
                schema: "identity",
                table: "action_tokens",
                column: "email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "action_tokens",
                schema: "identity");
        }
    }
}
