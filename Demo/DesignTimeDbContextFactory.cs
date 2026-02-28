using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Demo;

/// <summary>EF design-time factory so migrations use the same DB.mdf connection.</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<Models.DB>
{
    public Models.DB CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connStr = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connStr))
        {
            var dbPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "DB.mdf"));
            connStr = $@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={dbPath};Integrated Security=True";
        }
        else
        {
            connStr = connStr.Replace("|DataDirectory|", Path.GetFullPath(Directory.GetCurrentDirectory()) + Path.DirectorySeparatorChar);
        }

        var options = new DbContextOptionsBuilder<Models.DB>()
            .UseSqlServer(connStr)
            .Options;

        return new Models.DB(options);
    }
}
