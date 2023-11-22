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
    protected readonly ILogger<TokenHandlerBase> _logger;
    protected readonly AuthOptions _options;
    
    public abstract AuthProviders ProviderType { get; }

    protected TokenHandlerBase(IMemoryCache memoryCache, IOptions<AuthOptions> options, ILogger<TokenHandlerBase> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
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
}