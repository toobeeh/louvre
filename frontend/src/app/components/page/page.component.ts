import { Component } from '@angular/core';
import {AsyncPipe, NgForOf, NgIf} from "@angular/common";
import {RouterLink} from "@angular/router";
import {UserService} from "../../services/user.service";
import {UserDto} from "../../../api";
import {Observable} from "rxjs";
import {UsertypePipe} from "../../pipes/usertype.pipe";

@Component({
    selector: 'app-page',
    imports: [
        NgForOf,
        RouterLink,
        AsyncPipe,
        UsertypePipe,
        NgIf
    ],
    templateUrl: './page.component.html',
    standalone: true,
    styleUrl: './page.component.css'
})
export class PageComponent {

    protected user$: Observable<UserDto | null>;

    constructor(private readonly userService: UserService) {
        this.user$ = userService.user$;
    }

    protected readonly nav = [
        {name: "gallery", link: "/gallery", emoji: "🖼️"},
        {name: "gifs", link: "/gifs", emoji: "🎞️"},
    ]

}
