namespace Secretary.Options;

/// <summary>
/// Provider Option
/// </summary>
public class Provider
{
    /// <summary>
    /// Base provide Url
    /// </summary>
    public string BaseUrl { get; set; }
    
    /// <summary>
    /// Token endpoint
    /// </summary>
    public string TokenEndpoint { get; set; }
    
    /// <summary>
    /// User profile URL
    /// </summary>
    public string UserProfileEndpoint { get; set; }
    
    /// <summary>
    /// Client Id
    /// </summary>
    public string ClientId { get; set; }
    
    /// <summary>
    /// Client Secret
    /// </summary>
    public string ClientSecret { get; set; }
}