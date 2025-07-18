using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Minio;
using tobeh.Louvre.Server.Authentication;
using tobeh.Louvre.Server.Config;
using tobeh.Louvre.Server.Database;
using tobeh.Louvre.Server.Host;
using tobeh.Louvre.Server.Service;

namespace tobeh.Louvre.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // if not openapi generator active, add runtime services
        if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
        {
            builder.Services.AddMinio(configure => configure
                .WithCredentials(builder.Configuration.GetValue<string>("S3:AccessKey"),
                    builder.Configuration.GetValue<string>("S3:SecretKey"))
                .WithEndpoint(builder.Configuration.GetValue<string>("S3:Endpoint"))
                .WithSSL(false)
                .Build()
            );
        
            builder.Services.AddDbContext<AppDatabaseContext>();
            builder.Services.AddSingleton<AuthorizedUserCacheService>();
            builder.Services.AddSingleton<RenderSubmissionDispatcherService>();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<UsersService>();
            builder.Services.AddScoped<RendersService>();
            builder.Services.AddScoped<RenderingService>();
            builder.Services.AddScoped<StorageService>();
            builder.Services.AddScoped<TypoApiClientService>();
            builder.Services.AddScoped<AuthorizationService>();
            builder.Services.AddScoped<TypoCloudService>();
            builder.Services.AddScoped<RenderSubmissionWorkerService>();
            builder.Services.Configure<TypoApiConfig>(builder.Configuration.GetSection("TypoApi"));
            builder.Services.Configure<RendererConfig>(builder.Configuration.GetSection("Renderer"));
            builder.Services.Configure<S3Config>(builder.Configuration.GetSection("S3"));
            
            builder.Services.AddHostedService<MinioBucketSetupService>();
            builder.Services.AddHostedService<DatabaseSetupService>();
        }

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddOpenApi("louvre", options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
        });
        
        // add typo authentication scheme for typo bearer token
        builder.Services.AddAuthentication(TypoTokenAuthenticationHandler.Scheme)
            .AddScheme<AuthenticationSchemeOptions, TypoTokenAuthenticationHandler>(
                TypoTokenAuthenticationHandler.Scheme, null);

        var app = builder.Build();

        app.MapOpenApi();
        app.UseAuthorization();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/louvre.json", "Louvre API");
        });

        app.MapControllers();

        app.Run();
    }
}