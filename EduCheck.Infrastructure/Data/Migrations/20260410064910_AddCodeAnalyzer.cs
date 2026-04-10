using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduCheck.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeAnalyzer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnalysisResult",
                table: "SubmissionHistory",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnalysisResult",
                table: "SubmissionHistory");
        }
    }
}
