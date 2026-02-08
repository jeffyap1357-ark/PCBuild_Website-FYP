using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo.Migrations
{
    /// <inheritdoc />
    public partial class AddHardwareAndCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Hardware",
                columns: table => new
                {
                    HardwareId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hardware", x => x.HardwareId);
                    table.ForeignKey(
                        name: "FK_Hardware_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hardware_CategoryId",
                table: "Hardware",
                column: "CategoryId");

            // Seed categories: CPU, Motherboard, GPU, RAM, Storage, Power supply unit, CPU cooler, PC case
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "Type" },
                values: new object[,]
                {
                    { 1, "CPU" },
                    { 2, "Motherboard" },
                    { 3, "GPU" },
                    { 4, "RAM" },
                    { 5, "Storage" },
                    { 6, "Power supply unit" },
                    { 7, "CPU cooler" },
                    { 8, "PC case" }
                });

            // Seed sample hardware
            migrationBuilder.InsertData(
                table: "Hardware",
                columns: new[] { "HardwareId", "Name", "CategoryId", "Price", "Detail", "Brand" },
                values: new object[,]
                {
                    { 1, "Intel Core i5-12400", 1, 199.99m, "6 cores, 12 threads, up to 4.4 GHz", "Intel" },
                    { 2, "AMD Ryzen 5 5600X", 1, 229.99m, "6 cores, 12 threads, 3.7 GHz base", "AMD" },
                    { 3, "Intel Core i7-13700K", 1, 409.99m, "16 cores, 24 threads, up to 5.4 GHz", "Intel" },
                    { 4, "ASUS ROG Strix B650E-F", 2, 289.99m, "AM5, DDR5, PCIe 5.0", "ASUS" },
                    { 5, "MSI MAG B660M Mortar", 2, 159.99m, "LGA1700, DDR4, M-ATX", "MSI" },
                    { 6, "Gigabyte B550 AORUS Elite", 2, 149.99m, "AM4, DDR4, PCIe 4.0", "Gigabyte" },
                    { 7, "NVIDIA GeForce RTX 4060", 3, 299.99m, "8GB GDDR6, DLSS 3", "NVIDIA" },
                    { 8, "AMD Radeon RX 7600", 3, 269.99m, "8GB GDDR6, RDNA 3", "AMD" },
                    { 9, "NVIDIA GeForce RTX 4070", 3, 549.99m, "12GB GDDR6X, DLSS 3", "NVIDIA" },
                    { 10, "Corsair Vengeance 32GB DDR5", 4, 119.99m, "32GB (2x16GB), 5600MHz", "Corsair" },
                    { 11, "G.Skill Trident Z5 32GB", 4, 134.99m, "32GB (2x16GB), 6000MHz", "G.Skill" },
                    { 12, "Kingston FURY Beast 16GB DDR4", 4, 49.99m, "16GB (2x8GB), 3200MHz", "Kingston" },
                    { 13, "Samsung 970 EVO Plus 1TB", 5, 99.99m, "NVMe PCIe 3.0, 3500 MB/s read", "Samsung" },
                    { 14, "WD Black SN850X 1TB", 5, 129.99m, "NVMe PCIe 4.0, 7300 MB/s read", "WD" },
                    { 15, "Crucial P3 Plus 2TB", 5, 119.99m, "NVMe PCIe 4.0, 5000 MB/s read", "Crucial" },
                    { 16, "Corsair RM750x", 6, 119.99m, "750W, 80+ Gold, fully modular", "Corsair" },
                    { 17, "EVGA SuperNOVA 650 G5", 6, 89.99m, "650W, 80+ Gold, fully modular", "EVGA" },
                    { 18, "Seasonic FOCUS GX-850", 6, 139.99m, "850W, 80+ Gold, fully modular", "Seasonic" },
                    { 19, "Noctua NH-D15", 7, 109.99m, "Dual tower, 140mm fans", "Noctua" },
                    { 20, "Cooler Master Hyper 212", 7, 44.99m, "Single tower, 120mm fan", "Cooler Master" },
                    { 21, "be quiet! Dark Rock Pro 4", 7, 89.99m, "Dual tower, 135mm fan", "be quiet!" },
                    { 22, "NZXT H5 Flow", 8, 94.99m, "Mid tower, mesh front, tempered glass", "NZXT" },
                    { 23, "Lian Li O11 Dynamic", 8, 169.99m, "Mid tower, dual chamber, glass", "Lian Li" },
                    { 24, "Fractal Design Meshify C", 8, 109.99m, "Compact mid tower, mesh front", "Fractal Design" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Hardware");
            migrationBuilder.DropTable(name: "Categories");
        }
    }
}
