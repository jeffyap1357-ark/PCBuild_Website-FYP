global using Demo.Models;
global using Demo;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authentication.Cookies;
global using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// Use absolute path so DB.mdf is always found regardless of working directory
var dbPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "DB.mdf"));
var connStr = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={dbPath};Integrated Security=True";
builder.Services.AddSqlServer<DB>(connStr);
builder.Services.AddScoped<Helper>();
builder.Services.AddScoped<Demo.Services.PcRecommendService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });
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

// Seed real learning material (Build PC, hardware chapters, Install OS)
try
{
    var seedPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "SeedLearningGuides.sql");
    if (File.Exists(seedPath))
    {
        using var scopeSeed = app.Services.CreateScope();
        var dbSeed = scopeSeed.ServiceProvider.GetRequiredService<DB>();
        var sql = File.ReadAllText(seedPath);
        dbSeed.Database.ExecuteSqlRaw(sql);
        logger.LogInformation("Learning guides seed executed from Data/SeedLearningGuides.sql");
    }
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Learning guides seed failed. You can run Data/SeedLearningGuides.sql manually.");
}

// Ensure Guide has ContentModules column (for admin Learning Material Maker)
try
{
    using var scopeGuide = app.Services.CreateScope();
    var dbGuide = scopeGuide.ServiceProvider.GetRequiredService<DB>();
    dbGuide.Database.ExecuteSqlRaw(@"
        IF EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'dbo' AND t.name = N'Guide')
        AND NOT EXISTS (SELECT 1 FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'dbo' AND t.name = N'Guide' AND c.name = N'ContentModules')
        ALTER TABLE [dbo].[Guide] ADD [ContentModules] nvarchar(max) NULL;
    ");
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Guide ContentModules column may not exist. Add it manually if using Learning Material Maker.");
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
                CONSTRAINT [FK_PerformanceReportItems_Component_HardwareId] FOREIGN KEY ([HardwareId]) REFERENCES [dbo].[Component]([ComponentId]) ON DELETE CASCADE
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

// Ensure Quiz tables exist and seed sample questions (fallback)
try
{
    using var scope3 = app.Services.CreateScope();
    var db3 = scope3.ServiceProvider.GetRequiredService<DB>();
    db3.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'dbo' AND t.name = N'QuizQuestions')
        BEGIN
            CREATE TABLE [dbo].[QuizQuestions](
                [QuizQuestionId] int IDENTITY NOT NULL,
                [QuestionText] nvarchar(500) NOT NULL,
                [OptionA] nvarchar(200) NOT NULL,
                [OptionB] nvarchar(200) NOT NULL,
                [OptionC] nvarchar(200) NOT NULL,
                [OptionD] nvarchar(200) NOT NULL,
                [CorrectOption] nvarchar(1) NOT NULL,
                [IsActive] bit NOT NULL,
                CONSTRAINT [PK_QuizQuestions] PRIMARY KEY ([QuizQuestionId])
            );
            INSERT INTO [dbo].[QuizQuestions]([QuestionText],[OptionA],[OptionB],[OptionC],[OptionD],[CorrectOption],[IsActive]) VALUES
            (N'Which component is considered the ""brain"" of the computer?', N'GPU', N'CPU', N'SSD', N'Power supply', N'B', 1),
            (N'Which part stores your operating system and files permanently?', N'RAM', N'CPU', N'SSD/HDD', N'GPU', N'C', 1),
            (N'Which socket type do most recent AMD desktop CPUs use?', N'LGA1700', N'AM4/AM5', N'LGA1200', N'TR4', N'B', 1),
            (N'If you buy a DDR5 motherboard, which RAM should you choose?', N'DDR3', N'DDR4', N'DDR5', N'Any type', N'C', 1),
            (N'Which component mainly affects gaming performance at 1440p resolution?', N'CPU', N'GPU', N'Case', N'Power cable', N'B', 1),
            (N'What is the recommended minimum RAM size for modern gaming PCs?', N'4GB', N'8GB', N'16GB', N'64GB', N'C', 1),
            (N'Which component supplies power to all PC parts?', N'Power supply unit (PSU)', N'GPU', N'Motherboard', N'CPU cooler', N'A', 1),
            (N'What does an SSD generally provide compared to an HDD?', N'Slower boot times', N'Faster boot and load times', N'Lower capacity only', N'No difference', N'B', 1),
            (N'Which factor ensures a motherboard physically fits inside a PC case?', N'CPU brand', N'RAM speed', N'Form factor (ATX, mATX, ITX)', N'Number of USB ports', N'C', 1),
            (N'Which cooling option is usually quieter but more complex to install?', N'Air cooler', N'Passive heatsink', N'All-in-one (AIO) liquid cooler', N'No cooler needed', N'C', 1);
        END;
        IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'dbo' AND t.name = N'QuizAttempts')
        BEGIN
            CREATE TABLE [dbo].[QuizAttempts](
                [QuizAttemptId] int IDENTITY NOT NULL,
                [UserId] int NOT NULL,
                [TakenAt] datetime2 NOT NULL,
                [TotalQuestions] int NOT NULL,
                [CorrectCount] int NOT NULL,
                CONSTRAINT [PK_QuizAttempts] PRIMARY KEY ([QuizAttemptId]),
                CONSTRAINT [FK_QuizAttempts_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId]) ON DELETE CASCADE
            );
        END;
        IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'dbo' AND t.name = N'QuizAttemptDetails')
        BEGIN
            CREATE TABLE [dbo].[QuizAttemptDetails](
                [QuizAttemptDetailId] int IDENTITY NOT NULL,
                [QuizAttemptId] int NOT NULL,
                [QuizQuestionId] int NOT NULL,
                [SelectedOption] nvarchar(1) NULL,
                [IsCorrect] bit NOT NULL,
                CONSTRAINT [PK_QuizAttemptDetails] PRIMARY KEY ([QuizAttemptDetailId]),
                CONSTRAINT [FK_QuizAttemptDetails_QuizAttempts_QuizAttemptId] FOREIGN KEY ([QuizAttemptId]) REFERENCES [dbo].[QuizAttempts]([QuizAttemptId]) ON DELETE CASCADE,
                CONSTRAINT [FK_QuizAttemptDetails_QuizQuestions_QuizQuestionId] FOREIGN KEY ([QuizQuestionId]) REFERENCES [dbo].[QuizQuestions]([QuizQuestionId]) ON DELETE CASCADE
            );
            CREATE INDEX [IX_QuizAttemptDetails_QuizAttemptId] ON [dbo].[QuizAttemptDetails]([QuizAttemptId]);
            CREATE INDEX [IX_QuizAttemptDetails_QuizQuestionId] ON [dbo].[QuizAttemptDetails]([QuizQuestionId]);
        END
    ");
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Could not ensure Quiz tables exist.");
}

