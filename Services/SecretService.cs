using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using Secretary.Data;
using Secretary.DTOs;
using Secretary.Enums;
using Secretary.Interfaces;
using Secretary.Models;
using Secretary.Options;

namespace Secretary.Services
{
    public class SecretService : ISecretService
    {
        private readonly IGenericRepository<Secret> _repo;
        private readonly IEncryptionService _encryptionService;
        private readonly SecretOptions _secretOptions;
        private readonly ILogger _logger;

        public SecretService(IGenericRepository<Secret> repo,
            IOptions<SecretOptions> secretOptions,
            IEncryptionService encryptionService,
            ILogger<SecretService> logger)
        {
            _repo = repo;
            _encryptionService = encryptionService;
            _secretOptions = secretOptions.Value;
            _logger = logger;
        }

        public async Task<IEnumerable<Secret>> GetSecretsAsync(Expression<Func<Secret, bool>> expression)
        {
            var result = await _repo.FindAsync(expression);
            return result;
        }

        public async Task<Secret> GetSecretAsync(Expression<Func<Secret, bool>> expression)
        {
            var result = await _repo.FindAsync(expression);

            return result.FirstOrDefault();
        }

        public async Task<SecretDto> CreateSecretAsync(SecretDto secretDto)
        {
            secretDto.AccessAttemptsLeft = secretDto.AccessAttemptsLeft == 0 ?
                _secretOptions.DefaultAccessAttempts :
                secretDto.AccessAttemptsLeft;

            // If user not set the password, then generate a 'random' guid and use it to encrypt the body
            string key = secretDto.HasAccessPassword ? secretDto.AccessPassword : Guid.NewGuid().ToString();

            secretDto.Body = await _encryptionService.EncryptAsync(key, secretDto.Body);

            var secretToCreate = Secret.CreateFromDto(secretDto);

            var result = await _repo.CreateAsync(secretToCreate);
            int rowsAffected = await _repo.SaveChangesAsync();

            _logger.LogInformation($"New secret has been added to the database: '{rowsAffected}' row(s) affected.");

            var secretDtoToReturn = SecretDto.CreateFromSecret(result);
            secretDtoToReturn.AccessPassword = key;
            
            return secretDtoToReturn;
        }

        public async Task<Secret> RemoveSecretAsync(Guid id)
        {
            var result = await _repo.FindAsync(r => r.RemovalKey.Id == id);
            var secretToDelete = result.FirstOrDefault();

            if (secretToDelete is null)
                return null;

            return await this.RemoveSecretAsync(secretToDelete);
        }

        public async Task<Secret> RemoveSecretAsync(Secret secretToDelete)
        {
            var trackedEntity = _repo.Remove(secretToDelete);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"Secret with id '{secretToDelete.Id}' has been removed.");

            return trackedEntity.Entity;
        }

        public async Task<Secret> UpdateSecretAsync(Secret secretToUpdate)
        {
            var result = _repo.Update(secretToUpdate);
            await _repo.SaveChangesAsync();

            return result;
        }

        public async Task<ResultSecret> ValidateSecretAsync(Secret secret, string? accessPassword = null)
        {
            var result = new ResultSecret();

            if (secret is null)
            {
                result.ValidationResult = SecretValidationResult.NotFound;
                result.Message = "Secret not found or not ready to be published.";

                return result;
            }

            // Handle expired secrets.
            // It should be separate background task which cleans up all expiried secrets on scheduled bases.
            if (DateTime.UtcNow > secret.AvailableUntilUtc)
            {
                string msg = $"Access attempt to expired secret with id '{secret.Id}' detected. " +
                    "Preparing the secret for deletion and returning null.";
                _logger.LogInformation(msg);

                result.ValidationResult = SecretValidationResult.Expired;
                result.Message = msg;

                await this.RemoveSecretAsync(secret);

                return result;
            }

            if (DateTime.UtcNow < secret.AvailableFromUtc)
            {
                string msg = "Attempt to access the key with id '{secret.Id}' that is not ready to be published. " +
                    $"Check '{nameof(secret.AvailableFromUtc)}' property.";
                _logger.LogInformation(msg);

                result.ValidationResult = SecretValidationResult.EarlyToShow;
                result.Message = "Secret not found or not ready to be published.";

                return result;
            }
            
            if (string.IsNullOrEmpty(accessPassword))
            {
                string msg = $"Password required for secret id '{secret.Id}'";
                _logger.LogInformation(msg);

                result.ValidationResult = SecretValidationResult.PasswordRequired;
                result.Message = msg;

                return result;
            }

            var secretToReturn = SecretDto.CreateFromSecret(secret);

            // If self removal is not allowed then remove removalKey from DTO
            if (!secret.SelfRemovalAllowed)
                secretToReturn.RemovalKey = new Guid();

            string decryptedBody = await _encryptionService.DecryptAsync(accessPassword, secretToReturn.Body);

            if (string.IsNullOrEmpty(decryptedBody))
            {
                string msg = $"Password provided for secret id '{secret.Id}' is incorrect.";
                _logger.LogInformation(msg);

                result.ValidationResult = SecretValidationResult.PasswordIncorrect;
                result.Message = msg;

                return result;
            }

            secretToReturn.Body = decryptedBody; 

            result.ValidationResult = SecretValidationResult.SuccessfullyValidated;
            result.Message = $"Secret with is '{secret.Id}' has been validated.";
            result.SecretDto = secretToReturn;

            return result;
        }

        public async Task<Secret> ProccessAccessedSecretAsync(Secret secret)
        {
            secret.AccessAttemptsLeft--;

            // If 1 access attempt left, then remove the secret from DB but
            // return it for the last time
            if (secret.AccessAttemptsLeft < 1)
            {
                _logger.LogInformation($"Last access attempt detected for secret with id '{secret.Id}'.");

                await this.RemoveSecretAsync(secret);
                return secret;
            }
            else
            {
                await this.UpdateSecretAsync(secret);
            }

            return secret;
        }
    }
}

