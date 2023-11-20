using System.ComponentModel.DataAnnotations;

namespace Secretary.Models;

/// <summary>
/// Database model
/// This key is stored when the user was authenticated
/// </summary>
public class DecryptionKey
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Key { get; set; }
    
    public Guid SecretId { get; set; }
    
    [Required]
    public Secret Secret { get; set; }
}