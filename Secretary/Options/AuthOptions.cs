namespace Secretary.Options;

/// <summary>
/// Auth options
/// </summary>
public class AuthOptions
{
    /// <summary>
    /// A dictionary with the auth provider name as a key and the provider as a value
    /// </summary>
    public IDictionary<string, Provider> Providers { get; set; }
}