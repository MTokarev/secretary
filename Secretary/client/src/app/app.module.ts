import { APP_INITIALIZER, LOCALE_ID, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { HttpClientModule } from '@angular/common/http';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ConfigLoaderService } from './services/config-loader.service';

import { HomeComponent } from './home/home.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SecretComponent } from './secret/secret.component';

import { ToastrModule } from 'ngx-toastr';
import { AboutComponent } from './about/about.component';
import {
  FacebookLoginProvider,
  GoogleLoginProvider,
  GoogleSigninButtonModule, MicrosoftLoginProvider, SocialAuthService,
  SocialAuthServiceConfig
} from "@abacritt/angularx-social-login";
import {MySecretsComponent} from "./my-secrets/my-secrets.component";
import {ConvertUtcToLocal} from "./utils/pipes/convert-utc-to-local";
import {UserService} from "./services/user.service";
import {AuthProviders} from "./enums/auth-providers.enum";

export function initializeApp(configLoader: ConfigLoaderService){
  return () => configLoader.load();
}

export function initializeAuth(configLoader: ConfigLoaderService) {
  const baseConfig: SocialAuthServiceConfig = {
    autoLogin: false,
    providers: [],
    onError: (err) => {
      console.error(err);
    }
  };

  // Add Facebook if config exist
  if (ConfigLoaderService.config.auth?.facebook?.clientId?.length > 0) {
    baseConfig.providers.push({
      id: FacebookLoginProvider.PROVIDER_ID,
      provider: new FacebookLoginProvider(ConfigLoaderService.config.auth.facebook.clientId),
    });
  }

  // Add Facebook if config exist
  if (ConfigLoaderService.config.auth?.facebook?.clientId?.length > 0) {
    baseConfig.providers.push({
      id: FacebookLoginProvider.PROVIDER_ID,
      provider: new FacebookLoginProvider(ConfigLoaderService.config.auth.facebook.clientId),
    });
  }

  // Add Google if config exist
  if (ConfigLoaderService.config.auth?.google?.clientId?.length > 0) {
    baseConfig.providers.push({
      id: GoogleLoginProvider.PROVIDER_ID,
      provider: new GoogleLoginProvider(ConfigLoaderService.config.auth.google.clientId, {
        // This will stop asking user to login with popup prompt
        oneTapEnabled: false
      }),
    });
  }

  // Add Microsoft if config exist
  if (ConfigLoaderService.config.auth?.microsoft?.clientId?.length > 0) {
    baseConfig.providers.push({
      id: MicrosoftLoginProvider.PROVIDER_ID,
      provider: new MicrosoftLoginProvider(ConfigLoaderService.config.auth.microsoft.clientId)
    });
  }

  return baseConfig;
}

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    SecretComponent,
    AboutComponent,
    ConvertUtcToLocal,
    MySecretsComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    ToastrModule.forRoot(),
    GoogleSigninButtonModule,
  ],
  providers: [
    ConfigLoaderService,
    {
      provide: LOCALE_ID,
      useValue: 'en'
    },
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [ConfigLoaderService],
      multi: true
    },
    {
      provide: 'SocialAuthServiceConfig',
      useFactory: initializeAuth,
      deps: [ConfigLoaderService]
    },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
