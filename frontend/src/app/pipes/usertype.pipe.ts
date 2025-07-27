import { Pipe, PipeTransform } from '@angular/core';
import {UserTypeEnum} from "../../api";

@Pipe({
  name: 'usertype'
})
export class UsertypePipe implements PipeTransform {

  transform(value: number): string {
    switch (value) {
      case 0:
        return "Administrator";
      case 1:
        return "Contributor";
      case 2:
        return "Moderator";
      case 3:
        return "Drawer";
    }

    throw new Error("Unknown user type: " + value);
  }

}
