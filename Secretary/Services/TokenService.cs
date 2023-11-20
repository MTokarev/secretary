using Secretary.Enums;
using Secretary.Exceptions;
using Secretary.Interfaces;

namespace Secretary.Services;

/// <summary>
/// <see cref="ITokenService"/>
/// </summary>
public class TokenService : ITokenService
{
    private readonly IDictionary<AuthProviders, ITokenHandler> _validators;
    private readonly ILogger<TokenService> _logger;
    
    public TokenService(IEnumerable<ITokenHandler> validators, ILogger<TokenService> logger)
    {
        _validators = validators.ToDictionary(v => v.ProviderType);
        _logger = logger;
    }
    
    /// <summary>
    /// <see cref="ITokenService"/>
    /// </summary>
    public async ValueTask<bool> IsTokenValidAsync(AuthProviders provider, string token, CancellationToken cancellationToken)
    {
        var tokenHandler = GetITokenHandler(provider);
        return await tokenHandler.ValidateAsync(token, cancellationToken);
    }

    /// <summary>
    /// <see cref="ITokenHandler"/>
    /// </summary>
    public async ValueTask<string> GetUserEmailFromTokenAsync(AuthProviders provider, string token, CancellationToken cancellationToken)
    {
        var tokenHandler = GetITokenHandler(provider);
        var tokenResponse = await tokenHandler.GetTokenResponseAsync(token, cancellationToken);
        
        return tokenResponse.Email;
    }

    private ITokenHandler GetITokenHandler(AuthProviders provider)
    {
        if (_validators.TryGetValue(provider, out ITokenHandler tokenHandler))
        {
            _logger.LogDebug("Provider '{Provider}' is found in the '{ServiceName}' configuration. Starting Validation...",
                provider, 
                nameof(TokenService));

            return tokenHandler;
        }
        
        var message = $"Provider '{provider}' is not supported.";
        _logger.LogWarning(message);

        throw new AuthProviderNotSupportedException(message);
    }
}