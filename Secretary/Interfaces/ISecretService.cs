using System.Linq.Expressions;
using Secretary.DTOs;
using Secretary.Models;

namespace Secretary.Interfaces
{
    /// <summary>
    /// Represent interface for SecretService
    /// </summary>
    public interface ISecretService
    {
        /// <summary>
        /// Gets all secrets based on the expression
        /// </summary>
        /// <param name="expression">Predicate</param>
        /// <returns>Array of secrets <seealso cref="PaginatedResponse{T}"/></returns>
        Task<PaginatedResponse<SecretDto>> GetSecretsAsync(
            Expression<Func<Secret, bool>> expression,
            int page,
            int pageSize = 10);
        
        /// <summary>
        /// Gets all secrets based on the expression
        /// </summary>
        /// <param name="expression">Predicate</param>
        /// <returns>Array of secrets <seealso cref="PaginatedResponse{T}"/></returns>
        Task<IEnumerable<Secret>> GetSecretsAsync(Expression<Func<Secret, bool>> expression);
        
        /// <summary>
        /// Returns requested secret
        /// </summary>
        /// <param name="expression">Predicate to get a secret</param>
        /// <returns>Secret <seealso cref="Secret"/></returns>
        Task<Secret> GetSecretAsync(Expression<Func<Secret, bool>> expression);
        
        /// <summary>
        /// Validates secret
        /// </summary>
        /// <param name="secret">Secret to validate</param>
        /// <param name="accessPassword">Access password to decrypt secret body</param>
        /// <returns>Validation Result <seealso cref="ResultSecret"/></returns>
        Task<ResultSecret> ValidateSecretAsync(Secret secret, string? accessPassword = null);
        
        /// <summary>
        /// Processes a secret after it is been accessed. Based on the secret configuration
        /// it might change properties on the secret (like how many times it was accessed)
        /// ot delete the secret if the access attempt is equal to zero.
        /// </summary>
        /// <param name="secret">Secret to process</param>
        /// <returns>Processed secret <seealso cref="Secret"/></returns>
        Task<Secret> ProcessAccessedSecretAsync(Secret secret);
        
        /// <summary>
        /// Creates a secret
        /// </summary>
        /// <param name="secretExtendedDto">Secret to create DTO <seealso cref="SecretExtendedDto"/></param>
        /// <returns>Created secret <seealso cref="SecretExtendedDto"/></returns>
        Task<SecretExtendedDto> CreateSecretAsync(SecretExtendedDto secretExtendedDto);
        
        /// <summary>
        /// Removes the secret
        /// </summary>
        /// <param name="removalKeyId">Removal GUID </param>
        /// <returns>Returns removed secret <seealso cref="Secret"/></returns>
        Task<Secret> RemoveSecretAsync(Guid removalKeyId);
        
        /// <summary>
        /// Removes the secret
        /// </summary>
        /// <param name="secretToDelete">Secret to delete <seealso cref="Secret"/></param>
        /// <returns>Deleted secret <seealso cref="Secret"/></returns>
        Task<Secret> RemoveSecretAsync(Secret secretToDelete);
    }
}

