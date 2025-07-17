using Microsoft.AspNetCore.Authentication;
using toobeeh.Louvre.Server.Authentication;
using toobeeh.Louvre.Server.Config;
using toobeeh.Louvre.Server.Database;
using toobeeh.Louvre.Server.Service;

namespace toobeeh.Louvre.Server;

public class Program
{
    public static void Main(string[] args)
    {
        AppDatabaseContext.EnsureDatabaseExists();
        
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddOpenApi("louvre");
        
        // add typo authentication scheme for typo bearer token
        builder.Services.AddAuthentication(TypoTokenAuthenticationHandler.Scheme)
            .AddScheme<AuthenticationSchemeOptions, TypoTokenAuthenticationHandler>(
                TypoTokenAuthenticationHandler.Scheme, null);

        // add services
        builder.Services.AddDbContext<AppDatabaseContext>();
        builder.Services.AddSingleton<AuthorizedUserCacheService>();
        builder.Services.AddHttpClient();
        builder.Services.AddScoped<UsersService>();
        builder.Services.AddScoped<RendersService>();
        builder.Services.AddScoped<GifRenderService>();
        builder.Services.AddScoped<TypoApiClientService>();
        builder.Services.AddScoped<AuthorizationService>();
        builder.Services.AddScoped<TypoCloudService>();
        builder.Services.Configure<TypoApiConfig>(builder.Configuration.GetSection("TypoApi"));
        builder.Services.Configure<RendererConfig>(builder.Configuration.GetSection("Renderer"));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}