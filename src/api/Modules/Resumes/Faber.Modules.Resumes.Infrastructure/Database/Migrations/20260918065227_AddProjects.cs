using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Faber.Modules.Resumes.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "projects",
                schema: "resumes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    url = table.Column<string>(type: "character varying(2083)", maxLength: 2083, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    resume_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.id);
                    table.ForeignKey(
                        name: "fk_projects_resumes_resume_id",
                        column: x => x.resume_id,
                        principalSchema: "resumes",
                        principalTable: "resumes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_projects_resume_id",
                schema: "resumes",
                table: "projects",
                column: "resume_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "projects",
                schema: "resumes");
        }
    }
}
