using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Secretary.Enums;
using Secretary.Exceptions;
using Secretary.Models.Auth;
using Secretary.Options;

namespace Secretary.Validators;

/// <summary>
/// Microsoft Token Handler
/// <seealso cref="TokenHandlerBase"/>
/// </summary>
public class MicrosoftTokenHandler : TokenHandlerBase
{
    public override AuthProviders ProviderType => AuthProviders.Microsoft;
    
    private const int DefaultTokenTtlInHours = 12;
    
    private readonly ILogger<FacebookTokenHandler> _logger;
    private readonly Provider _provider;
    private readonly HttpClient _httpClient;
    
    public MicrosoftTokenHandler(IHttpClientFactory httpClientFactory,
        IMemoryCache memoryCache,
        IOptions<AuthOptions> options,
        ILogger<FacebookTokenHandler> logger) : base(memoryCache, options)
    {
        ValidateOptions();
        
        _httpClient = httpClientFactory.CreateClient();
        _provider = _options.Providers[nameof(AuthProviders.Microsoft)];
        _httpClient.BaseAddress = new Uri(_provider.BaseUrl);
        _logger = logger;
    }

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

        var tokenResponse = await GetTokenResponseAsync(token, cancellationToken);

        _memoryCache.Set(token, tokenResponse, DateTimeOffset.Now.AddHours(DefaultTokenTtlInHours));

        return true;
    }

    /// <summary>
    /// <see cref="TokenHandlerBase"/>
    /// </summary>
    public override async ValueTask<TokenResponse> GetTokenResponseAsync(string token, CancellationToken cancellationToken = default)
    {
        // If token exists in the cache, then we have validated this token before
        if (_memoryCache.TryGetValue(token, out TokenResponse tokenResponse))
        {
            _logger.LogDebug("User token was found in the cache. Skipping validation.");
            return tokenResponse;
        }
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        var response = await _httpClient.GetAsync(_provider.UserProfileEndpoint, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var message = $"Unable to get user information from '{_httpClient.BaseAddress}' endpoint. Http status "
                          + $"'{response.StatusCode}' and message '{response.ReasonPhrase}'.";
            _logger.LogError(message);
            throw new UnableToGetUserInformationException(message);
        }
        
        var contentString = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogDebug("Sending user token to the Microsoft to get user information.");
        
        var microsoftTokenResponse = JsonSerializer.Deserialize<MicrosoftTokenResponse>(contentString);
        var constructedTokenResponse = new TokenResponse
        {
            Email = microsoftTokenResponse.Mail
        };

        _memoryCache.Set(token, constructedTokenResponse, DateTimeOffset.Now.AddHours(DefaultTokenTtlInHours));

        return constructedTokenResponse;
    }
}