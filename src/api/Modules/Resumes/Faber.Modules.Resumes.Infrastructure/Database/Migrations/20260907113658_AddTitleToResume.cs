using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faber.Modules.Resumes.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTitleToResume : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "title",
                schema: "resumes",
                table: "resumes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE resumes.resumes r
                SET title = pd.trimmed_name
                FROM (
                    SELECT resume_id, left(trim(concat_ws(' ', firstname, lastname)), 100) AS trimmed_name
                    FROM resumes.personal_details
                ) pd
                WHERE pd.resume_id = r.id
                    AND pd.trimmed_name <> '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "title",
                schema: "resumes",
                table: "resumes");
        }
    }
}
