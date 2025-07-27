import { Injectable } from '@angular/core';
import {OAuthService} from "angular-oauth2-oidc";
import {distinctUntilChanged, switchMap, map, tap, of, BehaviorSubject} from "rxjs";
import {AuthorizeUserDto, UserDto, UsersService} from "../../api";

@Injectable({
  providedIn: 'root'
})
export class UserService {

    private readonly _user$ = new BehaviorSubject<UserDto | null>(null);

  constructor(private readonly oauthService: OAuthService, private readonly usersService: UsersService) {
    oauthService.events.pipe(
        map(() => oauthService.hasValidAccessToken()),
        distinctUntilChanged(),
        switchMap(token => {
          if(token === false) return of(null);

          return usersService.getCurrentUser();
        })
    ).subscribe(user => this._user$.next(user));
  }

  public get user$() {
      return this._user$.asObservable();
  }
}
