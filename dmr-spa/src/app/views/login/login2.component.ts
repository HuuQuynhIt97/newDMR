import { Component, OnInit, ViewChild } from '@angular/core';
import { AuthService } from '../../_core/_service/auth.service';
import { AlertifyService } from '../../_core/_service/alertify.service';
import { Router, ActivatedRoute, RouterStateSnapshot } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { UserForLogin } from 'src/app/_core/_model/user';
import { environment } from 'src/environments/environment';
import { RoleService } from 'src/app/_core/_service/role.service';
import { IRole, IUserRole } from 'src/app/_core/_model/role';
import { IBuilding } from 'src/app/_core/_model/building';
const ADMIN = 1;
const ADMIN_COSTING = 5;
const SUPERVISOR = 2;
const STAFF = 3;
const WORKER = 4;
const WORKER2 = 6;
const DISPATCHER = 6;
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class Login2Component implements OnInit {
  user: UserForLogin = {
    username: '',
    password: '',
    systemCode: environment.systemCode
  };
  uri: any;
  level: number;
  routerLinkAdmin = [
    // setting
    '/setting/account-1',
    '/setting/building',
    '/setting/supplier',
    '/setting/ingredient',
    '/setting/kind',
    '/setting/part',
    '/setting/material',
    '/setting/building-setting',
    '/setting/costing',
    // Establish
    '/establish/bpfc',
    '/establish/bpfc-schedule',
    '/establish/bpfc-status',

    // Excution
    '/execution/todolist',
    '/execution/output-quantity',
    '/execution/workplan',
    '/execution/incoming',

    // Troubleshooting
    '/troubleshooting/search',
    '/troubleshooting/Abnormal-List',
    // Report
    '/report/consumption',
    '/report/inventory',
    '/report/delivered-history'
  ];
  routerLinkAdminCosting = [
    '/setting/costing'
  ];
  routerLinkSupervisor = [
    // setting
    '/setting/account-1',
    '/setting/building',
    '/setting/supplier',
    '/setting/ingredient',
    '/setting/kind',
    '/setting/part',
    '/setting/material',
    '/setting/building-setting',
    // '/setting/costing',
    // Establish
    '/establish/bpfc',
    '/establish/bpfc-schedule',
    '/establish/bpfc-status',

    // Excution
    '/execution/todolist',
    '/execution/output-quantity',
    '/execution/workplan',
    '/execution/incoming',

    // Troubleshooting
    '/troubleshooting/search',
    '/troubleshooting/Abnormal-List',
    // Report
    '/report/consumption',
    '/report/inventory',
    '/report/delivered-history'
  ];
  routerLinkStaff = [
    // setting
    '/setting/supplier',
    '/setting/ingredient',
    '/setting/kind',
    '/setting/part',
    '/setting/material',
    '/setting/building-setting',
    // Establish
    '/establish/bpfc',
    '/establish/bpfc-schedule',
    // Excution
    '/execution/workplan',
    '/execution/incoming',

    // Troubleshooting
    '/troubleshooting/search',
    '/troubleshooting/Abnormal-List',
    // Report
  ];
  routerLinkWorker = [
    // Excution
    '/execution/todolist-2',
    '/execution/output-quantity',
    '/execution/incoming',
    '/execution/workplan',
    '/setting/ingredient',
  ];
  routerLinkWorker2 = [
    // Excution
    '/execution/todolist-2',
    '/execution/output-quantity',
    '/execution/incoming',
    '/execution/workplan',
    '/setting/ingredient',
  ];
  routerLinkDispatcher = [
    // Excution
    '/execution/todolist-2',
    '/execution/output-quantity',
    '/execution/incoming',
    '/execution/workplan',
    '/setting/ingredient',
  ];
  remember = false;
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private roleService: RoleService,
    private cookieService: CookieService,
    private alertifyService: AlertifyService
  ) {
    if (this.cookieService.get('remember') !== undefined) {
      if (this.cookieService.get('remember') === 'Yes') {
        this.user = {
          username: this.cookieService.get('username'),
          password: this.cookieService.get('password'),
          systemCode: environment.systemCode
        };
        this.remember = true;
        this.login();
      }
    }
    this.route.queryParams.subscribe(params => {
      this.uri = params.uri;
    });
  }
  role: number;
  ngOnInit(): void {
    if (localStorage.getItem('token')) {
      const uri = decodeURI(this.uri);
      if (uri !== 'undefined') {
        this.router.navigate([uri]);
      } else {
        this.router.navigate(['/ec/execution/todolist-2']);
      }
    }
  }
  onChangeRemember(args) {
    this.remember = args.target.checked;
  }
  login(): void {
    this.authService.login(this.user).subscribe(
      next => {
        this.role = JSON.parse(localStorage.getItem('user')).User.Role;
        const userId = JSON.parse(localStorage.getItem('user')).User.ID;
        this.roleService.getRoleByUserID(userId).subscribe((res: any) => {
          res = res || {};
          const userRole: IUserRole = {
            isLock: true,
            userID: userId,
            roleID: res.id
          };
          this.roleService.isLock(userRole).subscribe((isLock: boolean) => {
            if (isLock) {
              this.alertifyService.error('Your account has been locked!!!');
              return;
            } else {
              this.alertifyService.success('Login Success!!');
              if (this.remember) {
                this.cookieService.set('remember', 'Yes');
                this.cookieService.set('username', this.user.username);
                this.cookieService.set('password', this.user.password);
                this.cookieService.set('systemCode', this.user.systemCode.toString());
              } else {
                this.cookieService.set('remember', 'No');
                this.cookieService.set('username', '');
                this.cookieService.set('password', '');
                this.cookieService.set('systemCode', '');
              }
              localStorage.setItem('level', JSON.stringify(res));
              this.authService.setRoleValue(res as IRole);
              this.level = res.id;
              if (this.level === WORKER) {
                const currentLang = localStorage.getItem('lang');
                if (currentLang) {
                  localStorage.setItem('lang', currentLang);
                } else {
                  localStorage.setItem('lang', 'vi');
                }
              } else {
                const currentLang = localStorage.getItem('lang');
                if (currentLang) {
                  localStorage.setItem('lang', currentLang);
                } else {
                  localStorage.setItem('lang', 'en');
                }
              }
              this.checkRole();
            }
          });
        });
      },
      error => {
        this.alertifyService.error('Login failed!!');
      },
      () => {
      }
    );
  }
  checkRouteAdminCosting(uri) {
    let flag = false;
    this.routerLinkAdminCosting.forEach(element => {
      if (uri.includes(element)) {
        flag = true;
      }
    });
    return flag;
  }
  checkRouteAdmin(uri) {
    let flag = false;
    this.routerLinkAdmin.forEach(element => {
      if (uri.includes(element)) {
        flag = true;
      }
    });
    return flag;
  }
  checkRouteSupervisor(uri) {
    let flag = false;
    this.routerLinkSupervisor.forEach(element => {
      if (uri.includes(element)) {
        flag = true;
      }
    });
    return flag;
  }
  checkRouteStaff(uri) {
    let flag = false;
    this.routerLinkStaff.forEach(element => {
      if (uri.includes(element)) {
        flag = true;
      }
    });
    return flag;
  }
  checkRouteWorker(uri) {
    let flag = false;
    this.routerLinkWorker.forEach(element => {
      if (uri.includes(element)) {
        flag = true;
      }
    });
    return flag;
  }
  checkRouteWorker2(uri) {
    let flag = false;
    this.routerLinkWorker2.forEach(element => {
      if (uri.includes(element)) {
        flag = true;
      }
    });
    return flag;
  }
  checkRouteDispatcher(uri) {
    let flag = false;
    this.routerLinkDispatcher.forEach(element => {
      if (uri.includes(element)) {
        flag = true;
      }
    });
    return flag;
  }
  checkRole() {
    const uri = decodeURI(this.uri);
    switch (this.level) {
      case ADMIN:
        if (uri === 'undefined') {
          this.router.navigate(['/ec/execution/todolist-2']);
          break;
        }
        if (this.checkRouteAdmin(uri)) {
          this.router.navigate([uri]);
          break;
        }
        this.authService.logOut();
        break;
      case SUPERVISOR:
        if (uri === 'undefined') {
          this.router.navigate(['/ec/execution/todolist-2']);
          break;
        }
        if (this.checkRouteSupervisor(uri)) {
          this.router.navigate([uri]);
          break;
        }
        this.authService.logOut();
        break;
      case STAFF:
        if (uri === 'undefined') {
          this.router.navigate(['/ec/execution/todolist-2']);
          break;
        }
        if (this.checkRouteStaff(uri)) {
          this.router.navigate([uri]);
          break;
        }
        this.authService.logOut();
        break;
      case ADMIN_COSTING:
        if (uri === 'undefined') {
          this.router.navigate(['/ec/setting/costing']);
          break;
        }
        if (this.checkRouteAdminCosting(uri)) {
          this.router.navigate([uri]);
          break;
        }
        this.authService.logOut();
        break;
      case WORKER:
        if (uri === 'undefined') {
          this.router.navigate(['/ec/execution/todolist-2']);
          break;
        }
        if (this.checkRouteWorker(uri)) {
          this.router.navigate([uri]);
          break;
        }
        this.authService.logOut();
        break;
      case DISPATCHER:
        if (uri !== 'undefined') {
          this.router.navigate(['/ec/execution/todolist-2']);
          break;
        }
        if (this.checkRouteDispatcher(uri)) {
          this.router.navigate([uri]);
          break;
        }
        this.authService.logOut();
        break;
    }
  }
}
