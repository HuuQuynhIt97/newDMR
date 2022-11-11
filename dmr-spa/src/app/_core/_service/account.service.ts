import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Tutorial } from '../_model/tutorial';
import { PaginatedResult } from '../_model/pagination';
import { IUserRole } from '../_model/role';
import { ResponseDetail } from '../_model/responseDetail';
const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    Authorization: 'Bearer ' + localStorage.getItem('token')
  })
};
@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = environment.apiUrlEC;
  authUrl = environment.apiUrl2;
  messageSource = new BehaviorSubject<number>(0);
  currentMessage = this.messageSource.asObservable();
  // method này để change source message
  changeMessage(message) {
    this.messageSource.next(message);
  }
  constructor(private http: HttpClient) { }
  getAllUsers(page?, pageSize? ): Observable<PaginatedResult<object[]>> {
    const paginatedResult: PaginatedResult<object[]> = new PaginatedResult<
    object[]
    >();
    return this.http
      .get<object[]>(`${this.authUrl}Users/GetAllUsers/${page}/${pageSize}`, {
        observe: 'response'
      })
      .pipe(
        map(response => {
          paginatedResult.result = response.body;
          if (response.headers.get('Pagination') != null) {
            paginatedResult.pagination = JSON.parse(
              response.headers.get('Pagination')
              );
            }
          return paginatedResult;
        })
      );
  }
  deleteUser(id) { return this.http.delete(`${this.authUrl}Users/Delete/${id}/${environment.systemCode}`); }
  mapBuildingUser(userid, buildingid) { return this.http.get(`${this.baseUrl}BuildingUser/MapBuildingUser/${userid}/${buildingid}`); }
  getBuildings() { return this.http.get(`${this.baseUrl}Building/GetBuildings`); }
  updateUser(update) { return this.http.post(`${this.authUrl}Users/Update`, update); }
  createUser(create) { return this.http.post(`${this.authUrl}Users/Create`, create); }
  getBuildingUsers() {
      return this.http.get(`${this.baseUrl}BuildingUser/GetAllBuildingUsers`);
  }
  getLineByUserID(userid: number, buildingID: any) {
    return this.http.get<ResponseDetail<any>>(`${this.baseUrl}BuildingUser/GetLineByUserID/${userid}/${buildingID}`);
  }
  removeLineUser(create) { return this.http.post(`${this.baseUrl}BuildingUser/RemoveLineUser`, create); }
  mapLineUser(create) { return this.http.post(`${this.baseUrl}BuildingUser/mapLineUser`, create); }

  getBuildingByUserID(userid: number) {
    return this.http.get<ResponseDetail<any>>(`${this.baseUrl}BuildingUser/GetBuildingByUserID/${userid}`);
  }
  removeMultipleBuildingUser(create) { return this.http.post(`${this.baseUrl}BuildingUser/RemoveMultipleBuildingUser`, create); }
  mapMultipleBuildingUser(create) { return this.http.post(`${this.baseUrl}BuildingUser/MapMultipleBuildingUser`, create); }

}
