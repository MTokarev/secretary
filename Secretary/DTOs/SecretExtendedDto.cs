using Secretary.Models;
using System.ComponentModel.DataAnnotations;

namespace Secretary.DTOs
{
    /// <summary>
    /// Represent Data Transfer Object for secret 
    /// </summary>
    public class SecretExtendedDto : SecretDto
    {
        
        /// <summary>
        /// Optional field
        /// Will be set only if user made a login before creating a secret
        /// </summary>
        public string? SharedByEmail { get; set; }
        
        [Required]
        [StringLength(4096)]
        public string Body { get; set; }

        public bool HasAccessPassword => !string.IsNullOrEmpty(AccessPassword);

        /// <summary>
        /// Optional property, if not set then API will generate a random password for you.
        /// If set then the password will be used to encrypt secret. Service doesn't store access password anywhere.
        /// </summary>
        public string? AccessPassword { get; set; }
        
        public static SecretExtendedDto CreateFromSecret(Secret secret)
        {
            return new SecretExtendedDto {
                Id = secret.Id,
                SelfRemovalAllowed = secret.SelfRemovalAllowed,
                RemovalKey = secret.RemovalKey.Id,
                AccessAttemptsLeft = secret.AccessAttemptsLeft,
                Body = secret.Body,
                AvailableFromUtc = secret.AvailableFromUtc,
                AvailableUntilUtc = secret.AvailableUntilUtc,
                SharedByEmail = secret.SharedByEmail
            };
        }
    }
}

