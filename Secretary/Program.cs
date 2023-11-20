using System.Reflection;
using AspNetCoreRateLimit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Secretary.ApiEndpoints;
using Secretary.Data;
using Secretary.Interfaces;
using Secretary.Models;
using Secretary.Options;
using Secretary.Services;
using Secretary.Validators;
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

// Validators setup.
// Memory cache is required to work properly, make sure you register it as a DI.
builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(nameof(AuthOptions)));
builder.Services.AddScoped<ITokenHandler, FacebookTokenHandler>();
builder.Services.AddScoped<ITokenHandler, GoogleTokenHandler>();
builder.Services.AddScoped<ITokenHandler, MicrosoftTokenHandler>();
builder.Services.AddScoped<ITokenService, TokenService>();

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
app.MapFallbackToFile(Path.Combine(builder.Environment.ContentRootPath, "release", "wwwroot", "/index.html"));
app.UseStaticFiles();

// Map endpoints
app.MapSecretEndpoints();
#endregion

app.Run();