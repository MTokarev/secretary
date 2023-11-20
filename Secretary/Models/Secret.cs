using System;
using System.ComponentModel.DataAnnotations;
using Secretary.DTOs;

namespace Secretary.Models
{
    public class Secret
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(4096)]
        public string Body { get; set; }
        
        public string? SharedByEmail { get; set; }

        public int AccessAttemptsLeft { get; set; }

        public bool SelfRemovalAllowed { get; set; }

        [Required]
        public DateTime AvailableFromUtc { get; set; }

        [Required]
        public DateTime AvailableUntilUtc { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public RemovalKey RemovalKey { get; set; }
        
        public DecryptionKey DecryptionKey { get; set; }

        public static Secret CreateFromDto(SecretExtendedDto secretExtendedDto)
        {
            var secret = new Secret
            {
                Body = secretExtendedDto.Body,
                Id = Guid.NewGuid(),
                CreatedOnUtc = DateTime.UtcNow,
                SelfRemovalAllowed = secretExtendedDto.SelfRemovalAllowed,
                AccessAttemptsLeft = secretExtendedDto.AccessAttemptsLeft,
                AvailableFromUtc = secretExtendedDto.AvailableFromUtc,
                AvailableUntilUtc = secretExtendedDto.AvailableUntilUtc,
                SharedByEmail = secretExtendedDto.SharedByEmail?.ToLower()
            };

            secret.RemovalKey = new RemovalKey
            {
                Id = Guid.NewGuid(),
                SecretId = secret.Id,
                Secret = secret
            };
            
            return secret;
        }

    }
}

