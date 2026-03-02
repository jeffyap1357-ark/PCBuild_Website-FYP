global using Demo.Models;
global using Demo;
global using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// Use absolute path so DB.mdf is always found regardless of working directory
var dbPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "DB.mdf"));
var connStr = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={dbPath};Integrated Security=True";
builder.Services.AddSqlServer<DB>(connStr);
builder.Services.AddScoped<Helper>();
builder.Services.AddScoped<Demo.Services.PcRecommendService>();
builder.Services.AddAuthentication().AddCookie();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();

var app = builder.Build();

// Apply pending migrations on startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DB>();
    db.Database.Migrate();
    logger.LogInformation("Migrations applied. DB path: {Path}", dbPath);
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Migrations could not be applied. Run 'dotnet ef database update' if needed.");
}

// Ensure Guide table exists (fallback if migrations failed or DB was created manually)
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DB>();
    var created = db.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'dbo' AND t.name = N'Guide')
        BEGIN
            CREATE TABLE [dbo].[Guide]([GuideId] int IDENTITY NOT NULL, [GuideName] nvarchar(200) NOT NULL, [GuideDesc] nvarchar(500) NULL, [Content] nvarchar(max) NOT NULL, [SortOrder] int NOT NULL, CONSTRAINT [PK_Guide] PRIMARY KEY ([GuideId]));
            IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'UserGuideProgresses')
            BEGIN
                CREATE TABLE [dbo].[UserGuideProgresses]([UserId] int NOT NULL, [GuideId] int NOT NULL, [ReadAt] datetime2 NOT NULL, CONSTRAINT [PK_UserGuideProgresses] PRIMARY KEY ([UserId],[GuideId]), CONSTRAINT [FK_UserGuideProgresses_Guide_GuideId] FOREIGN KEY ([GuideId]) REFERENCES [dbo].[Guide]([GuideId]) ON DELETE CASCADE, CONSTRAINT [FK_UserGuideProgresses_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId]) ON DELETE CASCADE);
                CREATE INDEX [IX_UserGuideProgresses_GuideId] ON [dbo].[UserGuideProgresses]([GuideId]);
            END
            INSERT INTO [dbo].[Guide]([GuideName],[GuideDesc],[Content],[SortOrder]) VALUES (N'PC Build Basics',N'Introduction to building your own PC',N'<h3>Welcome to PC Building</h3><p>Building your own PC can be rewarding and cost-effective.</p><h4>What You Need</h4><ul><li>A clear workspace</li><li>Phillips screwdriver</li><li>Anti-static wrist strap</li><li>Your components</li></ul>',1),(N'Choosing a CPU',N'How to select the right processor',N'<h3>CPU Selection Guide</h3><p>The CPU is the brain of your computer. Consider core count, clock speed, and budget.</p>',2),(N'Motherboard Selection',N'Matching your motherboard to your build',N'<h3>Selecting the Right Motherboard</h3><p>Your motherboard must match your CPU socket and fit your case.</p>',3),(N'RAM and Storage',N'Memory and storage options',N'<h3>Memory and Storage</h3><p>16GB RAM minimum for gaming. SSD for OS and games.</p>',4),(N'GPU Guide',N'Graphics card basics',N'<h3>Graphics Card Basics</h3><p>The GPU handles visual output. Match it to your monitor resolution.</p>',5);
        END
    ");
    if (created > 0) logger.LogInformation("Guide table created via fallback SQL.");
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Could not ensure Guide table exists. Visit /Learning/Diagnose to see tables in DB.");
}

// Ensure PerformanceReport tables exist (fallback if migrations failed)
try
{
    using var scope2 = app.Services.CreateScope();
    var db2 = scope2.ServiceProvider.GetRequiredService<DB>();
    db2.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'dbo' AND t.name = N'PerformanceReports')
        BEGIN
            CREATE TABLE [dbo].[PerformanceReports](
                [PerformanceReportId] int IDENTITY NOT NULL,
                [UserId] int NOT NULL,
                [ReportName] nvarchar(200) NULL,
                [CreatedAt] datetime2 NOT NULL,
                [Budget] decimal(10,2) NOT NULL,
                [TotalPrice] decimal(10,2) NOT NULL,
                [TotalScore] int NOT NULL,
                CONSTRAINT [PK_PerformanceReports] PRIMARY KEY ([PerformanceReportId]),
                CONSTRAINT [FK_PerformanceReports_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId]) ON DELETE CASCADE
            );
        END;
        IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'dbo' AND t.name = N'PerformanceReportItems')
        BEGIN
            CREATE TABLE [dbo].[PerformanceReportItems](
                [PerformanceReportItemId] int IDENTITY NOT NULL,
                [PerformanceReportId] int NOT NULL,
                [HardwareId] int NOT NULL,
                [CategoryName] nvarchar(100) NOT NULL,
                [Price] decimal(10,2) NOT NULL,
                [Score] int NOT NULL,
                CONSTRAINT [PK_PerformanceReportItems] PRIMARY KEY ([PerformanceReportItemId]),
                CONSTRAINT [FK_PerformanceReportItems_PerformanceReports_PerformanceReportId] FOREIGN KEY ([PerformanceReportId]) REFERENCES [dbo].[PerformanceReports]([PerformanceReportId]) ON DELETE CASCADE,
                CONSTRAINT [FK_PerformanceReportItems_Hardware_HardwareId] FOREIGN KEY ([HardwareId]) REFERENCES [dbo].[Hardware]([HardwareId]) ON DELETE CASCADE
            );
            CREATE INDEX [IX_PerformanceReportItems_PerformanceReportId] ON [dbo].[PerformanceReportItems]([PerformanceReportId]);
            CREATE INDEX [IX_PerformanceReportItems_HardwareId] ON [dbo].[PerformanceReportItems]([HardwareId]);
        END
    ");
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Could not ensure PerformanceReport tables exist.");
}

// Import hardware from CSV if file exists (run AddHardwareCompatibility.sql first for Socket/DdrType columns)
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DB>();
    var csvPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "pc_hardware_dataset_v2.csv");
    await Demo.Services.CsvHardwareImport.ImportFromCsvAsync(db, csvPath, logger);
}
catch (Exception ex)
{
    logger.LogWarning(ex, "CSV hardware import skipped or failed. Run AddHardwareCompatibility.sql first.");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRequestLocalization("en-MY");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultControllerRoute();
app.Run();
