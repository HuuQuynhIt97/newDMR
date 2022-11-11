import { Injectable } from '@angular/core';
import { HttpHeaders, HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SettingService {
  baseUrl = environment.apiUrlEC;
  constructor(
    private http: HttpClient
  ) { }

  getAllSetting() {
    return this.http.get(this.baseUrl + 'Setting/GetAllSetting', {});
  }
  getAllGlueType() {
    return this.http.get(this.baseUrl + 'Setting/GetAllGlueType', {});
  }
  getSettingByBuilding(buildingID) {
    return this.http.get(this.baseUrl + 'Setting/GetSettingByBuilding/' + buildingID, {});
  }
  getMachineByBuilding(buildingID) {
    return this.http.get(this.baseUrl + 'Setting/GetMachineByBuilding/' + buildingID, {});
  }
  deleteSetting(buildingID) {
    return this.http.delete(this.baseUrl + 'Setting/DeleteSetting/' + buildingID, {});
  }
  deleteMachine(id) {
    return this.http.delete(this.baseUrl + 'Setting/DeleteMachine/' + id, {});
  }
  AddStir(entity) {
    return this.http.post(this.baseUrl + 'Setting/Create', entity);
  }
  addSetting(entity) {
    return this.http.post(this.baseUrl + 'Setting/CreateSetting', entity);
  }

  addMachine(entity) {
    return this.http.post(this.baseUrl + 'Setting/CreateMachine', entity);
  }
  updateStir(entity) {
    return this.http.put(this.baseUrl + 'Setting/Update', entity);
  }
  updateSetting(entity) {
    return this.http.put(this.baseUrl + 'Setting/UpdateSetting', entity);
  }

  updateMachine(entity) {
    return this.http.put(this.baseUrl + 'Setting/UpdateMachine', entity);
  }
}
