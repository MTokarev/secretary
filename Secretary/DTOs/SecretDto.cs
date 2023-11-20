using System.ComponentModel.DataAnnotations;
using Secretary.Models;

namespace Secretary.DTOs;

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
        /// If self-removal allowed then DTO will return back RemovalKey to the user
        /// </summary>
        public bool SelfRemovalAllowed { get; set; }
        
        /// <summary>
        /// How many times secret remains accessible. Will be removed when attempts = 0.
        /// </summary>
        public int AccessAttemptsLeft { get; set; }

        /// <summary>
        /// Decryption key exists on the secrets where creator is authenticated
        /// and thd custom access key is not set
        /// </summary>
        public string DecryptionKey { get; set; }

        /// <summary>
        /// Time in UTC when the secret becomes available.
        /// </summary>
        [Required]
        public DateTime AvailableFromUtc { get; set; }
        
        /// <summary>
        /// Time in UTC when the secret was created.
        /// </summary>
        [Required]
        public DateTime CreatedOnUtc { get; set; }

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
                CreatedOnUtc = secret.CreatedOnUtc,
                AccessAttemptsLeft = secret.AccessAttemptsLeft,
                AvailableFromUtc = secret.AvailableFromUtc,
                AvailableUntilUtc = secret.AvailableUntilUtc,
                DecryptionKey = secret.DecryptionKey?.Key
            };
        }
}