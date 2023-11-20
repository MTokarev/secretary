using Secretary.Enums;

namespace Secretary.Interfaces;

/// <summary>
/// Interface for the token service
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Validates token based on requested provider <seealso cref="AuthProviders"/>
    /// </summary>
    /// <param name="provider">Provide <seealso cref="AuthProviders"/></param>
    /// <param name="token">Token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if token is valid. Otherwise, false.</returns>
    ValueTask<bool> IsTokenValidAsync(AuthProviders provider, string token, CancellationToken cancellationToken);

    /// <summary>
    /// Extracts user's email from the token<seealso cref="AuthProviders"/>
    /// </summary>
    /// <param name="provider">Provide <seealso cref="AuthProviders"/></param>
    /// <param name="token">Token to extract from</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User's email address</returns>
    ValueTask<string> GetUserEmailFromTokenAsync(AuthProviders provider, string token,
        CancellationToken cancellationToken);
}