using System.Text.Json.Serialization;

namespace Secretary.Models.Auth;

/// <summary>
/// Facebook response from the user profile endpoint
/// </summary>
public class FacebookUserProfileResponse
{
    [JsonPropertyName("email")]
    public string Email { get; set; }
}