using System;
using System.Security.Cryptography;
using System.Text;
using Secretary.Interfaces;

namespace Secretary.Services
{
    public class EncryptionService : IEncryptionService
    {
        private readonly ILogger<EncryptionService> _logger;

        public EncryptionService(ILogger<EncryptionService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Decrypt base64 string
        /// </summary>
        /// <param name="key">Decription key</param>
        /// <param name="encryptedBase64String">Base 64 string to decrypt</param>
        /// <returns>Descrypted string</returns>
        public async Task<string> DecryptAsync(string key, string encryptedBase64String)
        {
            if (String.IsNullOrEmpty(key) || String.IsNullOrEmpty(encryptedBase64String))
            {
                string msg = $"'{nameof(key)}' and '{nameof(encryptedBase64String)}' must not be null or empty.";
                _logger.LogError(msg);
                throw new ArgumentNullException(msg);
            }

            using var aes = Aes.Create();

            // AES key requires to be specific length, to achieve that we use hash function
            using var sha256 = SHA256.Create();
            byte[] keyHashed = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));

            aes.Key = keyHashed;
            aes.IV = new byte[16];

            using var ms = new MemoryStream(Convert.FromBase64String(encryptedBase64String));

            using var cryptoStream = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            await ms.FlushAsync();

            string? result = null;
            try
            {
                result = await streamReader.ReadToEndAsync();

            }
            catch (CryptographicException ex)
            {
                _logger.LogWarning("Exception has been thrown during decryption operation." + "" +
                    $"Provided decryption key might not be valid. Message '{ex.Message}'. Exception '{ex}'.");
            }

            return result;
        }

        /// <summary>
        /// Symmetric stryng encryption
        /// </summary>
        /// <param name="key">Key to encrypt string</param>
        /// <param name="plainString">String to encrypt</param>
        /// <returns>Base 64 formatted encrypted string</returns>
        public async Task<string> EncryptAsync(string? key, string plainString)
        {

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(plainString))
            {
                string msg = $"'{nameof(key)}' and '{nameof(plainString)}' must not be null or empty.";
                _logger.LogError(msg);
                throw new ArgumentNullException(msg);
            }

            using var aes = Aes.Create();

            // AES key requires to be specific length, to achieve that we use hash function
            using var sha256 = SHA256.Create();
            byte[] keyHashed = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));

            aes.Key = keyHashed;

            // Set initialization vector
            aes.IV = new byte[16];

            using var ms = new MemoryStream();

            using var cryptoStream = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            using var encryptWriter = new StreamWriter(cryptoStream);

            await encryptWriter.WriteAsync(plainString);

            await encryptWriter.FlushAsync();
            await cryptoStream.FlushFinalBlockAsync();

            return Convert.ToBase64String(ms.ToArray());
        }
    }
}

