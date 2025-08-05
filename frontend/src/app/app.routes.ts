import { Routes } from '@angular/router';
import {GalleryComponent} from "./pages/gallery/gallery.component";
import {GifsComponent} from "./pages/gifs/gifs.component";
import {LoginComponent} from "./pages/login/login.component";
import {authGuard} from "./guards/auth.guard";
import {UsersComponent} from "./pages/users/users.component";

export const routes: Routes = [
    {
        path: "",
        redirectTo: "gallery",
        pathMatch: "full",
    },
    {
        path: "login",
        component: LoginComponent,
    },
    {
        path: "users",
        component: UsersComponent,
        canActivate: [authGuard()],
    },
    {
        path: "gallery",
        component: GalleryComponent,
        canActivate: [authGuard()],
    },
    {
        path: "gifs",
        component: GifsComponent,
        canActivate: [authGuard()],
    }
];
