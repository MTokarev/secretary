using System;
using Secretary.DTOs;
using Secretary.Enums;

namespace Secretary.Models
{
    public class ResultSecret
    {
        public SecretValidationResult ValidationResult { get; set; }
        public string Message { get; set; }
        public SecretDto SecretDto { get; set; }
    }
}

