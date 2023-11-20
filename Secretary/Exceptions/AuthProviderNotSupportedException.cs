namespace Secretary.Exceptions;

/// <summary>
/// Throws when auth provider is not supported
/// </summary>
public class AuthProviderNotSupportedException : Exception
{
    public AuthProviderNotSupportedException(string message) : base(message)
    {
        
    }
}