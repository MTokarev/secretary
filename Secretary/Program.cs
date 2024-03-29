using System.Reflection;
using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Secretary.ApiEndpoints;
using Secretary.Data;
using Secretary.Extensions;
using Secretary.Interfaces;
using Secretary.Models;
using Secretary.Options;
using Secretary.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

#region builderConfig
// Add Logging configuration. Load from appsettings.json.
builder.Host.UseSerilog((ctx, logger) =>
{
    logger.ReadFrom.Configuration(ctx.Configuration);
});
builder.Services.AddLogging();

// Throttling middleware.
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));

// Swagger config
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Secretary API",
        Version = "v1"
    });
});

// Configure background job.
builder.Services.AddHostedService<RemoveExpiredSecretsJob>();

// Secret service setup.
builder.Services.Configure<SecretOptions>(builder.Configuration.GetSection(nameof(SecretOptions)));
builder.Services.AddScoped<IGenericRepository<Secret>, GenericRepository<Secret>>();

builder.Services.AddHttpClient();

// Auth setup.
builder.Services.RegisterTokenHandlers(builder.Configuration);

// Encryption service config.
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<ISecretService, SecretService>();

// Configure database.
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configuring CORS policy to allow Angular client access the backend.
// Required when front-end leaves on the different hostname.
string corsPolicyName = "AllowAngularClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName,
        policy =>
        {
            string[] corsUris = builder.Configuration.GetSection("CorsUris").Get<string[]>();
            policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithOrigins(corsUris);
        });
});

// Configure AppInsights telemetry, pass connection string to the env
// more on https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core?tabs=netcore6
// if you don't provide connection string for appInsights then monitoring won't be enabled
builder.Services.AddApplicationInsightsTelemetry();

// MVC is required for AppInsights until MSFT fixes the bug
// more on https://github.com/Microsoft/ApplicationInsights-aspnetcore/issues/502
builder.Services.AddMvc();

#endregion

#region appConfig

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(corsPolicyName);

// Must be defined after app.UseCors()
app.UseIpRateLimiting();

app.UseHttpsRedirection();

// Configure fallback to the index.html to allow Angular handle the routing.
app.MapFallbackToFile(Path.Combine(builder.Environment.WebRootPath, "/index.html"));
app.UseStaticFiles(new StaticFileOptions
{
    // This is required to read config on the clients every one hours.
    // Without this setting the config will be cached forever, and in case of change in the configuration
    // the only option would be to do a force refresh on client browser or clear browser cache.
    OnPrepareResponse = ctx =>
    {
        if (string.Equals(ctx.File.Name, "config.prod.json", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.TryAdd("Cache-Control", "public,max-age=3600");
        }
    }
});

// Map endpoints
app.MapSecretEndpoints();
#endregion

app.Run();