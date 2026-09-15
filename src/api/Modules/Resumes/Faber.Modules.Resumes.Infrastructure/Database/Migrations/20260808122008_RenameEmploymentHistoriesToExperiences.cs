using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faber.Modules.Resumes.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmploymentHistoriesToExperiences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "employment_histories",
                schema: "resumes",
                newName: "experiences",
                newSchema: "resumes");

            migrationBuilder.RenameIndex(
                name: "ix_employment_histories_resume_id",
                schema: "resumes",
                table: "experiences",
                newName: "ix_experiences_resume_id");

            migrationBuilder.Sql(
                """
                ALTER TABLE resumes.experiences
                    RENAME CONSTRAINT pk_employment_histories TO pk_experiences;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE resumes.experiences
                    RENAME CONSTRAINT fk_employment_histories_resumes_resume_id
                    TO fk_experiences_resumes_resume_id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE resumes.experiences
                    RENAME CONSTRAINT fk_experiences_resumes_resume_id
                    TO fk_employment_histories_resumes_resume_id;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE resumes.experiences
                    RENAME CONSTRAINT pk_experiences TO pk_employment_histories;
                """);

            migrationBuilder.RenameIndex(
                name: "ix_experiences_resume_id",
                schema: "resumes",
                table: "experiences",
                newName: "ix_employment_histories_resume_id");

            migrationBuilder.RenameTable(
                name: "experiences",
                schema: "resumes",
                newName: "employment_histories",
                newSchema: "resumes");
        }
    }
}
