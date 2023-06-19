using System;
using System.ComponentModel.DataAnnotations;

namespace Secretary.Models;

public class RemovalKey
{
    [Key]
    public Guid Id { get; set; }

    public Guid SecretId { get; set; }

    [Required]
    public Secret Secret { get; set; }
}