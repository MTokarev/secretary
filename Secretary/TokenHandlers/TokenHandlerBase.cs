using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Secretary.Enums;
using Secretary.Exceptions;
using Secretary.Interfaces;
using Secretary.Models.Auth;
using Secretary.Options;

namespace Secretary.Validators;

/// <summary>
/// <see cref="ITokenHandler"/>
/// </summary>
public abstract class TokenHandlerBase : ITokenHandler
{
    protected readonly IMemoryCache _memoryCache;
    protected readonly AuthOptions _options;
    
    public abstract AuthProviders ProviderType { get; }

    protected TokenHandlerBase(IMemoryCache memoryCache, IOptions<AuthOptions> options)
    {
        _memoryCache = memoryCache;
        _options = options.Value;
    }
    
    /// <summary>
    /// <see cref="ITokenHandler"/>
    /// </summary>
    public abstract ValueTask<bool> ValidateAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// <see cref="ITokenHandler"/>
    /// </summary>
    public abstract ValueTask<TokenResponse> GetTokenResponseAsync(string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if options are present for auth provide
    /// </summary>
    /// <exception cref="MissingTokenValidatorConfigurationException"></exception>
    protected virtual void ValidateOptions(Action<Provider> additionalCheck = default)
    {
        // 'Enum.GetName()' is used to avoid boxing\unboxing
        if (!_options.Providers.TryGetValue(Enum.GetName(typeof(AuthProviders), ProviderType), out var provider))
        {
            throw new NotImplementedException($"Unable to locate '{nameof(ProviderType)}' in configuration." 
                + " Please make sure that configuration exist");
        }

        if (string.IsNullOrEmpty(provider.ClientId)
            || string.IsNullOrEmpty(provider.BaseUrl))
        {
            throw new MissingTokenValidatorConfigurationException($"Provider '{ProviderType}' exist in the configuration, "
              + $"but required parameters are missing. Make sure {nameof(provider.BaseUrl)}' is set.");
        }

        if (additionalCheck != null)
        {
            additionalCheck.Invoke(provider);
        }
    }
}