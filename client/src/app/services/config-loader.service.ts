import { HttpBackend, HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { AppConfig } from '../models/app-config.model';

@Injectable({
  providedIn: 'root'
})
export class ConfigLoaderService {
  static config: AppConfig;
  private httpClient: HttpClient;

  constructor(handler: HttpBackend) {
    this.httpClient = new HttpClient(handler);
   }

   load() {
    const configFile = `assets/config/config.${environment.name}.json`;
    return new Promise<void>((resolve, reject) => {
      this.httpClient.get(configFile).subscribe({
        next: (response) => {
          ConfigLoaderService.config = <AppConfig>response;

          if (environment.production) {
            const baseUrl: string = window.location.origin + '/';
            ConfigLoaderService.config.siteConfig.siteAddress = baseUrl;
            ConfigLoaderService.config.urls.base = baseUrl;
          }
          
          console.log(ConfigLoaderService.config.siteConfig.siteAddress);
          resolve();
        },
        error: (error: any) => {
          reject(`Could not load file '${configFile}': ${JSON.stringify(error)}`);
        }
      })
    });
   }
}
