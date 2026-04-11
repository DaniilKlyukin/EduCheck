using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCheck.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Title",
                table: "Subjects",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_SubmissionId",
                table: "Reviews",
                column: "SubmissionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Submissions_SubmissionId",
                table: "Reviews",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Submissions_SubmissionId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_Title",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_SubmissionId",
                table: "Reviews");
        }
    }
}
