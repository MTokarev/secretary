using System.Text.Json.Serialization;

namespace Secretary.Models.Auth;

/// <summary>
/// Facebook token response
/// <seealso cref="TokenResponse"/>
/// </summary>
public class FacebookTokenResponse : TokenResponse
{
    [JsonPropertyName("data")]
    public TokenData Data { get; set; }
}

/// <summary>
/// Nested class that carry the token data
/// </summary>
public class TokenData
{
    [JsonPropertyName("expires_at")]
    public long ExpiresAt { get; set; }
    
    [JsonPropertyName("is_valid")] 
    public bool IsValid { get; set; }
}