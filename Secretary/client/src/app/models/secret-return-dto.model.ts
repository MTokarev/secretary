export class SecretReturnDto {
  id: string = "";
  selfRemovalAllowed: boolean = false;
  removalKey: string= "";
  body: string= "";
  accessPassword: string= "";
  accessAttemptsLeft: number = 0;
  createdOnUtc: string = "";
  availableFromUtc: string= "";
  availableUntilUtc: string= "";
  hasBeenCopied: boolean = false
  decryptionKey: string = "";
  linkToTheSecretCopied: boolean = true;
}
