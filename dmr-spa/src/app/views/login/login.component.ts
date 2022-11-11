import { PermissionService } from 'src/app/_core/_service/permission.service';
import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { AuthService } from '../../_core/_service/auth.service';
import { AlertifyService } from '../../_core/_service/alertify.service';
import { Router, ActivatedRoute, RouterStateSnapshot } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { UserForLogin } from 'src/app/_core/_model/user';
import { environment } from 'src/environments/environment';
import { RoleService } from 'src/app/_core/_service/role.service';
import { IRole, IUserRole } from 'src/app/_core/_model/role';
import { IBuilding } from 'src/app/_core/_model/building';
import { AuthenticationService } from 'src/app/_core/_service/authentication.service';
import { Subscription } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { FunctionSystem } from 'src/app/_core/_model/application-user';
import { NgxSpinnerService } from 'ngx-spinner';
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit, OnDestroy {
  busy = false;
  username = '';
  password = '';
  loginError = false;
  private subscription: Subscription;

  user: UserForLogin = {
    username: '',
    password: '',
    systemCode: environment.systemCode
  };
  uri: any;
  level: number;

  remember = false;
  functions: FunctionSystem[];
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthenticationService,
    private permisisonService: PermissionService,
    private roleService: RoleService,
    private spinner: NgxSpinnerService,
    private cookieService: CookieService,
    private permissionService: PermissionService,
    private alertifyService: AlertifyService
  ) {
    if (this.cookieService.get('remember') !== undefined) {
      if (this.cookieService.get('remember') === 'Yes') {
        this.user = {
          username: this.cookieService.get('username'),
          password: this.cookieService.get('password'),
          systemCode: environment.systemCode
        };
        this.username = this.cookieService.get('username');
        this.password = this.cookieService.get('password');
        this.remember = true;
        this.login();
      }
    }
    this.uri = this.route.snapshot.queryParams.uri || '/ec/execution/todolist-2';
  }
  role: number;
  ngOnInit(): void {
    const accessToken = localStorage.getItem('token');
    const refreshToken = localStorage.getItem('refresh_token');
    if (accessToken && refreshToken && this.route.routeConfig.path === 'login') {
      const uri = decodeURI(this.uri) || '/ec/execution/todolist-2';
      this.router.navigate([uri]);
    }
  }
  onChangeRemember(args) {
    this.remember = args.target.checked;
  }
  authentication() {
    return this.authService
      .login(this.username, this.password).toPromise();
  }
  async login() {
    if (!this.username || !this.password) {
      return;
    }
    this.busy = true;
    try {
      console.log('Begin authenication');
      await this.authentication();
      console.log('End authenication');

      this.role = JSON.parse(localStorage.getItem('user')).user.role;
      const userId = JSON.parse(localStorage.getItem('user')).user.id;
      console.log('Begin getBuildingUserByUserID');

      const res = await this.authService.getBuildingUserByUserID(userId).toPromise();
      console.log('end getBuildingUserByUserID', res);

      localStorage.setItem('building', JSON.stringify(res.data));
      this.authService.setBuildingValue(res.data as IBuilding[]);

      console.log('Begin getRoleByUserID');

      const roleUser = await this.roleService.getRoleByUserID(userId).toPromise();
      localStorage.setItem('level', JSON.stringify(roleUser));
      this.authService.setRoleValue(roleUser as IRole);
      console.log('end getRoleByUserID');

      console.log('Begin getMenu', userId);

      const menus = await this.permissionService.getMenuByUserPermission(userId).toPromise();
      localStorage.setItem('menus', JSON.stringify(menus));
      console.log('end getMenu', menus);

      const userRole: IUserRole = {
        isLock: true,
        userID: userId,
        roleID: roleUser.id
      };
      console.log('Begin isLock');

      const isLock = await this.roleService.isLock(userRole).toPromise();
      if (isLock) {
        this.alertifyService.error('Your account has been locked!!!');
        return;
      }
      console.log('end isLock');

      console.log('Begin getActionInFunctionByRoleID');

      const functions = await this.permisisonService.getActionInFunctionByRoleID(roleUser.id).toPromise();
      this.functions = functions as FunctionSystem[];
      localStorage.setItem("functions", JSON.stringify(functions));
      this.authService.setFunctions(functions as any);

      const currentLang = localStorage.getItem('lang');
      if (currentLang) {
        localStorage.setItem('lang', currentLang);
      } else {
        localStorage.setItem('lang', 'vi');
      }

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
      setTimeout(() => {
        const check = this.checkRole();
        if (check) {
          const uri = decodeURI(this.uri);
          this.router.navigate([uri]);
        } else {
          this.router.navigate(['/ec/execution/todolist-2']);
        }

      });
      console.log('end getActionInFunctionByRoleID');
      this.alertifyService.success('Login Success!!');
      this.busy = false;

    } catch (error) {
      this.busy = true;
      this.loginError = true;
    }
  }
  getMenu(userid) {
    this.permissionService.getMenuByUserPermission(userid).toPromise();
  }
  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  checkRole(): boolean {
    const uri = decodeURI(this.uri);
    const permissions = this.functions.map(x => x.url);
    for (const url of permissions) {
      if (uri.includes(url)) {
        return true;
      }
    }
    return false;

  }
}
