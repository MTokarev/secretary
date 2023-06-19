using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Secretary.Data;
using Secretary.DTOs;
using Secretary.Enums;
using Secretary.Interfaces;
using Secretary.Models;
using Secretary.Options;
using Secretary.Services;

namespace Secretary.Test.Services
{
    public class SecretServiceShould
    {
        private readonly Mock<IGenericRepository<Secret>> _repo;
        private readonly IOptions<SecretOptions> _secretOptions;
        private readonly SecretService _secretService;
        private readonly Mock<IEncryptionService> _encryptionService;
        IEnumerable<Secret> _secrets;
        IEnumerable<SecretDto> _secretsDto;

        public SecretServiceShould()
        {
            _secretOptions = Microsoft.Extensions.Options.Options.Create(new SecretOptions());
            _repo = new Mock<IGenericRepository<Secret>>();
            _encryptionService = new Mock<IEncryptionService>();
            this.Init_Data();
            _secretService = new SecretService(_repo.Object, _secretOptions, _encryptionService.Object, NullLogger<SecretService>.Instance);
        }

        private void Init_Data()
        {

            var secret1 = new Secret
            {
                Id = Guid.NewGuid(),
                AccessAttemptsLeft = 3,
                Body = "Ahhh, you want to get my first secret?",
                AvailableFromUtc = DateTime.UtcNow,
                AvailableUntilUtc = DateTime.UtcNow.AddMinutes(10),
                CreatedOnUtc = DateTime.UtcNow
            };
            secret1.RemovalKey = new RemovalKey
            {
                Id = Guid.NewGuid(),
                Secret = secret1,
                SecretId = secret1.Id
            };

            var secret2 = new Secret
            {
                Id = Guid.NewGuid(),
                AccessAttemptsLeft = 3,
                Body = "Ahhh, you want to get my second secret?",
                AvailableFromUtc = DateTime.UtcNow,
                AvailableUntilUtc = DateTime.UtcNow.AddMinutes(-10),
                CreatedOnUtc = DateTime.UtcNow
            };
            secret2.RemovalKey = new RemovalKey
            {
                Id = Guid.NewGuid(),
                Secret = secret2,
                SecretId = secret2.Id
            };

            _secrets = new List<Secret>
            {
                secret1,
                secret2
            }.AsQueryable();

            _secretsDto = _secrets.Select(s => SecretDto.CreateFromSecret(s))
                .ToList();

            _repo.Setup(x => x.FindAsync(x => x.Id == _secrets.First().Id))
                .ReturnsAsync(new List<Secret> { _secrets.First() });
            _repo.Setup(x => x.CreateAsync(It.IsAny<Secret>()))
                .ReturnsAsync((Secret s) => s);
            _repo.Setup(x => x.Update(It.IsAny<Secret>()))
                .Returns((Secret s) => s);
            _repo.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

        }

        [Fact]
        public async Task Return_DesiredSecret()
        {
            // arrange
            // set up in Init_Data

            // act
            var result = await _secretService.GetSecretAsync(x => x.Id == _secrets.First().Id);

            // assert
            Assert.Equal(_secrets.First().Id, result.Id);
            Assert.NotEqual(new Guid(), result.Id);
        }

        [Fact]
        public async Task Return_Null_If_Secret_DoesNot_Exists()
        {
            // arrange
            var guid = new Guid(); // Guid with all zeroes

            // act
            var result = await _secretService.GetSecretAsync(x => x.Id == guid);

            // assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Create_Secret()
        {
            // arrange
            var secretDto = _secretsDto.First();

            // act
            var result = await _secretService.CreateSecretAsync(secretDto);

            // assert
            Assert.Equal(secretDto.Body, result.Body);
            Assert.NotEqual(secretDto.Id, result.Id);
        }

        [Fact]
        public async Task Return_Nothing_If_Secret_Has_Password_But_User_Dont_Provide_IT()
        {
            // arrange
            var secret = _secrets.First();

            // act
            var result = await _secretService.ValidateSecretAsync(secret);

            // assert
            Assert.Equal(result.ValidationResult, SecretValidationResult.PasswordRequired);
            Assert.Null(result.SecretDto);
        }

        [Fact]
        public async Task Return_Nothing_If_Secret_Has_Password_But_User_Provide_Incorrect()
        {
            // arrange
            var secret = _secrets.First();

            // act
            var result = await _secretService.ValidateSecretAsync(secret, "wrongPassword");

            // assert
            Assert.Equal(result.ValidationResult, SecretValidationResult.PasswordIncorrect);
            Assert.Null(result.SecretDto);
        }


        [Fact]
        public async Task Substract_AccessCount_Each_Time_Secret_Gets_Accessed()
        {
            // arrange
            var secret = _secrets.First();
            int initiallyAccessAtemptLeft = secret.AccessAttemptsLeft;

            // act
            var result = await _secretService.ProcessAccessedSecretAsync(secret);

            // assert
            Assert.True(initiallyAccessAtemptLeft > result.AccessAttemptsLeft);
        }
    }
}

