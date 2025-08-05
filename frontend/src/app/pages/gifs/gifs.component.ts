import { Component } from '@angular/core';
import {BehaviorSubject, map, Observable} from "rxjs";
import {RenderInfoDto, RenderPreviewDto, RendersService, UserDto, UsersService} from "../../../api";
import {AsyncPipe, NgForOf, NgIf} from "@angular/common";
import {UserService} from "../../services/user.service";
import {UsertypePipe} from "../../pipes/usertype.pipe";

@Component({
  selector: 'app-gifs',
  imports: [
    AsyncPipe,
    NgForOf,
    NgIf
  ],
  templateUrl: './gifs.component.html',
  standalone: true,
  styleUrl: './gifs.component.css'
})
export class GifsComponent {

  private _page = 0;

  protected readonly pageSize = 50;
  protected readonly renders$ = new BehaviorSubject<RenderPreviewDto[] | null>(null);
  protected readonly selected$ = new BehaviorSubject<RenderInfoDto | null>(null);
  protected readonly user$: Observable<UserDto | null>;
  protected readonly users$ = new BehaviorSubject<UserDto[] | null>(null);

  constructor(
      private readonly renderService: RendersService,
      private readonly userService: UserService,
      private readonly usersService: UsersService) {
    this.searchRenders();
    this.user$ = userService.user$;
    usersService.getAuthorizedUsers().subscribe(users => this.users$.next(users));
  }

  protected get page(){
    return this._page + 1; // Convert to 1-based index for display
  }

  searchRenders(rendered?: boolean, title?: string) {
    this.renderService.findRenders({
      titleIncludeQuery: title,
      rendered: rendered,
      page: this._page,
      pageSize: this.pageSize
    }).subscribe(renders => this.renders$.next(renders));
  }

  nextPage() {
    this._page++;
    this.searchRenders();
  }

  previousPage() {
    if (this._page > 0) {
      this._page--;
      this.searchRenders();
    }
  }

  select(render: RenderPreviewDto | null) {
    if(render === null) this.selected$.next(null);
    else {
      this.renderService.getRender(render.id!).subscribe(info => this.selected$.next(info));
    }
  }

  protected get userCanManage$(){
    return this.user$.pipe(
        map(user => user !== null && user.userType! < 3)
    )
  }

  proposeTitle(render: RenderPreviewDto) {
    const input = prompt("Propose a title for the render:", render.title);
    if(!input || input.trim().length === 0) return;

    this.renderService.proposeRenderTitle(render.id!, {
      title: input
    }).subscribe({
      next: () => {
        this.searchRenders();
        this.select(render);
        alert("Title proposed");
      },
      error: (err) => console.error("Failed to propose title", err)
    });
    this.select(null);
  }

  proposeDrawer(render: RenderPreviewDto, input: string) {

    const drawer = Number(input);
    if(isNaN(drawer) || drawer <= 0) {
      alert("Invalid drawer");
      return;
    }

    this.renderService.proposeRenderDrawer(render.id!, {
      drawerTypoId: drawer
    }).subscribe({
      next: () => {
        this.searchRenders();
        this.select(render);
        alert("Drawer proposed");
      },
      error: (err) => console.error("Failed to propose drawer", err)
    });
    this.select(null);
  }

  approve(render: RenderInfoDto){
    this.renderService.approveRender(render.id!).subscribe({
      next: () => {
        this.searchRenders();
        this.select(render);
        alert("Render approved");
      },
      error: (err) => console.error("Failed to approve render", err)
    });
    this.select(null);
  }

  unapprove(render: RenderInfoDto) {
    this.renderService.unapproveRender(render.id!).subscribe({
      next: () => {
        this.searchRenders();
        this.select(render);
        alert("Render unapproved");
      },
      error: (err) => console.error("Failed to unapprove render", err)
    });
    this.select(null);
  }

  remove(render: RenderInfoDto) {
    if(!confirm("Are you sure you want to remove this render? This action cannot be undone.")) return;

    this.renderService.removeRender(render.id!).subscribe({
      next: () => {
        this.searchRenders();
        this.select(null);
        alert("Render removed successfully");
      },
      error: (err) => {
        console.error("Failed to remove render", err);
        alert("Failed to remove render");
      }
    });
  }

  rerenderGif(render: RenderInfoDto) {

    const renderSecondsInput = prompt("Enter gif duration in seconds:", render.renderParameters?.durationSeconds.toString());
    const renderSeconds = Number(renderSecondsInput);
    if(isNaN(renderSeconds) || renderSeconds <= 0) {
      alert("Invalid render duration");
      return;
    }

    const renderFpsInput = prompt("Enter gif fps:", render.renderParameters?.framesPerSecond.toString());
    const renderFps = Number(renderFpsInput);
    if(isNaN(renderFps) || renderFps <= 0) {
      alert("Invalid render fps");
      return;
    }

    const renderOptimizationInput = prompt("Enter gif optimization level in %:", render.renderParameters?.optimizationLevelPercent.toString());
    const renderOptimization = Number(renderOptimizationInput);
    if(isNaN(renderOptimization) || renderOptimization < 0 || renderOptimization > 100) {
      alert("Invalid render optimization level");
      return;
    }

    this.renderService.rerenderRender(render.id!, {
      framesPerSecond: renderFps,
      durationSeconds: renderSeconds,
      optimizationLevelPercent: renderOptimization
    }).subscribe({
      next: () => {
        this.searchRenders();
        this.select(render);
        alert("Render is being processed with new parameters");
      },
      error: (err) => console.error("Failed to start rerendering", err)
    });
    this.select(null);
  }

  protected readonly performance = performance;
}
