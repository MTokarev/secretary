import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ResultSecret } from '../models/result-secret.model';
import { SecretCreateDto } from '../models/secret-create-dto.model';
import { SecretReturnDto } from '../models/secret-return-dto.model';
import { ConfigLoaderService } from './config-loader.service';

@Injectable({
  providedIn: 'root'
})
export class SecretService {
  private secretsUrl = ConfigLoaderService.config.urls.base + ConfigLoaderService.config.urls.secrets;

  constructor(private client: HttpClient) { }

  getSecret(secretId: string, accessKey?: string) {
    const httpOptions = {
      headers: new HttpHeaders({
        
      })
    };

    if (accessKey) {
      httpOptions.headers = httpOptions.headers.append("accessPassword", accessKey);
    }

    return this.client.get<ResultSecret>(this.secretsUrl + secretId, httpOptions)
  }

  createSecret(secretDto: SecretCreateDto, accessKey?: string) {
    return this.client.post<SecretReturnDto>(this.secretsUrl, secretDto);
  }

  deleteSecret(removalKey: string) {
    return this.client.delete(this.secretsUrl + removalKey);
  }
}
