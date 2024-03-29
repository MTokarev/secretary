﻿using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using Secretary.DTOs;
using Secretary.Enums;
using Secretary.Interfaces;
using Secretary.Models;
using Secretary.Options;

namespace Secretary.Services
{
  public class SecretService : ISecretService
  {
        private const int MaxPageSize = 20;
        
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

        public async Task<PaginatedResponse<SecretDto>> GetSecretsAsync(
            Expression<Func<Secret, bool>> expression, 
            int page,
            int pageSize = 10)
        {
            var totalItems = await _repo.GetCount(expression);
            
            var paginatedResponse = new PaginatedResponse<SecretDto>
            {
                Page = page == 0 
                    ? 1
                    : page,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            // TODO: We need to return a result where the client can get the reason of seeing an empty result
            // why empty result is returned.
            if (totalItems == 0 
                || paginatedResponse.Page > paginatedResponse.TotalPages)
            {
                paginatedResponse.Data = Enumerable.Empty<SecretDto>();
                return paginatedResponse;
            }
            
            // Use max page size if client has requested a bigger page
            paginatedResponse.PageSize = pageSize > MaxPageSize
                ? MaxPageSize
                : pageSize;
            
            var skipItems = page == 1 
                ? 0 
                : paginatedResponse.PageSize * (page - 1);
            var secrets = await _repo.GetAsync(expression, 
                (s) => s.CreatedOnUtc 
                , pageSize, 
                skipItems);
            paginatedResponse.Data = secrets.Select(SecretDto.CreateFromSecret);
            
            return paginatedResponse;
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

        public async Task<SecretExtendedDto> CreateSecretAsync(SecretExtendedDto secretExtendedDto)
        {
            secretExtendedDto.AccessAttemptsLeft = secretExtendedDto.AccessAttemptsLeft == 0 ?
                _secretOptions.DefaultAccessAttempts :
                secretExtendedDto.AccessAttemptsLeft;

            // If user not set the password, then generate a 'random' guid and use it to encrypt the body
            string key = secretExtendedDto.HasAccessPassword ? secretExtendedDto.AccessPassword : Guid.NewGuid().ToString();

            secretExtendedDto.Body = await _encryptionService.EncryptAsync(key, secretExtendedDto.Body);
            
            var secretToCreate = Secret.CreateFromDto(secretExtendedDto);
            
            if (secretExtendedDto is { SharedByEmail: not null, HasAccessPassword: false } )
            {
                secretToCreate.DecryptionKey = new DecryptionKey
                {
                    Id = Guid.NewGuid(),
                    Key = key,
                    SecretId = secretToCreate.Id,
                    Secret = secretToCreate
                };
            }
            
            var result = await _repo.CreateAsync(secretToCreate);
            int rowsAffected = await _repo.SaveChangesAsync();

            _logger.LogInformation($"New secret has been added to the database: '{rowsAffected}' row(s) affected.");

            var secretDtoToReturn = SecretExtendedDto.CreateFromSecret(result);
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
            // It should be separate background task which cleans up all expired secrets on scheduled bases.
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

            var secretToReturn = SecretExtendedDto.CreateFromSecret(secret);

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
            result.SecretExtendedDto = secretToReturn;

            return result;
        }

        public async Task<Secret> ProcessAccessedSecretAsync(Secret secret)
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

