import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {OAuthService} from "angular-oauth2-oidc";
import {authConfig} from "./app.config";
import {PageComponent} from "./components/page/page.component";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, PageComponent],
  templateUrl: './app.component.html',
  standalone: true,
  styleUrl: './app.component.css'
})
export class AppComponent {

  constructor(private oauthService: OAuthService) {
    this.oauthService.configure(authConfig);
    this.oauthService.loadDiscoveryDocumentAndTryLogin().then(() => {
      if (!this.oauthService.hasValidAccessToken()) {
        oauthService.initCodeFlow();
      }
    });
  }

  title = 'Louvre';
}
