import { Component } from '@angular/core';
import {OAuthService} from "angular-oauth2-oidc";
import {Router} from "@angular/router";

@Component({
  selector: 'app-login',
  imports: [],
  standalone: true,
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {

  constructor(private readonly oauthService: OAuthService, private readonly router: Router) {
    oauthService.loadDiscoveryDocumentAndTryLogin().then(() => {

      /* if already logged in, navigate to start */
      if (this.oauthService.hasValidAccessToken()) {
        this.router.navigate(["/"]);
      }
    });
  }

  protected login(){
    this.oauthService.loadDiscoveryDocumentAndTryLogin().then(() => {
      if (!this.oauthService.hasValidAccessToken()) {
        this.oauthService.initCodeFlow();
      }
      else {
        this.router.navigate(["/"]);
      }
    });
  }

}