// Ensure QuizQuestions has Level and Explanation (EF model / quiz seeds require these)
try
{
    using var scopeQuizCols = app.Services.CreateScope();
    var dbQuizCols = scopeQuizCols.ServiceProvider.GetRequiredService<DB>();
    dbQuizCols.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('QuizQuestions') AND name = 'Level')
            ALTER TABLE [QuizQuestions] ADD [Level] nvarchar(20) NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('QuizQuestions') AND name = 'Explanation')
            ALTER TABLE [QuizQuestions] ADD [Explanation] nvarchar(1000) NULL;
    ");
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Could not ensure QuizQuestions Level/Explanation columns.");
}

// Seed Normal and Professional quiz questions if none exist
try
{
    using var scopeQuiz = app.Services.CreateScope();
    var dbQuiz = scopeQuiz.ServiceProvider.GetRequiredService<DB>();
    var hasNormal = dbQuiz.QuizQuestions.Any(q => q.Level == "Normal");
    var hasProfessional = dbQuiz.QuizQuestions.Any(q => q.Level == "Professional");
    if (!hasNormal || !hasProfessional)
    {
        var toAdd = new List<QuizQuestion>();
        if (!hasNormal)
        {
            toAdd.AddRange(new[]
            {
                new QuizQuestion { QuestionText = "What TDP range is typical for a mid-range gaming CPU?", OptionA = "35-65W", OptionB = "65-125W", OptionC = "125-250W", OptionD = "250W+", CorrectOption = "B", Level = "Normal", Explanation = "Most gaming CPUs (e.g. Ryzen 5, Core i5) sit in the 65-125W TDP range. High-end chips can exceed 125W.", IsActive = true },
                new QuizQuestion { QuestionText = "Which PCIe slot is typically used for the primary GPU?", OptionA = "x1", OptionB = "x4", OptionC = "x8", OptionD = "x16", CorrectOption = "D", Level = "Normal", Explanation = "The top x16 slot offers full bandwidth for the GPU. Use the slot closest to the CPU for best performance.", IsActive = true },
                new QuizQuestion { QuestionText = "What does \"dual channel\" mean for RAM?", OptionA = "Two sticks in one slot", OptionB = "Two sticks in paired slots for doubled bandwidth", OptionC = "Two different RAM brands", OptionD = "RAM with two colors", CorrectOption = "B", Level = "Normal", Explanation = "Dual channel uses two matched sticks in the correct slots (often A2+B2) to double memory bandwidth.", IsActive = true },
                new QuizQuestion { QuestionText = "Which PSU efficiency rating is best for a gaming build?", OptionA = "80 Plus", OptionB = "80 Plus Bronze", OptionC = "80 Plus Gold", OptionD = "80 Plus Titanium", CorrectOption = "D", Level = "Normal", Explanation = "Titanium is the highest 80 Plus tier. Gold is a good balance of efficiency and cost for most builds.", IsActive = true },
                new QuizQuestion { QuestionText = "What is the main benefit of an M.2 NVMe SSD over SATA?", OptionA = "Larger capacity", OptionB = "Much faster read/write speeds", OptionC = "Lower price", OptionD = "Easier installation", CorrectOption = "B", Level = "Normal", Explanation = "NVMe uses PCIe lanes for far higher throughput than SATA. Gen4 NVMe can exceed 7000 MB/s.", IsActive = true },
                new QuizQuestion { QuestionText = "Which CPU cooler type is best for heavy overclocking?", OptionA = "Stock cooler", OptionB = "Low-profile air", OptionC = "Tower air or 240mm+ AIO", OptionD = "No cooler", CorrectOption = "C", Level = "Normal", Explanation = "Overclocking increases heat. A beefy tower or 240mm+ AIO provides the thermal headroom needed.", IsActive = true },
                new QuizQuestion { QuestionText = "What does \"ATX\" refer to in a motherboard?", OptionA = "CPU socket type", OptionB = "Form factor / size standard", OptionC = "RAM type", OptionD = "GPU interface", CorrectOption = "B", Level = "Normal", Explanation = "ATX is a form factor that defines board dimensions and mounting. Common sizes: ATX, micro-ATX, Mini-ITX.", IsActive = true },
                new QuizQuestion { QuestionText = "Why might you need a BIOS update before installing a new CPU?", OptionA = "To add more RAM", OptionB = "To support newer CPU generations on the same socket", OptionC = "To enable RGB", OptionD = "To fix GPU drivers", CorrectOption = "B", Level = "Normal", Explanation = "Motherboards may need a BIOS update to recognize newer CPUs on the same socket (e.g. Ryzen 5000 on B450).", IsActive = true },
                new QuizQuestion { QuestionText = "What is the purpose of case airflow?", OptionA = "To look good", OptionB = "To remove heat from components", OptionC = "To power the PC", OptionD = "To store data", CorrectOption = "B", Level = "Normal", Explanation = "Good airflow (intake + exhaust fans) moves hot air out and cool air in, keeping temps stable.", IsActive = true },
                new QuizQuestion { QuestionText = "Which factor most affects RAM compatibility with a motherboard?", OptionA = "Brand", OptionB = "DDR generation and speed", OptionC = "Color", OptionD = "Price", CorrectOption = "B", Level = "Normal", Explanation = "DDR4/DDR5 are not interchangeable. The board's QVL lists tested speeds and capacities.", IsActive = true }
            });
        }
        if (!hasProfessional)
        {
            toAdd.AddRange(new[]
            {
                new QuizQuestion { QuestionText = "What does CPU \"boost clock\" vs \"base clock\" indicate?", OptionA = "Same thing", OptionB = "Base = sustained, Boost = short burst max", OptionC = "Boost is for gaming only", OptionD = "Base is overclocked", CorrectOption = "B", Level = "Professional", Explanation = "Base clock is the guaranteed all-core speed. Boost allows higher single/light-thread speeds for short periods.", IsActive = true },
                new QuizQuestion { QuestionText = "Which PCIe generation roughly doubles bandwidth over the previous?", OptionA = "Same gen, different slot", OptionB = "Each new gen (3.0→4.0→5.0) doubles bandwidth", OptionC = "Only 5.0 matters", OptionD = "Gen does not affect bandwidth", CorrectOption = "B", Level = "Professional", Explanation = "PCIe 4.0 doubles 3.0; 5.0 doubles 4.0. x16 Gen4 = 32 GB/s; Gen5 = 64 GB/s.", IsActive = true },
                new QuizQuestion { QuestionText = "What is \"thermal throttling\"?", OptionA = "CPU runs faster when hot", OptionB = "CPU slows down to prevent overheating", OptionC = "Cooler stops working", OptionD = "GPU takes over", CorrectOption = "B", Level = "Professional", Explanation = "When temps hit the limit, the CPU reduces clock speed to stay within safe thermal envelope.", IsActive = true },
                new QuizQuestion { QuestionText = "Why might a 750W PSU be insufficient for a high-end build?", OptionA = "Always enough", OptionB = "Peak power spikes (e.g. RTX 4090) can exceed rated wattage", OptionC = "PSUs lose power over time", OptionD = "Voltage matters more", CorrectOption = "B", Level = "Professional", Explanation = "High-end GPUs have transient spikes. A 4090 + 13900K can momentarily draw well over 750W.", IsActive = true },
                new QuizQuestion { QuestionText = "What does \"CL\" in RAM timings (e.g. CL16) mean?", OptionA = "Capacity Level", OptionB = "Cas Latency - cycles to first data", OptionC = "Clock Level", OptionD = "Compatibility Level", CorrectOption = "B", Level = "Professional", Explanation = "CAS Latency is the delay (in cycles) from request to first data. Lower CL = faster response at same speed.", IsActive = true },
                new QuizQuestion { QuestionText = "Which cooling solution is best for extreme overclocking (LN2 aside)?", OptionA = "Stock cooler", OptionB = "120mm AIO", OptionC = "Custom loop or 360mm+ AIO", OptionD = "Passive heatsink", CorrectOption = "C", Level = "Professional", Explanation = "Custom loops and large AIOs dissipate the most heat, essential for pushing clocks on high-TDP chips.", IsActive = true },
                new QuizQuestion { QuestionText = "What is the main trade-off with smaller form factors (e.g. ITX)?", OptionA = "Cheaper", OptionB = "Limited expansion and cooling options", OptionC = "Faster performance", OptionD = "More RAM slots", CorrectOption = "B", Level = "Professional", Explanation = "ITX boards have fewer slots, smaller coolers fit, and cable management is tighter. Trade size for flexibility.", IsActive = true },
                new QuizQuestion { QuestionText = "Why might you use a \"curve optimizer\" or per-core offset?", OptionA = "To reduce FPS", OptionB = "To fine-tune voltage/clock per core for better efficiency", OptionC = "To disable cores", OptionD = "To change RAM speed", CorrectOption = "B", Level = "Professional", Explanation = "Curve optimizer (AMD) or per-core offsets (Intel) let you undervolt or tweak individual cores for stability and temps.", IsActive = true },
                new QuizQuestion { QuestionText = "What does \"resizable BAR\" (ReBAR) do?", OptionA = "Increases RAM size", OptionB = "Lets CPU access full GPU VRAM at once for better performance", OptionC = "Fixes driver bugs", OptionD = "Enables RGB sync", CorrectOption = "B", Level = "Professional", Explanation = "ReBAR allows the CPU to access the entire GPU frame buffer at once instead of 256MB chunks, improving some games.", IsActive = true },
                new QuizQuestion { QuestionText = "Which factor most affects 1% low FPS in games?", OptionA = "Monitor refresh rate", OptionB = "CPU single-thread and RAM latency", OptionC = "GPU brand", OptionD = "Case airflow", CorrectOption = "B", Level = "Professional", Explanation = "1% lows (frame time consistency) are often CPU and memory-bound. Fast RAM and strong single-thread help.", IsActive = true }
            });
        }
        if (toAdd.Count > 0)
        {
            dbQuiz.QuizQuestions.AddRange(toAdd);
            dbQuiz.SaveChanges();
            logger.LogInformation("Seeded {Count} quiz questions for Normal/Professional levels.", toAdd.Count);
        }
    }
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Could not seed Normal/Professional quiz questions.");
}

