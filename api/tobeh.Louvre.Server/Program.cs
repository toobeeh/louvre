using System.Reflection;
using System.Text.Json.Serialization;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using tobeh.Louvre.Server.Authorization;
using tobeh.Louvre.Server.Config;
using tobeh.Louvre.Server.Database;
using tobeh.Louvre.Server.Database.Model;
using tobeh.Louvre.Server.Host;
using tobeh.Louvre.Server.Mapper;
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
            builder.Services.AddSingleton<RenderTaskDispatcherService>();
            builder.Services.AddHttpClient();
            builder.Services.AddHttpClient<TypoApiClientService>();
            builder.Services.AddScoped<UsersService>();
            builder.Services.AddScoped<RendersService>();
            builder.Services.AddScoped<RenderingService>();
            builder.Services.AddScoped<StorageService>();
            builder.Services.AddScoped<TypoApiClientService>();
            builder.Services.AddScoped<AuthorizationService>();
            builder.Services.AddScoped<UserRequestContext>();
            builder.Services.AddScoped<TypoCloudService>();
            builder.Services.AddScoped<RenderTaskWorkerService>();
            builder.Services.AddScoped<IAuthorizationHandler, RoleRequirementHandler>();
            builder.Services.Configure<TypoApiConfig>(builder.Configuration.GetSection("TypoApi"));
            builder.Services.Configure<RendererConfig>(builder.Configuration.GetSection("Renderer"));
            builder.Services.Configure<S3Config>(builder.Configuration.GetSection("S3"));
            builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            
            builder.Services.AddHostedService<MinioBucketSetupService>();
            builder.Services.AddHostedService<DatabaseSetupService>();

            builder.Services.AddAutoMapper(config => config.AddProfile(typeof(MapperProfile)));
        }

        // add authentication via jwt/openid connect
        builder.Services
            .AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", jwtOptions =>
            {
                jwtOptions.Authority = "https://api.typo.rip/openid";
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = "https://api.louvre.tobeh.host"
                };
            });
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        builder.Services.AddAuthorization(options =>
        {
            foreach (UserTypeEnum userType in Enum.GetValuesAsUnderlyingType<UserTypeEnum>())
            {
                options.AddPolicy($"Role:{userType.ToString()}",
                    policy => policy.Requirements.Add(new RoleRequirement(userType)));
            }
        });

        // register controllers
        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();

        // set up swagger & openapi
        builder.Services.AddSwaggerGen(config =>
        {
            config.SwaggerDoc("louvre", new () {Title = "Louvre API", Version = "v1"});
            config.SupportNonNullableReferenceTypes();
            /*config.SchemaFilter<SwaggerRequiredSchemaFilter>();*/
            // Use method name as operationId
            config.CustomOperationIds(apiDesc => apiDesc.TryGetMethodInfo(out var methodInfo) ? methodInfo.Name : null);

            config.AddSecurityDefinition("openid", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OpenIdConnect,
                OpenIdConnectUrl = new Uri("https://api.typo.rip/openid/.well-known/openid-configuration"),
                Description = "OAuth2 AuthorizationCode flow"
            });
            
            config.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "openid"
                        }
                    },
                    []
                }
            });
            
            config.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri("https://placeholder-for-generator"),
                        TokenUrl = new Uri("https://placeholder-for-generator"),
                    }
                },
                Description = "OAuth2 AuthorizationCode flow for API generator"
            });
            
            config.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    []
                }
            });
        });

        var app = builder.Build();

        app.UseCors("AllowAll");
        app.UseAuthorization();
        app.MapOpenApi();
        app.UseSwagger(config => config.RouteTemplate = "openapi/{documentName}.json");
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/louvre.json", "Louvre API");
            options.OAuthClientId(builder.Configuration.GetValue<string>("TypoApi:OauthClientId"));
        });

        app.MapControllers();

        app.Run();
    }
}