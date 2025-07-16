using Microsoft.EntityFrameworkCore;
using toobeeh.Louvre.Server.Database.Model;

namespace toobeeh.Louvre.Server.Database;

public class AppDatabaseContext : DbContext
{
    private const string Path = "./data";
    private static string DbPath => System.IO.Path.Combine(Path, "app.db");

    public static void EnsureDatabaseExists()
    {
        Directory.CreateDirectory(Path);
        var ctx = new AppDatabaseContext();
        ctx.Database.EnsureCreated();
        ctx.Dispose();
    }

    public DbSet<UserEntity> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use SQLite as the database provider
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}