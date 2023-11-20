import { SecretValidationResult } from "../enums/secret-validation-result.enum";
import { SecretShowDto } from "./secret-show-dto.model";

export interface ResultSecret {
    validationResult: SecretValidationResult;
    message: string;
    secretExtendedDto: SecretShowDto | null;
}
