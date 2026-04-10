using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCheck.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AssignmentRemoveOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Assignments");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_AssignmentId",
                table: "Submissions",
                column: "AssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Assignments_AssignmentId",
                table: "Submissions",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Students_StudentId",
                table: "Submissions",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Assignments_AssignmentId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Students_StudentId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_AssignmentId",
                table: "Submissions");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Assignments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
