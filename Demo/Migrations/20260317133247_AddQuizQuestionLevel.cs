using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizQuestionLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Component",
                newName: "Component",
                newSchema: "dbo");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "QuizQuestions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentModules",
                schema: "dbo",
                table: "Guide",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "ContentModules",
                schema: "dbo",
                table: "Guide");

            migrationBuilder.RenameTable(
                name: "Component",
                schema: "dbo",
                newName: "Component");
        }
    }
}
