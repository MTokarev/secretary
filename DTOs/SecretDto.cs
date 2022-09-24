using System;
using Secretary.Models;
using System.ComponentModel.DataAnnotations;

namespace Secretary.DTOs
{
    /// <summary>
    /// Represent Data Transfer Object for secret 
    /// </summary>
    public class SecretDto
    {
        /// <summary>
        /// Will be ignored on creation
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Will be ignored on creation
        /// </summary>
        public Guid RemovalKey { get; set; }

        /// <summary>
        /// If selfremoval allowed then DTO will return back RemovalKey to the user
        /// </summary>
        public bool SelfRemovalAllowed { get; set; }

        [Required]
        [StringLength(2048)]
        public string Body { get; set; }

        public bool HasAccessPassword => !string.IsNullOrEmpty(AccessPassword);

        /// <summary>
        /// Optional propetry, if not set then API will generate a random password for you.
        /// If set then the password will be used to encrypt secret. Service doesn't store access password anywhere.
        /// </summary>
        public string? AccessPassword { get; set; }

        /// <summary>
        /// How many times secret remains accessible. Will be removed when attempts = 0.
        /// </summary>
        public int AccessAttemptsLeft { get; set; }

        /// <summary>
        /// Time in UTC when the secret becomes available, until that time the API returns HTTP404.
        /// </summary>
        [Required]
        public DateTime AvailableFromUtc { get; set; }


        /// <summary>
        /// Specify available 'until date' in UTC. Once this date pass the secret will be removed.
        /// </summary>
        [Required]
        public DateTime AvailableUntilUtc { get; set; }

        public static SecretDto CreateFromSecret(Secret secret)
        {
            return new SecretDto {
                Id = secret.Id,
                SelfRemovalAllowed = secret.SelfRemovalAllowed,
                RemovalKey = secret.RemovalKey.Id,
                AccessAttemptsLeft = secret.AccessAttemptsLeft,
                Body = secret.Body,
                AvailableFromUtc = secret.AvailableFromUtc,
                AvailableUntilUtc = secret.AvailableUntilUtc
            };
        }
    }
}

