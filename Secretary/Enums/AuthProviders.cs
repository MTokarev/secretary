using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Secretary.Enums;

/// <summary>
/// Known Auth Providers
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AuthProviders
{
    /// <summary>
    /// Facebook
    /// </summary>
    [EnumMember(Value = nameof(AuthProviders.Facebook))]
    Facebook,
    
    /// <summary>
    /// Google
    /// </summary>
    [EnumMember(Value = nameof(AuthProviders.Google))]
    Google,
    
    /// <summary>
    /// Microsoft
    /// </summary>
    [EnumMember(Value = nameof(AuthProviders.Microsoft))]
    Microsoft
}