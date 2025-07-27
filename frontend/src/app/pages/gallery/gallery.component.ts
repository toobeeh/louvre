import {Component} from '@angular/core';
import {CloudImageDto, CloudService, RendersService} from "../../../api";
import {BehaviorSubject} from "rxjs";
import {AsyncPipe, NgForOf, NgIf} from "@angular/common";
import {CloudImageComponent} from "../../components/cloud-image/cloud-image.component";

@Component({
  selector: 'app-gallery',
  imports: [
    NgIf,
    AsyncPipe,
    CloudImageComponent,
    NgForOf
  ],
  templateUrl: './gallery.component.html',
  standalone: true,
  styleUrl: './gallery.component.css'
})
export class GalleryComponent {

  private _page = 0;

  protected readonly pageSize = 50;
  protected readonly images$ = new BehaviorSubject<CloudImageDto[] | null>(null);
  protected readonly selected$ = new BehaviorSubject<CloudImageDto | null>(null);

  constructor(private readonly cloudService: CloudService, private readonly renderService: RendersService) {
    this.searchImages();
  }

  protected get page() {
    return this._page + 1; // Convert to 1-based index for display
  }

  searchImages(title?: string, own?: boolean) {
    this.cloudService.searchUserCloud({
      titleQuery: title,
      isOwnQuery: own ?? false,
      createdInPrivateLobbyQuery: false,
      page: this._page,
      pageSize: this.pageSize
    }).subscribe(images => this.images$.next(images));
  }

  nextPage() {
    this._page++;
    this.searchImages();
  }

  previousPage() {
    if (this._page > 0) {
      this._page--;
      this.searchImages();
    }
  }

  select(image: CloudImageDto | null){
    this.selected$.next(image);
  }

  submit(image: CloudImageDto) {
    this.renderService.submit({
      cloudId: image.id
    }).subscribe({
        next: () => {
            alert("Image submitted successfully!");
        },
        error: (error) => {
            alert("Error submitting image - is it already added?");
        }
    });
    this.selected$.next(null);
  }

}
