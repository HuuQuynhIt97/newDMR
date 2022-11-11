import { AlertifyService } from './alertify.service';
import { Injectable, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, Subscription, throwError } from 'rxjs';
import { map, tap, delay, finalize, catchError } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { ApplicationUser, FunctionSystem } from '../_model/application-user';
import { JwtHelperService } from '@auth0/angular-jwt';
import { IBuilding } from '../_model/building';
import { IRole, IUserRole } from '../_model/role';
import { User } from '../_model/user';
import { CookieService } from 'ngx-cookie-service';
import { ResponseDetail } from '../_model/responseDetail';
import { RoleService } from './role.service';

interface LoginResult {
  id: number;
  username: string;
  role: string;
  originalUserName: string;
  accessToken: string;
  refreshToken: string;
}

interface UserForReturnLogin {
  id: number;
  username: string;
  alias: string;
  role: number;
  oCLevel: number;
  listOCs: any;
  image: string;
  isLeader: boolean;
  subscribeLine: boolean;
  employeeID: string;
  systems: number[];
}

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService implements OnDestroy {
  baseUrl = environment.apiUrl + 'auth/login';
  jwtHelper = new JwtHelperService();
  currentUser: User;
  roleValue = new BehaviorSubject<IRole>({ id: 0, name: '' });


  buildingValue = new BehaviorSubject<IBuilding[]>([
    {} as IBuilding]);
  decodedToken: any;
  profile: any;
  levelSource = new BehaviorSubject<any>({});
  currentLevel = this.levelSource.asObservable();

  private readonly apiUrl = `${environment.apiUrl}account`;
  private timer: Subscription;
  // tslint:disable-next-line:variable-name
  private _user = new BehaviorSubject<ApplicationUser>(null);
  user$: Observable<ApplicationUser> = this._user.asObservable();

  // tslint:disable-next-line:variable-name
  private _function = new BehaviorSubject<FunctionSystem>(null);
  function$: Observable<FunctionSystem> = this._function.asObservable();

  public functionsValue = new BehaviorSubject<FunctionSystem>
    ({} as FunctionSystem);
  private storageEventListener(event: StorageEvent) {
    if (event.storageArea === localStorage) {
      if (event.key === 'logout-event') {
        this.stopTokenTimer();
        this._user.next(null);
        this.setFunctions(null);
      }
      if (event.key === 'login-event') {
        this.stopTokenTimer();
        this.http.get<LoginResult>(`${this.apiUrl}/user`).subscribe((x) => {
          this._user.next(x as ApplicationUser);
        });
      }
      if (event.key === 'functions') {
        if (JSON.stringify(event.oldValue) !== JSON.stringify(event.newValue)) {
          this.logOut();
          window.location.reload();
        }
      }
      if (event.key === 'level') {
        if (JSON.stringify(event.oldValue) !== JSON.stringify(event.newValue)) {
          this.logOut();
          window.location.reload();
        }
      }
      if (event.key === 'user') {
        if (JSON.stringify(event.oldValue) !== JSON.stringify(event.newValue)) {
          this.logOut();
          window.location.reload();
        }
      }
      if (event.key === 'menus') {
        if (JSON.stringify(event.oldValue) !== JSON.stringify(event.newValue)) {
          this.logOut();
          window.location.reload();
        }
      }
    }
  }

  constructor(
    private router: Router,
    private http: HttpClient,
    private cookieService: CookieService,
    private alertify: AlertifyService,
    private roleService: RoleService
  ) {
    window.addEventListener('storage', this.storageEventListener.bind(this));
  }

  ngOnDestroy(): void {
    window.removeEventListener('storage', this.storageEventListener.bind(this));
  }

  login(username: string, password: string) {
    return this.http
      .post<LoginResult>(`${this.apiUrl}/login`, { username, password })
      .pipe(
        map((x: any) => {
          const loginResult = x.loginResult as LoginResult;
          const user = x.user;
          localStorage.setItem('user', JSON.stringify(user));
          localStorage.setItem('avatar', user.user.image);
          this._user.next(loginResult as ApplicationUser);
          this.decodedToken = this.jwtHelper.decodeToken(loginResult.accessToken);
          this.currentUser = user.user;
          this.setLocalStorage(loginResult);
          this.startTokenTimer();
          return loginResult;
        })
      );
  }
  getBuildingUserByUserID(userID) {
    const url = `${environment.apiUrlEC}BuildingUser/GetBuildingUserByUserID/${userID}`;
    return this.http.get<ResponseDetail<IBuilding[]>>(url, {});
  }
  logout() {
    this.http
      .post<unknown>(`${this.apiUrl}/logout`, {})
      .pipe(
        finalize(() => {
          this.clearLocalStorage();
          this._user.next(null);
          this.setBuildingValue(null);
          this.setRoleValue(null);
          this._user.next(null);
          this.stopTokenTimer();
        })
      )
      .subscribe();
  }

  refreshToken() {
    const refreshToken = localStorage.getItem('refresh_token');
    if (!refreshToken || refreshToken === undefined + '') {
      this.clearLocalStorage();
      return of(null);
    }

    return this.http
      .post<LoginResult>(`${this.apiUrl}/refresh-token`, { refreshToken })
      .pipe(
        catchError(err => of([])),
        map((x: LoginResult) => {
          this._user.next(x as ApplicationUser);
          this.setLocalStorage(x);
          this.startTokenTimer();
          return x;
        })
      );
  }

  setLocalStorage(x: LoginResult) {
    localStorage.setItem('token', x.accessToken);
    localStorage.setItem('refresh_token', x.refreshToken);
    localStorage.setItem('login-event', 'login' + Math.random());
  }

  clearLocalStorage() {
    localStorage.clear();
    localStorage.setItem('logout-event', 'logout' + Math.random());
  }

  private getTokenRemainingTime() {
    const accessToken = localStorage.getItem('token');
    if (!accessToken) {
      return 0;
    }
    const jwtToken = JSON.parse(atob(accessToken.split('.')[1]));
    const expires = new Date(jwtToken.exp * 1000);
    return expires.getTime() - Date.now();
  }

  private startTokenTimer() {
    const timeout = this.getTokenRemainingTime();
    this.timer = of(true)
      .pipe(
        delay(timeout),
        tap(() => this.refreshToken().subscribe())
      )
      .subscribe();
  }

  private stopTokenTimer() {
    this.timer?.unsubscribe();
  }
  public loggedIn() {
    const token = localStorage.getItem('token');
    return !this.jwtHelper.isTokenExpired(token);
  }
  logOut() {
    this.cookieService.deleteAll();
    localStorage.clear();
    this.decodedToken = null;
    this.currentUser = null;
    this.functionsValue.next({} as FunctionSystem);
    this._user.next({} as ApplicationUser);
    this.buildingValue.next([{} as IBuilding]);
    this.roleValue.next({} as IRole);
    // const uri = this.router.url;
    // this.router.navigate(['login'], { queryParams: { uri } });
  }
  roleMatch(allowedRoles): boolean {
    let isMatch = false;
    const userRoles = this.decodedToken.role as Array<string>;
    allowedRoles.forEach(element => {
      if (userRoles.includes(element)) {
        isMatch = true;
        return;
      }
    });
    return isMatch;
  }

  public setBuildingValue(building: IBuilding[]): void {
    this.buildingValue.next(building);
  }
  public getBuildingValue(): Observable<IBuilding[]> {
    return this.buildingValue.asObservable();
  }
  public setRoleValue(role: IRole): void {
    this.roleValue.next(role);
  }
  public getRoleValue(): Observable<IRole> {
    return this.roleValue.asObservable();
  }

  public setFunctions(functions: FunctionSystem): void {
    this.functionsValue.next(functions);
  }
  public getFunctionsValue(): Observable<FunctionSystem> {
    return this.functionsValue.asObservable();
  }
}
