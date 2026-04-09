using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCheck.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class BigModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileHash",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "FileStoragePath",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "StudentName",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "SubjectTitle",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "TeacherComment",
                table: "Submissions");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "Submissions",
                newName: "CurrentVersion");

            migrationBuilder.RenameColumn(
                name: "ReceiveAt",
                table: "Submissions",
                newName: "LastActivityAt");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignmentId",
                table: "Submissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "HasLateUpload",
                table: "Submissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "Submissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileStoragePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FileHash = table.Column<string>(type: "text", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsLate = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Histories_Submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "Submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionVersion = table.Column<int>(type: "integer", nullable: false),
                    Grade = table.Column<int>(type: "integer", nullable: true),
                    TeacherComment = table.Column<string>(type: "text", nullable: true),
                    CheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Group = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Semester = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignments_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_StudentId_AssignmentId",
                table: "Submissions",
                columns: new[] { "StudentId", "AssignmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_SubjectId",
                table: "Assignments",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_FileHash",
                table: "Histories",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_Histories_SubmissionId",
                table: "Histories",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Email",
                table: "Students",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropTable(
                name: "Histories");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_StudentId_AssignmentId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "AssignmentId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "HasLateUpload",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Submissions");

            migrationBuilder.RenameColumn(
                name: "LastActivityAt",
                table: "Submissions",
                newName: "ReceiveAt");

            migrationBuilder.RenameColumn(
                name: "CurrentVersion",
                table: "Submissions",
                newName: "Version");

            migrationBuilder.AddColumn<string>(
                name: "FileHash",
                table: "Submissions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Submissions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileStoragePath",
                table: "Submissions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "Submissions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "Submissions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StudentName",
                table: "Submissions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubjectTitle",
                table: "Submissions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TeacherComment",
                table: "Submissions",
                type: "text",
                nullable: true);
        }
    }
}
