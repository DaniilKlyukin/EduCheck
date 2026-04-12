using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCheck.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SubjectCombiIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subjects_Title",
                table: "Subjects");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Title_Semester",
                table: "Subjects",
                columns: new[] { "Title", "Semester" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subjects_Title_Semester",
                table: "Subjects");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Title",
                table: "Subjects",
                column: "Title",
                unique: true);
        }
    }
}
