import {CanActivateFn, Router} from '@angular/router';
import {inject} from "@angular/core";
import {OAuthService} from "angular-oauth2-oidc";

export const authGuard: () => CanActivateFn = () => {
  return () => {
    const oauthService = inject(OAuthService);
    if(oauthService.hasValidAccessToken()) return true;

    const router = inject(Router);
    return router.createUrlTree(["/login"]);
  }
};
