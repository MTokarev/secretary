using System.Reflection;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Secretary.Data;
using Secretary.DTOs;
using Secretary.Enums;
using Secretary.Extensions;
using Secretary.Interfaces;
using Secretary.Models;
using Secretary.Options;
using Secretary.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

#region builderConfig
// Add Logging congif. Load from appdettings.json.
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
builder.Services.AddHostedService<RemoveExpiriedSecretsJob>();

// Secret service setup.
builder.Services.Configure<SecretOptions>(builder.Configuration.GetSection(nameof(SecretOptions)));
builder.Services.AddScoped<IGenericRepository<Secret>, GenericRepository<Secret>>();

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
// if you don't provide connection string for appInsights then monitoring won't be enalbled
builder.Services.AddApplicationInsightsTelemetry();

// MVC is required for AppInsigths until MSFT fixes the bug
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
#endregion

#region endpoints

app.MapGet("/secrets/{id:guid}", async ([FromRoute]Guid id, [FromHeader]string? accessPassword, ISecretService secretService) =>
{
    var secretFromDb = await secretService.GetSecretAsync(s => s.Id == id);

    var validatedResult = await secretService.ValidateSecretAsync(secretFromDb, accessPassword);

    if (validatedResult.ValidationResult == SecretValidationResult.SuccessfullyValidated)
        await secretService.ProccessAccessedSecretAsync(secretFromDb);

    return validatedResult.GetResult();
    
}).Produces<SecretDto>(StatusCodes.Status200OK)
  .Produces(StatusCodes.Status404NotFound)
  .WithName("GetSecret");

app.MapPost("/secrets", async ([FromBody] SecretDto secretDto, ISecretService secretService) =>
{
    if (secretDto.AvailableFromUtc > secretDto.AvailableUntilUtc)
        return Results.BadRequest("Expiration date must be greather than available from. " +
            $"Start date: '{secretDto.AvailableFromUtc}'. End date: '{secretDto.AvailableUntilUtc}'.");

    var result = await secretService.CreateSecretAsync(secretDto);

    return Results.CreatedAtRoute("GetSecret", new { id = result.Id }, result);
}).Produces<SecretDto>(StatusCodes.Status201Created);

app.MapDelete("/secrets/{removalKeyId:guid}", async ([FromRoute] Guid removalKeyId,  ISecretService secretService) =>
{
    var secretToDelete = await secretService.RemoveSecretAsync(removalKeyId);

    if (secretToDelete is null)
        return Results.NotFound($"Secret with removal key id '{removalKeyId}' not found.");

    return Results.NoContent();
}).Produces(StatusCodes.Status204NoContent);

#endregion

app.Run();