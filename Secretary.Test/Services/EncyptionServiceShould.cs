using System;
using Microsoft.Extensions.Logging.Abstractions;
using Secretary.Interfaces;
using Secretary.Services;

namespace Secretary.Test.Services
{
    public class EncyptionServiceShould
    {
        private readonly IEncryptionService _encyptionService;

        public EncyptionServiceShould()
        {
            _encyptionService = new EncryptionService(NullLogger<EncryptionService>.Instance);
        }

        [Fact]
        public async Task Return_Expected_Encrypted_Base64_String()
        {
            // arrange
            string dataToEncrypt = "MySecret1";
            string key = "UseThisSecretToEncrypt";

            string expectedString = "7Htbe+YRqsolkhZnCzX3mQ==";

            // act
            string base64Result = await _encyptionService.EncryptAsync(key, dataToEncrypt);

            // assert
            Assert.Equal(expectedString, base64Result);

        }

        [Fact]
        public async Task Return_Expected_Decrypted_String()
        {
            // arrange
            string encryptedString = "7Htbe+YRqsolkhZnCzX3mQ==";
            string key = "UseThisSecretToEncrypt";

            string expectedString = "MySecret1";

            // act
            string result = await _encyptionService.DecryptAsync(key, encryptedString);

            // assert
            Assert.Equal(expectedString, result);

        }

        [Fact]
        public async Task Return_Null_If_Key_Incorrect()
        {
            // arrange
            string encryptedString = "7Htbe+YRqsolkhZnCzX3mQ==";
            string incorrectKey = "WrongKey";

            // act
            string result = await _encyptionService.DecryptAsync(incorrectKey, encryptedString);

            // assert
            Assert.Null(result);

        }
    }
}

