import { Routes } from '@angular/router';
import {GalleryComponent} from "./pages/gallery/gallery.component";
import {GifsComponent} from "./pages/gifs/gifs.component";

export const routes: Routes = [
    {
        path: "",
        redirectTo: "gallery",
        pathMatch: "full",
    },
    {
        path: "gallery",
        component: GalleryComponent
    },
    {
        path: "gifs",
        component: GifsComponent
    }
];
