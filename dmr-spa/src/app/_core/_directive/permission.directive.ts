import { filter } from 'rxjs/operators';
import { Directive, ElementRef, Input, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AuthenticationService } from '../_service/authentication.service';

@Directive({
  selector: '[appPermission]'
})
export class PermissionDirective implements OnInit {
  @Input() appFunction: string;
  @Input() appAction: string;

  constructor(private el: ElementRef, private route: ActivatedRoute, private authService: AuthenticationService) {
  }
  ngOnInit() {
    const loggedInUser = this.authService.loggedIn();
    if (loggedInUser) {
      const functions = JSON.parse(localStorage.getItem("functions"));
      const childrenTemp = functions.map(el => {
        return el.childrens;
      });
      const functionCode = this.route.snapshot.data.functionCode;
      const permissions = [].concat.apply([], childrenTemp).filter(x => x.functionCode === functionCode);
      if (permissions && permissions.filter(x => {
        return x.functionCode === this.appFunction && x.code === this.appAction;
      }).length > 0) {
        this.el.nativeElement.style.display = '';
      } else {
        this.el.nativeElement.style.display = 'none';
      }
    } else {
      this.el.nativeElement.style.display = 'none';
    }
  }
}
