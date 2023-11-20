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
/// <see cref="TokenHandlerBase"/>
/// </summary>
public class FacebookTokenHandler : TokenHandlerBase
{
    public override AuthProviders ProviderType => AuthProviders.Facebook;
    
    private readonly ILogger<FacebookTokenHandler> _logger;
    private readonly Provider _provider;
    private readonly HttpClient _httpClient;

    public FacebookTokenHandler(
        IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IOptions<AuthOptions> options,
        ILogger<FacebookTokenHandler> logger) : base(memoryCache, options)
    {
        base.ValidateOptions(provider =>
        {
            if (string.IsNullOrEmpty(provider.UserProfileEndpoint)
                || string.IsNullOrEmpty(provider.TokenEndpoint)
                || string.IsNullOrEmpty(provider.ClientId)
                || string.IsNullOrEmpty(provider.ClientSecret))
            {
                throw new MissingTokenValidatorConfigurationException($"Provider '{ProviderType}' exist in the configuration, "
                  + $"but the properties '{nameof(provider.UserProfileEndpoint)}, {nameof(provider.TokenEndpoint)}, " +
                  $"{nameof(provider.ClientId)}, {nameof(provider.ClientSecret)}' aren't set.");
            }
        });
        
        _httpClient = httpClientFactory.CreateClient();
        _provider = _options.Providers[nameof(AuthProviders.Facebook)];
        _httpClient.BaseAddress = new Uri(_provider.BaseUrl);
        _logger = logger;

    }
    
    /// <summary>
    /// <see cref="TokenHandlerBase"/>
    /// </summary>
    public override async ValueTask<bool> ValidateAsync(string token, CancellationToken cancellationToken = default)
    {
        // If token exists in the cache, then we have validated this token before
        if (_memoryCache.TryGetValue(token, out FacebookTokenResponse _))
        {
            _logger.LogDebug("User token was found in the cache. Skipping validation.");
            return true;
        }

        var facebookTokenResponse = await GetFacebookTokenResponseAsync(token, cancellationToken);
        if (!facebookTokenResponse.Data.IsValid)
        {
            return false;
        }

        facebookTokenResponse.Email = await GetUserEmailAddressAsync(token, cancellationToken);
        
        // Put token in the cache and set the same expiration date as it was received in the response
        _logger.LogInformation("User token has been validated by Facebook.");
        _logger.LogDebug("Getting user's email adder...");
        
        _memoryCache.Set(
            token, 
            facebookTokenResponse, 
            DateTimeOffset.FromUnixTimeSeconds(facebookTokenResponse.Data.ExpiresAt));
        
        return true;
    }

    public override async ValueTask<TokenResponse> GetTokenResponseAsync(string token, CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(token, out FacebookTokenResponse tokenResponse))
        {
            _logger.LogDebug("User token was found in the cache. Returning cached response.");
            return tokenResponse;
        }
        
        var facebookTokenResponse = await GetFacebookTokenResponseAsync(token, cancellationToken);
        facebookTokenResponse.Email = await GetUserEmailAddressAsync(token, cancellationToken);
        
        return facebookTokenResponse;
    }
    private async ValueTask<string> GetUserEmailAddressAsync(string token, CancellationToken cancellationToken)
    {
        var query = new QueryBuilder
        {
            { "fields", "email" }
        };
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var response = await _httpClient.GetAsync($"{_provider.UserProfileEndpoint}{query}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var message = $"Unable to get user information from '{_httpClient.BaseAddress}' endpoint. Http status "
                + $"'{response.StatusCode}' and message '{response.ReasonPhrase}'.";
            _logger.LogError(message);
            throw new UnableToGetUserInformationException(message);
        }
        
        var contentString = await response.Content.ReadAsStringAsync(cancellationToken);
        var content = JsonSerializer.Deserialize<FacebookUserProfileResponse>(contentString);

        return content.Email;
    }


    private async ValueTask<FacebookTokenResponse> GetFacebookTokenResponseAsync(string token,
        CancellationToken cancellationToken)
    {
        var query = new QueryBuilder
        {
            { "input_token", token },
            { "access_token", $"{_provider.ClientId}|{_provider.ClientSecret}" }
        };
        
        var response = await _httpClient.GetAsync($"{_provider.TokenEndpoint}{query}", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        
        var contentString = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("Sending user token to the Facebook to verify the token.");
        
        return JsonSerializer.Deserialize<FacebookTokenResponse>(contentString);
    }
}