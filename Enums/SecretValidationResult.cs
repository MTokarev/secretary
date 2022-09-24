using System;
namespace Secretary.Enums
{
    public enum SecretValidationResult
    {
        SuccessfullyValidated,
        NotFound,
        EarlyToShow,
        Expired,
        PasswordRequired,
        PasswordIncorrect
    }
}

