using System.Text.Json.Serialization;

namespace Secretary.Models.Auth;

/// <summary>
/// Token response from auth provider
/// </summary>
public class TokenResponse
{
    [JsonPropertyName("email")]
    public string Email { get; set; }
}