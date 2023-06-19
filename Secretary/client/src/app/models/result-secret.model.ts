import { SecretValidationResult } from "../enums/secret-validation-result.enum";
import { SecretReturnDto } from "./secret-return-dto.model";
import { SecretShowDto } from "./secret-show-dto.model";

export interface ResultSecret {
    validationResult: SecretValidationResult;
    message: string;
    secretDto: SecretShowDto | null;
}