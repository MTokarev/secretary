export enum SecretValidationResult {
  SuccessfullyValidated,
  NotFound,
  EarlyToShow,
  Expired,
  PasswordRequired,
  PasswordIncorrect
}