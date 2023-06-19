using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Secretary.DTOs;
using Secretary.Models;

namespace Secretary.Interfaces
{
    public interface ISecretService
    {
        Task<IEnumerable<Secret>> GetSecretsAsync(Expression<Func<Secret, bool>> expression);
        Task<Secret> GetSecretAsync(Expression<Func<Secret, bool>> expression);
        Task<ResultSecret> ValidateSecretAsync(Secret secret, string? accessPassword = null);
        Task<Secret> ProcessAccessedSecretAsync(Secret secret);
        Task<SecretDto> CreateSecretAsync(SecretDto secretDto);
        Task<Secret> RemoveSecretAsync(Guid removalKeyId);
        Task<Secret> RemoveSecretAsync(Secret secretToDelete);
    }
}

