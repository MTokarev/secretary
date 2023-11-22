using Secretary.Enums;
using Secretary.Interfaces;
using Secretary.Options;
using Secretary.Services;
using Secretary.Validators;

namespace Secretary.Extensions;

/// <summary>
/// Extension class to register services
/// </summary>
public static class RegisterServices
{
    /// <summary>
    /// Register Auth Token Handlers
    /// </summary>
    /// <param name="services"></param>
    public static void RegisterTokenHandlers(this IServiceCollection services, IConfiguration config)
    {

        var authOptions = new AuthOptions();
        var authSection = config.GetSection(nameof(AuthOptions));
        var logger = services.BuildServiceProvider().GetService<ILogger<Program>>();
        
        services.Configure<AuthOptions>(authSection);
        authSection.Bind(authOptions);

        if (authOptions.Providers.TryGetValue(nameof(AuthProviders.Facebook), out var facebookProvider))
        {
            if (string.IsNullOrEmpty(facebookProvider.BaseUrl)
                || string.IsNullOrEmpty(facebookProvider.TokenEndpoint)
                || string.IsNullOrEmpty(facebookProvider.UserProfileEndpoint)
                || string.IsNullOrEmpty(facebookProvider.ClientId)
                || string.IsNullOrEmpty(facebookProvider.ClientSecret))
            {
                logger.LogWarning("Unable to add auth provider {ProviderName}. Make sure you have provided: "
                    + "'{BaseUrl}, {TokenEndpoint}, {UserProfileEndpoint}, {ClientId}, {ClientSecret}'.",
                    nameof(AuthProviders.Facebook),
                    nameof(facebookProvider.BaseUrl),
                    nameof(facebookProvider.TokenEndpoint),
                    nameof(facebookProvider.UserProfileEndpoint),
                    nameof(facebookProvider.ClientId),
                    nameof(facebookProvider.ClientSecret));
            }
            else
            {
                services.AddScoped<ITokenHandler, FacebookTokenHandler>();
            }
        }

        if (authOptions.Providers.TryGetValue(nameof(AuthProviders.Google), out var googleProvider))
        {
            if (string.IsNullOrEmpty(googleProvider.BaseUrl)
                || string.IsNullOrEmpty(googleProvider.TokenEndpoint))
            {
                logger.LogWarning("Unable to add auth provider {ProviderName}. Make sure you have provided: "
                    + "'{BaseUrl}, '{TokenEndpoint}'.",
                    nameof(googleProvider.BaseUrl),
                    nameof(googleProvider.TokenEndpoint));
            }
            else
            {
                services.AddScoped<ITokenHandler, GoogleTokenHandler>();
            }
        }
        
        if (authOptions.Providers.TryGetValue(nameof(AuthProviders.Microsoft), out var microsoftProvider))
        {
            if (string.IsNullOrEmpty(microsoftProvider.BaseUrl)
                || string.IsNullOrEmpty(microsoftProvider.UserProfileEndpoint))
            {
                logger.LogWarning("Unable to add auth provider {ProviderName}. Make sure you have provided: "
                    + "'{BaseUrl}, '{TokenEndpoint}'.",
                    nameof(microsoftProvider.BaseUrl),
                    nameof(microsoftProvider.UserProfileEndpoint));
            }
            else
            {
                services.AddScoped<ITokenHandler, MicrosoftTokenHandler>();
            }
        }
        
        services.AddScoped<ITokenService, TokenService>();
    }
}