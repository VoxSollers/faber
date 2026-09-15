using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faber.Modules.Resumes.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AlignValidatorsAndAddUserIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "uri",
                schema: "resumes",
                table: "links",
                type: "character varying(2083)",
                maxLength: 2083,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_resumes_user_id",
                schema: "resumes",
                table: "resumes",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_resumes_user_id",
                schema: "resumes",
                table: "resumes");

            migrationBuilder.AlterColumn<string>(
                name: "uri",
                schema: "resumes",
                table: "links",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2083)",
                oldMaxLength: 2083,
                oldNullable: true);
        }
    }
}
