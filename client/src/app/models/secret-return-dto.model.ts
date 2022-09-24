export class SecretReturnDto {
  id: string = "";
  selfRemovalAllowed: boolean = false;
  removalKey: string= "";
  body: string= "";
  accessPassword: string= "";
  accessAttemptsLeft: number = 0;
  availableFromUtc: string= "";
  availableUntilUtc: string= ""
}