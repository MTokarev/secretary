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
            switch (result.ValidationResult)
            {
                case SecretValidationResult.SuccessfullyValidated:
                    return Results.Ok(result);
                case SecretValidationResult.PasswordIncorrect:
                case SecretValidationResult.PasswordRequired:
                    return Results.BadRequest(result);
                case SecretValidationResult.NotFound:
                case SecretValidationResult.EarlyToShow:
                    return Results.NotFound(result);
                case SecretValidationResult.Expired:
                    return Results.BadRequest(result);
                default:
                    return Results.Problem();
            }
        }
    }
}

