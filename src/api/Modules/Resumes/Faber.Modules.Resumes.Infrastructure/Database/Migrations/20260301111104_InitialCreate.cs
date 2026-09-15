using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faber.Modules.Resumes.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "resumes");

            migrationBuilder.CreateTable(
                name: "resumes",
                schema: "resumes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    pro_file = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    localization = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, defaultValue: "en-us"),
                    hobbies = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resumes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "courses",
                schema: "resumes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    school = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    resume_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_courses", x => x.id);
                    table.ForeignKey(
                        name: "fk_courses_resumes_resume_id",
                        column: x => x.resume_id,
                        principalSchema: "resumes",
                        principalTable: "resumes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "educations",
                schema: "resumes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    school = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    degree = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    resume_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_educations", x => x.id);
                    table.ForeignKey(
                        name: "fk_educations_resumes_resume_id",
                        column: x => x.resume_id,
                        principalSchema: "resumes",
                        principalTable: "resumes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "employment_histories",
                schema: "resumes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    employer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    resume_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employment_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_employment_histories_resumes_resume_id",
                        column: x => x.resume_id,
                        principalSchema: "resumes",
                        principalTable: "resumes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "languages",
                schema: "resumes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    level = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    resume_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_languages", x => x.id);
                    table.ForeignKey(
                        name: "fk_languages_resumes_resume_id",
                        column: x => x.resume_id,
                        principalSchema: "resumes",
                        principalTable: "resumes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "links",
                schema: "resumes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    uri = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    resume_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_links_resumes_resume_id",
                        column: x => x.resume_id,
                        principalSchema: "resumes",
                        principalTable: "resumes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "personal_details",
                schema: "resumes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    photo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    firstname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    lastname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    street = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    post_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    driving_license = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    resume_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_personal_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_personal_details_resumes_resume_id",
                        column: x => x.resume_id,
                        principalSchema: "resumes",
                        principalTable: "resumes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "skills",
                schema: "resumes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    level = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    resume_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_skills", x => x.id);
                    table.ForeignKey(
                        name: "fk_skills_resumes_resume_id",
                        column: x => x.resume_id,
                        principalSchema: "resumes",
                        principalTable: "resumes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_courses_resume_id",
                schema: "resumes",
                table: "courses",
                column: "resume_id");

            migrationBuilder.CreateIndex(
                name: "ix_educations_resume_id",
                schema: "resumes",
                table: "educations",
                column: "resume_id");

            migrationBuilder.CreateIndex(
                name: "ix_employment_histories_resume_id",
                schema: "resumes",
                table: "employment_histories",
                column: "resume_id");

            migrationBuilder.CreateIndex(
                name: "ix_languages_resume_id",
                schema: "resumes",
                table: "languages",
                column: "resume_id");

            migrationBuilder.CreateIndex(
                name: "ix_links_resume_id",
                schema: "resumes",
                table: "links",
                column: "resume_id");

            migrationBuilder.CreateIndex(
                name: "ix_personal_details_resume_id",
                schema: "resumes",
                table: "personal_details",
                column: "resume_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_skills_resume_id",
                schema: "resumes",
                table: "skills",
                column: "resume_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "courses",
                schema: "resumes");

            migrationBuilder.DropTable(
                name: "educations",
                schema: "resumes");

            migrationBuilder.DropTable(
                name: "employment_histories",
                schema: "resumes");

            migrationBuilder.DropTable(
                name: "languages",
                schema: "resumes");

            migrationBuilder.DropTable(
                name: "links",
                schema: "resumes");

            migrationBuilder.DropTable(
                name: "personal_details",
                schema: "resumes");

            migrationBuilder.DropTable(
                name: "skills",
                schema: "resumes");

            migrationBuilder.DropTable(
                name: "resumes",
                schema: "resumes");
        }
    }
}
