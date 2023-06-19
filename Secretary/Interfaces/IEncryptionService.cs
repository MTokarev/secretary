using System;
namespace Secretary.Interfaces
{
    public interface IEncryptionService
    {
        Task<string> EncryptAsync(string key, string plainString);
        Task<string> DecryptAsync(string key, string encryptedString);
    }
}
