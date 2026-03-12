using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo.Migrations
{
    /// <inheritdoc />
    public partial class AddHardwareReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "Guide",
                newName: "Guide",
                newSchema: "dbo");

            migrationBuilder.AddColumn<string>(
                name: "Reference",
                table: "Hardware",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsShared",
                table: "Builds",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "SharedAt",
                table: "Builds",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Community",
                schema: "dbo",
                columns: table => new
                {
                    CommunityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BuildName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    SharedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: true),
                    RatingCount = table.Column<int>(type: "int", nullable: false),
                    AuthorComment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Image = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Community", x => x.CommunityId);
                    table.ForeignKey(
                        name: "FK_Community_Builds_BuildId",
                        column: x => x.BuildId,
                        principalTable: "Builds",
                        principalColumn: "BuildId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Community_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DraftBuilds",
                schema: "dbo",
                columns: table => new
                {
                    DraftBuildId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BuildName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftBuilds", x => x.DraftBuildId);
                    table.ForeignKey(
                        name: "FK_DraftBuilds_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    FeedbackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ModuleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "PerformanceReports",
                columns: table => new
                {
                    PerformanceReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReportName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Budget = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceReports", x => x.PerformanceReportId);
                    table.ForeignKey(
                        name: "FK_PerformanceReports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizAttempts",
                columns: table => new
                {
                    QuizAttemptId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TakenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    CorrectCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttempts", x => x.QuizAttemptId);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizQuestions",
                columns: table => new
                {
                    QuizQuestionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OptionA = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OptionB = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OptionC = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OptionD = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CorrectOption = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizQuestions", x => x.QuizQuestionId);
                });

            migrationBuilder.CreateTable(
                name: "CommunityComments",
                schema: "dbo",
                columns: table => new
                {
                    CommunityCommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommunityId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityComments", x => x.CommunityCommentId);
                    table.ForeignKey(
                        name: "FK_CommunityComments_Community_CommunityId",
                        column: x => x.CommunityId,
                        principalSchema: "dbo",
                        principalTable: "Community",
                        principalColumn: "CommunityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunityComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommunityRatings",
                schema: "dbo",
                columns: table => new
                {
                    CommunityRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommunityId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityRatings", x => x.CommunityRatingId);
                    table.ForeignKey(
                        name: "FK_CommunityRatings_Community_CommunityId",
                        column: x => x.CommunityId,
                        principalSchema: "dbo",
                        principalTable: "Community",
                        principalColumn: "CommunityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommunityRatings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DraftBuildDetails",
                schema: "dbo",
                columns: table => new
                {
                    DraftBuildDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DraftBuildId = table.Column<int>(type: "int", nullable: false),
                    HardwareId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftBuildDetails", x => x.DraftBuildDetailId);
                    table.ForeignKey(
                        name: "FK_DraftBuildDetails_DraftBuilds_DraftBuildId",
                        column: x => x.DraftBuildId,
                        principalSchema: "dbo",
                        principalTable: "DraftBuilds",
                        principalColumn: "DraftBuildId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DraftBuildDetails_Hardware_HardwareId",
                        column: x => x.HardwareId,
                        principalTable: "Hardware",
                        principalColumn: "HardwareId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerformanceReportItems",
                columns: table => new
                {
                    PerformanceReportItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PerformanceReportId = table.Column<int>(type: "int", nullable: false),
                    HardwareId = table.Column<int>(type: "int", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceReportItems", x => x.PerformanceReportItemId);
                    table.ForeignKey(
                        name: "FK_PerformanceReportItems_Hardware_HardwareId",
                        column: x => x.HardwareId,
                        principalTable: "Hardware",
                        principalColumn: "HardwareId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PerformanceReportItems_PerformanceReports_PerformanceReportId",
                        column: x => x.PerformanceReportId,
                        principalTable: "PerformanceReports",
                        principalColumn: "PerformanceReportId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizAttemptDetails",
                columns: table => new
                {
                    QuizAttemptDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizAttemptId = table.Column<int>(type: "int", nullable: false),
                    QuizQuestionId = table.Column<int>(type: "int", nullable: false),
                    SelectedOption = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttemptDetails", x => x.QuizAttemptDetailId);
                    table.ForeignKey(
                        name: "FK_QuizAttemptDetails_QuizAttempts_QuizAttemptId",
                        column: x => x.QuizAttemptId,
                        principalTable: "QuizAttempts",
                        principalColumn: "QuizAttemptId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAttemptDetails_QuizQuestions_QuizQuestionId",
                        column: x => x.QuizQuestionId,
                        principalTable: "QuizQuestions",
                        principalColumn: "QuizQuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Community_BuildId",
                schema: "dbo",
                table: "Community",
                column: "BuildId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Community_UserId",
                schema: "dbo",
                table: "Community",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityComments_CommunityId",
                schema: "dbo",
                table: "CommunityComments",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityComments_UserId",
                schema: "dbo",
                table: "CommunityComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityRatings_CommunityId",
                schema: "dbo",
                table: "CommunityRatings",
                column: "CommunityId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityRatings_UserId",
                schema: "dbo",
                table: "CommunityRatings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftBuildDetails_DraftBuildId",
                schema: "dbo",
                table: "DraftBuildDetails",
                column: "DraftBuildId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftBuildDetails_HardwareId",
                schema: "dbo",
                table: "DraftBuildDetails",
                column: "HardwareId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftBuilds_UserId",
                schema: "dbo",
                table: "DraftBuilds",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_UserId",
                table: "Feedbacks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReportItems_HardwareId",
                table: "PerformanceReportItems",
                column: "HardwareId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReportItems_PerformanceReportId",
                table: "PerformanceReportItems",
                column: "PerformanceReportId");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceReports_UserId",
                table: "PerformanceReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptDetails_QuizAttemptId",
                table: "QuizAttemptDetails",
                column: "QuizAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttemptDetails_QuizQuestionId",
                table: "QuizAttemptDetails",
                column: "QuizQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_UserId",
                table: "QuizAttempts",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityComments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CommunityRatings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DraftBuildDetails",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "PerformanceReportItems");

            migrationBuilder.DropTable(
                name: "QuizAttemptDetails");

            migrationBuilder.DropTable(
                name: "Community",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "DraftBuilds",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PerformanceReports");

            migrationBuilder.DropTable(
                name: "QuizAttempts");

            migrationBuilder.DropTable(
                name: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "Reference",
                table: "Hardware");

            migrationBuilder.DropColumn(
                name: "IsShared",
                table: "Builds");

            migrationBuilder.DropColumn(
                name: "SharedAt",
                table: "Builds");

            migrationBuilder.RenameTable(
                name: "Guide",
                schema: "dbo",
                newName: "Guide");
        }
    }
}
