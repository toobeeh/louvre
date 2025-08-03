import {ApplicationConfig, inject, provideZoneChangeDetection} from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import {AuthConfig, OAuthService, provideOAuthClient} from "angular-oauth2-oidc";
import {provideHttpClient} from "@angular/common/http";
import { Configuration } from "../api";
import {environment} from "../environments/environment";

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideHttpClient(),
    provideOAuthClient(),
    provideRouter(routes),
    {
      provide: Configuration,
      useFactory: () => {
        const authService = inject(OAuthService);

        return new Configuration({
          basePath: environment.apiBaseUrl,
          credentials: {
            "oauth2": () => authService.getAccessToken()
          }
        });
      },
      deps: [OAuthService],
      multi: false
    },
  ]
};

export const authConfig: AuthConfig = {
  issuer: "https://api.typo.rip/openid",
  redirectUri: window.location.origin,
  clientId: environment.oauthClientId,
  responseType: "code",
  strictDiscoveryDocumentValidation: false,
  disablePKCE: true,
}
