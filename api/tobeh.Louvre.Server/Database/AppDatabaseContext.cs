using Microsoft.EntityFrameworkCore;
using tobeh.Louvre.Server.Database.Model;

namespace tobeh.Louvre.Server.Database;

public class AppDatabaseContext : DbContext
{
    public readonly string DbDirectory = "./data";
    public string DbPath => Path.Combine(DbDirectory, "app.db");

    public DbSet<UserEntity> Users { get; set; }
    public DbSet<RenderEntity> Renders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Use SQLite as the database provider
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }
}