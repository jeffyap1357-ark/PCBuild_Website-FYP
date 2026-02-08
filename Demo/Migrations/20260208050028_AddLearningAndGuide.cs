using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningAndGuide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningMaterials",
                columns: table => new
                {
                    MaterialId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningMaterials", x => x.MaterialId);
                });

            migrationBuilder.CreateTable(
                name: "UserInterests",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    MarkedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInterests", x => new { x.UserId, x.MaterialId });
                    table.ForeignKey(
                        name: "FK_UserInterests_LearningMaterials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "LearningMaterials",
                        principalColumn: "MaterialId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserInterests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProgresses",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProgresses", x => new { x.UserId, x.MaterialId });
                    table.ForeignKey(
                        name: "FK_UserProgresses_LearningMaterials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "LearningMaterials",
                        principalColumn: "MaterialId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserInterests_MaterialId",
                table: "UserInterests",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProgresses_MaterialId",
                table: "UserProgresses",
                column: "MaterialId");

            // Seed sample learning materials
            migrationBuilder.InsertData(
                table: "LearningMaterials",
                columns: new[] { "Title", "Category", "Content", "SortOrder" },
                values: new object[,]
                {
                    { "PC Build Basics", "Getting Started", "<h3>Welcome to PC Building</h3><p>Building your own PC can be rewarding and cost-effective. This guide covers the basics you need to know before starting.</p><h4>What You'll Need</h4><ul><li>A clear workspace</li><li>Phillips screwdriver</li><li>Anti-static wrist strap (recommended)</li><li>Your components</li></ul>", 1 },
                    { "Choosing a CPU", "Components", "<h3>CPU Selection Guide</h3><p>The CPU (Central Processing Unit) is the brain of your computer. Consider these factors:</p><ul><li><strong>Core count:</strong> More cores help with multitasking</li><li><strong>Clock speed:</strong> Higher MHz = faster single-thread performance</li><li><strong>Budget:</strong> AMD and Intel both offer good options</li></ul>", 2 },
                    { "Motherboard Selection", "Components", "<h3>Selecting the Right Motherboard</h3><p>Your motherboard must match your CPU socket and fit your case. Check compatibility before buying.</p><p>Key features: RAM slots, M.2 slots, PCIe slots, and expansion options.</p>", 3 },
                    { "RAM and Storage", "Components", "<h3>Memory & Storage</h3><p><strong>RAM:</strong> 16GB is minimum for gaming; 32GB for heavy workloads.</p><p><strong>Storage:</strong> SSD for OS and games; consider NVMe for faster speeds. HDD for bulk storage.</p>", 4 },
                    { "GPU Guide", "Components", "<h3>Graphics Card Basics</h3><p>The GPU handles all visual output. For gaming, this is often the most important component.</p><p>Match your GPU to your monitor resolution: 1080p, 1440p, or 4K.</p>", 5 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserInterests");

            migrationBuilder.DropTable(
                name: "UserProgresses");

            migrationBuilder.DropTable(
                name: "LearningMaterials");
        }
    }
}
