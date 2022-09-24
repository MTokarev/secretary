export class SecretShowDto {
  body: string= "";
  selfRemovalAllowed: boolean = false;
  removalKey: string= "";
  accessAttemptsLeft: number = 0;
  availableFromUtc: string= "";
  availableUntilUtc: string= ""
}