// Ensure Feedback table exists (fallback)
try
{
    using var scope4 = app.Services.CreateScope();
    var db4 = scope4.ServiceProvider.GetRequiredService<DB>();
    db4.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT 1 FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = N'dbo' AND t.name = N'Feedbacks')
        BEGIN
            CREATE TABLE [dbo].[Feedbacks](
                [FeedbackId] int IDENTITY NOT NULL,
                [UserId] int NULL,
                [Rating] int NOT NULL,
                [Comment] nvarchar(1000) NULL,
                [ModuleName] nvarchar(200) NULL,
                [PageUrl] nvarchar(500) NULL,
                [CreatedAt] datetime2 NOT NULL,
                CONSTRAINT [PK_Feedbacks] PRIMARY KEY ([FeedbackId]),
                CONSTRAINT [FK_Feedbacks_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId]) ON DELETE SET NULL
            );
        END
    ");
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Could not ensure Feedback table exists.");
}

// Ensure Users table has Role and IsRootAdmin columns, seed root admin
try
{
    using var scopeUsers = app.Services.CreateScope();
    var dbUsers = scopeUsers.ServiceProvider.GetRequiredService<DB>();
    var helper = scopeUsers.ServiceProvider.GetRequiredService<Helper>();
    dbUsers.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Role')
        BEGIN
            ALTER TABLE [Users] ADD [Role] nvarchar(20) NOT NULL DEFAULT 'User';
        END
        ELSE
        BEGIN
            UPDATE [Users] SET [Role] = 'User' WHERE [Role] IS NULL OR [Role] = '';
        END;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IsRootAdmin')
        BEGIN
            ALTER TABLE [Users] ADD [IsRootAdmin] bit NOT NULL DEFAULT 0;
        END
        ELSE
        BEGIN
            UPDATE [Users] SET [IsRootAdmin] = 0 WHERE [IsRootAdmin] IS NULL;
        END;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IsBlocked')
        BEGIN
            ALTER TABLE [Users] ADD [IsBlocked] bit NOT NULL DEFAULT 0;
        END
        ELSE
        BEGIN
            UPDATE [Users] SET [IsBlocked] = 0 WHERE [IsBlocked] IS NULL;
        END;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Avatar')
            ALTER TABLE [Users] ADD [Avatar] nvarchar(100) NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Gender')
            ALTER TABLE [Users] ADD [Gender] nvarchar(20) NULL;
    ");
    var rootEmail = builder.Configuration["RootAdmin:Email"] ?? "admin@pcbuild.local";
    var rootPassword = builder.Configuration["RootAdmin:Password"] ?? "Admin@123";
    var rootAdmin = dbUsers.Users.FirstOrDefault(u => u.IsRootAdmin);
    if (rootAdmin == null)
    {
        var existingAdmin = dbUsers.Users.FirstOrDefault(u => u.Username == "admin");
        if (existingAdmin != null)
        {
            existingAdmin.IsRootAdmin = true;
            existingAdmin.Role = Roles.Admin;
            existingAdmin.Email = rootEmail;
            existingAdmin.Password = helper.HashPassword(rootPassword);
        }
        else
        {
            dbUsers.Users.Add(new User
            {
                Username = "admin",
                Email = rootEmail,
                Password = helper.HashPassword(rootPassword),
                Role = Roles.Admin,
                IsRootAdmin = true
            });
        }
        dbUsers.SaveChanges();
        logger.LogInformation("Root admin account created. Login with Email: {Email}", rootEmail);
    }
    else
    {
        rootAdmin.Email = rootEmail;
        rootAdmin.Password = helper.HashPassword(rootPassword);
        dbUsers.SaveChanges();
    }
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Could not ensure root admin.");
}

