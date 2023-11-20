using Secretary.Enums;
using Secretary.Models.Auth;

namespace Secretary.Interfaces;

/// <summary>
/// Interface for token validator
/// </summary>
public interface ITokenHandler
{
    /// <summary>
    /// Auth Provider for this validator <seealso cref="AuthProviders"/>
    /// </summary>
    AuthProviders ProviderType { get; }
    
    /// <summary>
    /// Validates token
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token is valid. Otherwise, false.</returns>
    ValueTask<bool> ValidateAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns token response
    /// </summary>
    /// <param name="token">Token to extract from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see cref="TokenResponse"/></returns>
    ValueTask<TokenResponse> GetTokenResponseAsync(string token, CancellationToken cancellationToken = default);
}