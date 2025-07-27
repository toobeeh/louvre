import {Component, Input} from '@angular/core';
import {CloudImageDto} from "../../../api";
import {NgIf} from "@angular/common";

@Component({
  selector: 'app-cloud-image',
  imports: [
    NgIf
  ],
  templateUrl: './cloud-image.component.html',
  standalone: true,
  styleUrl: './cloud-image.component.css'
})
export class CloudImageComponent {

  @Input()
  public image?: CloudImageDto;

}
