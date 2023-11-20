using Secretary.Enums;

namespace Secretary.DTOs;

/// <summary>
/// Auth DTO that the client sends
/// </summary>
public class AuthDto
{
    /// <summary>
    /// Auth provider for particular request
    /// </summary>
    public AuthProviders Provider { get; set; }
    
    /// <summary>
    /// Auth token
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// ASP.Net cannot use objects in the headers
    /// Unless user provides a method to construct the model from the header string
    /// </summary>
    public static bool TryParse(
        string? value, 
        IFormatProvider? provider,
        out AuthDto? auth)
    {
        // Value here is a coma separated string of parameter name and value
        // We need to extract only value
        var segments = value?
            .Split(',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where((element, index) => index % 2 == 1)
            .ToList();

        if (segments?.Count == 2 &&
            Enum.TryParse(segments[0], ignoreCase: true, out AuthProviders autProvider))
        {
            auth = new AuthDto
            {
                Provider = autProvider,
                Token = segments[1]
            };

            return true;
        }
        
        auth = null;
        return false;
    }
}