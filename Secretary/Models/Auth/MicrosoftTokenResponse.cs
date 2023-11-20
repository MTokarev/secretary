using System.Text.Json.Serialization;

namespace Secretary.Models.Auth;

/// <summary>
/// Microsoft token response
/// <seealso cref="TokenResponse"/>
/// </summary>
public class MicrosoftTokenResponse : TokenResponse
{
    [JsonPropertyName("mail")]
    public string Mail { get; set; }
}