using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCheck.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Histories_Submissions_SubmissionId",
                table: "Histories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Histories",
                table: "Histories");

            migrationBuilder.RenameTable(
                name: "Histories",
                newName: "SubmissionHistory");

            migrationBuilder.RenameIndex(
                name: "IX_Histories_SubmissionId",
                table: "SubmissionHistory",
                newName: "IX_SubmissionHistory_SubmissionId");

            migrationBuilder.RenameIndex(
                name: "IX_Histories_FileHash",
                table: "SubmissionHistory",
                newName: "IX_SubmissionHistory_FileHash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubmissionHistory",
                table: "SubmissionHistory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubmissionHistory_Submissions_SubmissionId",
                table: "SubmissionHistory",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubmissionHistory_Submissions_SubmissionId",
                table: "SubmissionHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubmissionHistory",
                table: "SubmissionHistory");

            migrationBuilder.RenameTable(
                name: "SubmissionHistory",
                newName: "Histories");

            migrationBuilder.RenameIndex(
                name: "IX_SubmissionHistory_SubmissionId",
                table: "Histories",
                newName: "IX_Histories_SubmissionId");

            migrationBuilder.RenameIndex(
                name: "IX_SubmissionHistory_FileHash",
                table: "Histories",
                newName: "IX_Histories_FileHash");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Histories",
                table: "Histories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Histories_Submissions_SubmissionId",
                table: "Histories",
                column: "SubmissionId",
                principalTable: "Submissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
