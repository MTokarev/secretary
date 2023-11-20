using System;
using Secretary.DTOs;
using Secretary.Enums;
using Secretary.Models;

namespace Secretary.Extensions
{
    public static class SecretExtension
    {
        public static IResult GetResult(this ResultSecret result)
        {
            return result.ValidationResult switch
            {
                SecretValidationResult.SuccessfullyValidated => Results.Ok(result),
                SecretValidationResult.PasswordIncorrect => Results.BadRequest(result),
                SecretValidationResult.PasswordRequired => Results.BadRequest(result),
                SecretValidationResult.NotFound => Results.NotFound(result),
                SecretValidationResult.EarlyToShow => Results.NotFound(result),
                SecretValidationResult.Expired => Results.BadRequest(result),
                _ => Results.Problem()
            };
        }
    }
}

