using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo.Migrations
{
    /// <inheritdoc />
    public partial class AddGuideTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guide",
                columns: table => new
                {
                    GuideId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuideName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GuideDesc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guide", x => x.GuideId);
                });

            migrationBuilder.CreateTable(
                name: "UserGuideProgresses",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    GuideId = table.Column<int>(type: "int", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGuideProgresses", x => new { x.UserId, x.GuideId });
                    table.ForeignKey(
                        name: "FK_UserGuideProgresses_Guide_GuideId",
                        column: x => x.GuideId,
                        principalTable: "Guide",
                        principalColumn: "GuideId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGuideProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserGuideProgresses_GuideId",
                table: "UserGuideProgresses",
                column: "GuideId");

            migrationBuilder.InsertData(
                table: "Guide",
                columns: new[] { "GuideName", "GuideDesc", "Content", "SortOrder" },
                values: new object[,]
                {
                    { "PC Build Basics", "Introduction to building your own PC", "<h3>Welcome to PC Building</h3><p>Building your own PC can be rewarding and cost-effective. This guide covers the basics.</p><h4>What You Need</h4><ul><li>A clear workspace</li><li>Phillips screwdriver</li><li>Anti-static wrist strap</li><li>Your components</li></ul>", 1 },
                    { "Choosing a CPU", "How to select the right processor", "<h3>CPU Selection Guide</h3><p>The CPU is the brain of your computer. Consider core count, clock speed, and budget.</p>", 2 },
                    { "Motherboard Selection", "Matching your motherboard to your build", "<h3>Selecting the Right Motherboard</h3><p>Your motherboard must match your CPU socket and fit your case. Check compatibility before buying.</p>", 3 },
                    { "RAM and Storage", "Memory and storage options", "<h3>Memory and Storage</h3><p>16GB RAM minimum for gaming. SSD for OS and games. HDD for bulk storage.</p>", 4 },
                    { "GPU Guide", "Graphics card basics", "<h3>Graphics Card Basics</h3><p>The GPU handles visual output. Match it to your monitor resolution: 1080p, 1440p, or 4K.</p>", 5 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGuideProgresses");

            migrationBuilder.DropTable(
                name: "Guide");
        }
    }
}
