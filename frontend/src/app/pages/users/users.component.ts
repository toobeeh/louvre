import { Component } from '@angular/core';
import {AsyncPipe, NgForOf, NgIf} from "@angular/common";
import {BehaviorSubject, Observable} from "rxjs";
import {UserDto, UsersService, UserTypeEnum} from "../../../api";
import {UserService} from "../../services/user.service";
import {UsertypePipe} from "../../pipes/usertype.pipe";

@Component({
    selector: 'app-users',
    imports: [
        AsyncPipe,
        NgForOf,
        NgIf,
        UsertypePipe
    ],
    templateUrl: './users.component.html',
    styleUrl: './users.component.css',
    standalone: true
})
export class UsersComponent {

    protected readonly users$ = new BehaviorSubject<UserDto[] | null>(null);
    protected readonly user$: Observable<UserDto | null>;

    constructor(private readonly usersService: UsersService, private readonly userService: UserService) {
        this.user$ = userService.user$;
        this.loadUsers();
    }

    protected loadUsers() {
        this.usersService.getAuthorizedUsers().subscribe(users => {
            this.users$.next(users);
        });
    }

    protected renameUser(user: UserDto) {
        const value = prompt("Enter new name for user", user.name);
        const name = value?.trim();
        if(name === undefined || name === null || name.length === 0) {
            alert("Name cannot be empty");
            return;
        }

        this.usersService.renameUser(user.typoId, {newName: name}).subscribe({
            next: user =>{
                alert("User renamed successfully");
                this.loadUsers();
            },
            error: err => {
                console.error("Error renaming user", err);
                alert("Failed to rename user");
            }
        });
    }

    protected changeType(user: UserDto) {
        const value = prompt("Enter new type for user (1: moderator, 2: contributor, 3: drawer)", user.userType?.toString());
        const type = Number(value?.trim());
        if(isNaN(type) || type > 3 || type < 1) {
            alert("Invalid user type");
            return;
        }

        this.usersService.promoteUser(user.typoId, {newUserType: type as UserTypeEnum}).subscribe({
            next: user => {
                alert("User type changed successfully");
                this.loadUsers();
            },
            error: err => {
                console.error("Error changing user type", err);
                alert("Failed to change user type");
            }
        });
    }

    protected addUser() {
        const idValue = prompt("Enter the discord id of the user to add");
        const id = idValue?.trim() ?? "";
        if(id.length <= 0) {
            alert("Invalid user discord id");
            return;
        }

        const typeValue = prompt("Enter new type for user (1: moderator, 2: contributor, 3: drawer)");
        const type = Number(typeValue?.trim());
        if(isNaN(type) || type > 3 || type < 1) {
            alert("Invalid user type");
            return;
        }

        this.usersService.authorizeUser({discordId: id, userType: type as UserTypeEnum}).subscribe({
            next: user => {
                alert("User added successfully");
                this.loadUsers();
            },
            error: err => {
                console.error("Error adding user", err);
                alert("Failed to add user");
            }
        });
    }

}
