import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';

import { User } from '../_model/user';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from '../../../environments/environment';
import { BehaviorSubject, Observable } from 'rxjs';
import { Router } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { AlertifyService } from './alertify.service';
import { IBuilding } from '../_model/building';
import { IRole } from '../_model/role';
import { ResponseDetail } from '../_model/responseDetail';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  baseUrl = environment.apiUrl + 'auth/login';
  jwtHelper = new JwtHelperService();
  currentUser: User;
  roleValue = new BehaviorSubject<IRole>({ id: 0, name: '' });
  buildingValue = new BehaviorSubject<IBuilding[]>([
    {} as IBuilding ]);
  decodedToken: any;
  profile: any;
  levelSource = new BehaviorSubject<any>({});
  currentLevel = this.levelSource.asObservable();
  constructor(
    private http: HttpClient,
    private router: Router,
    private alertify: AlertifyService,
    private cookieService: CookieService) {}

  login(model: any) {
    return this.http.post(this.baseUrl, model).pipe(
      map((response: any) => {
        const data = response;
        if (data) {
          localStorage.setItem('token', data.token);
          localStorage.setItem('user', JSON.stringify(data.user));
          localStorage.setItem('avatar', data.user.User.image);
          this.decodedToken = this.jwtHelper.decodeToken(data.token);
          this.currentUser = data.user.User;
          // this.getBuildingByUserID(data.user.User.ID).subscribe((res: any) => {
          //   res = res || {};
          //   localStorage.setItem('level', JSON.stringify(res));
          //   this.role = res as IRole;
          //   this.levelSource.next(res);
          // });
          this.getBuildingUserByUserID(data.user.User.ID)
            .subscribe((res) => {
              localStorage.setItem('building', JSON.stringify(res.data));
              this.setBuildingValue(res.data as IBuilding[]);
          });
        }
      })
    );
  }

  getBuildingUserByUserID(userID) {
    const url = `${environment.apiUrlEC}BuildingUser/GetBuildingUserByUserID/${userID}`;
    return this.http.get<ResponseDetail<IBuilding[]>>(url, {});
  }
  loggedIn() {
    const token = localStorage.getItem('token');
    return !this.jwtHelper.isTokenExpired(token);
  }
  logOut() {
    this.cookieService.deleteAll();
    localStorage.clear();
    this.decodedToken = null;
    this.currentUser = null;
    this.buildingValue = null;
    this.roleValue = null;
    // this.alertify.message('Logged out');
    // const uri = this.router.url;
    this.router.navigate(['login']);
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
}
