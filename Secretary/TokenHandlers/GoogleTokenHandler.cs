using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Secretary.Enums;
using Secretary.Exceptions;
using Secretary.Models.Auth;
using Secretary.Options;

namespace Secretary.Validators;

/// <summary>
/// Google token handler
/// <seealso cref="TokenHandlerBase"/>
/// </summary>
public class GoogleTokenHandler : TokenHandlerBase
{
    private readonly ILogger<GoogleTokenHandler> _logger;
    private readonly Provider _provider;
    private readonly HttpClient _httpClient;

    public GoogleTokenHandler(
        IMemoryCache memoryCache,
        ILogger<GoogleTokenHandler> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<AuthOptions> options) : base(memoryCache, options)
    {
        _provider = _options.Providers[nameof(AuthProviders.Google)];
        ValidateOptions((_provider) =>
        {
            if (string.IsNullOrEmpty(_provider.TokenEndpoint))
            {
                throw new MissingTokenValidatorConfigurationException($"Provider '{ProviderType}' exist in the configuration." +
                    $" However, '{nameof(_provider.TokenEndpoint)}' isn't set.");
            }
        });
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(_provider.BaseUrl);
        _logger = logger;
    }

    public override AuthProviders ProviderType => AuthProviders.Google;
    
    /// <summary>
    /// <see cref="TokenHandlerBase"/>
    /// </summary>
    public override async ValueTask<bool> ValidateAsync(string token, CancellationToken cancellationToken = default)
    {
        // If token exists in the cache, then we have validated this token before
        if (_memoryCache.TryGetValue(token, out TokenResponse _))
        {
            _logger.LogDebug("User token was found in the cache. Skipping validation.");
            return true;
        }

        GoogleTokenResponse googleTokenResponse;
        try
        {
            googleTokenResponse = await GetGoogleTokenResponseAsync(token, cancellationToken);
        }
        catch (UnableToGetUserInformationException)
        {
            return false;
        }

        // Put token in the cache and set the same expiration date as it was received in the response
        _logger.LogInformation("User token has been validated by Google}.");
        _logger.LogDebug("Adding valid token to the cache.");
        _memoryCache.Set(
            token, 
            googleTokenResponse, 
            DateTimeOffset.FromUnixTimeSeconds(long.Parse(googleTokenResponse.ExpireAt)));
        return true;

    }

    /// <summary>
    /// <see cref="TokenHandlerBase"/>
    /// </summary>
    public override async ValueTask<TokenResponse> GetTokenResponseAsync(string token, CancellationToken cancellationToken = default)
    {
        // If token exists in the cache, then we have validated this token before
        if (_memoryCache.TryGetValue(token, out TokenResponse cachedToken))
        {
            _logger.LogDebug("User token was found in the cache. Returning cached version.");
            return cachedToken;
        }

        return await GetGoogleTokenResponseAsync(token, cancellationToken);
    }

    private async ValueTask<GoogleTokenResponse> GetGoogleTokenResponseAsync(string token,
        CancellationToken cancellationToken)
    {
        var query = new QueryBuilder
        {
            { "id_token", token }
        };
        
        var response = await _httpClient.GetAsync( $"{_provider.TokenEndpoint}{query}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var message = $"Unable to get user information from '{_httpClient.BaseAddress}' endpoint. Http status "
                + $"'{response.StatusCode}' and message '{response.ReasonPhrase}'.";
            _logger.LogError(message);
            throw new UnableToGetUserInformationException(message);
        }
        
        var contentString = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<GoogleTokenResponse>(contentString);
    }
}