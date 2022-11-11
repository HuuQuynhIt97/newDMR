import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Tutorial } from '../_model/tutorial';
import { HierarchyNode, IBuilding } from '../_model/building';
const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    Authorization: 'Bearer ' + localStorage.getItem('token')
  })
};
@Injectable({
  providedIn: 'root'
})
export class BuildingLunchTimeService {
  baseUrl = environment.apiUrlEC;
  messageSource = new BehaviorSubject<number>(0);
  currentMessage = this.messageSource.asObservable();
  // method này để change source message
  changeMessage(message) {
    this.messageSource.next(message);
  }
  constructor(private http: HttpClient) { }
  getBuildings() {
    return this.http.get<Array<IBuilding>>(`${this.baseUrl}BuildingLunchTime/GetAllBuildings`);
  }
  getPeriodMixingByBuildingID(buildingID: number) {
    return this.http.get(`${this.baseUrl}BuildingLunchTime/getPeriodMixingByBuildingID/${buildingID}`);
  }
  addOrUpdateLunchTime(item: any) {
    return this.http.post(`${this.baseUrl}BuildingLunchTime/AddOrUpdateLunchTime`, item);
  }
  updatePeriodMixing(item: any) {
    return this.http.put(`${this.baseUrl}BuildingLunchTime/updatePeriodMixing`, item);
  }
  addPeriodMixing(item: any) {
    return this.http.post(`${this.baseUrl}BuildingLunchTime/addPeriodMixing`, item);
  }
  deletePeriodMixing(id) {
    return this.http.delete(`${this.baseUrl}BuildingLunchTime/deletePeriodMixing/${id}`);
  }

  addLunchTimeBuilding(item: any) {
    return this.http.put(`${this.baseUrl}BuildingLunchTime/AddLunchTimeBuilding`, item);
  }
  updatePeriodDispatch(item: any) {
    return this.http.put(`${this.baseUrl}BuildingLunchTime/updatePeriodDispatch`, item);
  }
  addPeriodDispatch(item: any) {
    return this.http.post(`${this.baseUrl}BuildingLunchTime/addPeriodDispatch`, item);
  }
  deletePeriodDispatch(id) {
    return this.http.delete(`${this.baseUrl}BuildingLunchTime/deletePeriodDispatch/${id}`);
  }
  getPeriodDispatchByPeriodMixingID(periodMixingID: number) {
    return this.http.get(`${this.baseUrl}BuildingLunchTime/GetPeriodDispatchByPeriodMixingID/${periodMixingID}`);
  }
}
