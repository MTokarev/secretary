using System.Text.Json.Serialization;

namespace Secretary.Models.Auth;

/// <summary>
/// Token response from Google
/// </summary>
public class GoogleTokenResponse : TokenResponse
{
    [JsonPropertyName("aud")]
    public string Audience { get; set; }
    
    [JsonPropertyName("exp")]
    public string ExpireAt { get; set; }
}