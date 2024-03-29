export class SecretCreateDto {
  sharedByEmail: string | null = null;
  body: string = "";
  accessPassword: string = "";
  selfRemovalAllowed: boolean = false;
  accessAttemptsLeft: number = 0;
  availableFromUtc: string = "";
  availableUntilUtc: string = ""

  public get hasAccessPassword() {
    return this.body.length > 0;
  }
}
