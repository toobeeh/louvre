using Minio;
using Minio.DataModel.Args;
using tobeh.Louvre.Server.Database;
using tobeh.Louvre.Server.Service;

namespace tobeh.Louvre.Server.Host;

public class DatabaseSetupService(
    ILogger<DatabaseSetupService> logger, 
    IServiceScopeFactory _scopeFactory
    ) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogTrace("StartAsync");
        
        var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDatabaseContext>();
        
        logger.LogInformation("Ensuring database is created at {DbPath}", db.DbPath);
        Directory.CreateDirectory(db.DbDirectory);
        var ctx = new AppDatabaseContext();
        await ctx.Database.EnsureCreatedAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}