// Ensure Component has performance attribute columns (fallback if migration not applied)
try
{
    using var scopeHw = app.Services.CreateScope();
    var dbHw = scopeHw.ServiceProvider.GetRequiredService<DB>();
    dbHw.Database.ExecuteSqlRaw(@"
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Component') AND name = 'ReleaseYear')
            ALTER TABLE [Component] ADD [ReleaseYear] int NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Component') AND name = 'Rating')
            ALTER TABLE [Component] ADD [Rating] int NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Component') AND name = 'PerformanceScore')
            ALTER TABLE [Component] ADD [PerformanceScore] int NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Component') AND name = 'PowerConsumption')
            ALTER TABLE [Component] ADD [PowerConsumption] int NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Component') AND name = 'GamingScore')
            ALTER TABLE [Component] ADD [GamingScore] int NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Component') AND name = 'OfficeScore')
            ALTER TABLE [Component] ADD [OfficeScore] int NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Component') AND name = 'EditingScore')
            ALTER TABLE [Component] ADD [EditingScore] int NULL;
        IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Component') AND name = 'Reference')
            ALTER TABLE [Component] ADD [Reference] nvarchar(500) NULL;

        -- Rewire foreign keys from Hardware -> Component while keeping HardwareId column names.
        DECLARE @sql nvarchar(max);

        IF OBJECT_ID('dbo.BuildDetails') IS NOT NULL
        BEGIN
            SET @sql = N'';
            SELECT @sql = @sql + N'ALTER TABLE [' + OBJECT_SCHEMA_NAME(parent_object_id) + N'].[' + OBJECT_NAME(parent_object_id) + N'] DROP CONSTRAINT [' + name + N'];'
            FROM sys.foreign_keys
            WHERE parent_object_id = OBJECT_ID('dbo.BuildDetails') AND referenced_object_id = OBJECT_ID('dbo.Hardware');
            IF (@sql <> N'') EXEC sp_executesql @sql;

            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('dbo.BuildDetails') AND referenced_object_id = OBJECT_ID('dbo.Component'))
                ALTER TABLE [BuildDetails] WITH CHECK ADD CONSTRAINT [FK_BuildDetails_Component_HardwareId] FOREIGN KEY ([HardwareId]) REFERENCES [Component]([ComponentId]) ON DELETE CASCADE;
        END

        IF OBJECT_ID('dbo.DraftBuildDetails') IS NOT NULL
        BEGIN
            SET @sql = N'';
            SELECT @sql = @sql + N'ALTER TABLE [' + OBJECT_SCHEMA_NAME(parent_object_id) + N'].[' + OBJECT_NAME(parent_object_id) + N'] DROP CONSTRAINT [' + name + N'];'
            FROM sys.foreign_keys
            WHERE parent_object_id = OBJECT_ID('dbo.DraftBuildDetails') AND referenced_object_id = OBJECT_ID('dbo.Hardware');
            IF (@sql <> N'') EXEC sp_executesql @sql;

            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('dbo.DraftBuildDetails') AND referenced_object_id = OBJECT_ID('dbo.Component'))
                ALTER TABLE [DraftBuildDetails] WITH CHECK ADD CONSTRAINT [FK_DraftBuildDetails_Component_HardwareId] FOREIGN KEY ([HardwareId]) REFERENCES [Component]([ComponentId]) ON DELETE CASCADE;
        END

        IF OBJECT_ID('dbo.PerformanceReportItems') IS NOT NULL
        BEGIN
            SET @sql = N'';
            SELECT @sql = @sql + N'ALTER TABLE [' + OBJECT_SCHEMA_NAME(parent_object_id) + N'].[' + OBJECT_NAME(parent_object_id) + N'] DROP CONSTRAINT [' + name + N'];'
            FROM sys.foreign_keys
            WHERE parent_object_id = OBJECT_ID('dbo.PerformanceReportItems') AND referenced_object_id = OBJECT_ID('dbo.Hardware');
            IF (@sql <> N'') EXEC sp_executesql @sql;

            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('dbo.PerformanceReportItems') AND referenced_object_id = OBJECT_ID('dbo.Component'))
                ALTER TABLE [PerformanceReportItems] WITH CHECK ADD CONSTRAINT [FK_PerformanceReportItems_Component_HardwareId] FOREIGN KEY ([HardwareId]) REFERENCES [Component]([ComponentId]) ON DELETE CASCADE;
        END
    ");
}
catch (Exception ex)
{
    logger.LogWarning(ex, "Could not add Component performance columns.");
}

// Import hardware from CSV if file exists
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<DB>();
    var csvPath = Path.Combine(builder.Environment.ContentRootPath, "Data", "hardware_table.csv");
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
app.Use(async (ctx, next) =>
{
    if (ctx.User.Identity?.IsAuthenticated == true)
    {
        var name = ctx.User.Identity.Name;
        if (!string.IsNullOrEmpty(name))
        {
            await using var scope = ctx.RequestServices.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<DB>();
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == name);
            if (user != null && user.IsBlocked)
            {
                await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                var qs = "?blocked=1";
                if (!ctx.Request.Path.StartsWithSegments("/Account/Login"))
                {
                    ctx.Response.Redirect("/Account/Login" + qs);
                    return;
                }
            }
        }
    }
    await next();
});
app.UseAuthorization();
// Support /Hardware/5 as shorthand for /Hardware/Detail/5
app.MapControllerRoute("HardwareById", "Hardware/{id:int}", new { controller = "Hardware", action = "Detail" });
app.MapDefaultControllerRoute();

logger.LogInformation("Ready. Open http://localhost:5006 or https://localhost:7143 in your browser.");
app.Run();
