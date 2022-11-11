import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Tutorial } from '../_model/tutorial';
import { IStir, IStirForAdd, IStirForUpdate } from '../_model/stir';
const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    Authorization: 'Bearer ' + localStorage.getItem('token')
  })
};
@Injectable({
  providedIn: 'root'
})
export class StirService {
  baseUrl = environment.apiUrlEC;
  constructor(private http: HttpClient) { }
  getStirInfo(glueName) {
    return this.http.get(`${this.baseUrl}Stir/GetStirInfo/${glueName}`);
  }
  getRPM(stirID) {
    return this.http.get(`${this.baseUrl}Stir/GetRPM/${stirID}`);
  }
  getRPMByMachineCode(machineCode, startTime, endTime) {
    return this.http.get(`${this.baseUrl}Stir/GetRPMByMachineCode/${machineCode}/${startTime}/${endTime}`);
  }

  getStirByMixingInfoID(mixingInfoID: number) {
    return this.http.get<IStir[]>(`${this.baseUrl}Stir/getStirByMixingInfoID/${mixingInfoID}`);
  }
  create(model: IStirForAdd) {
    return this.http.post(`${this.baseUrl}Stir/Create`, model);
  }
  update(model: IStirForUpdate) {
    return this.http.put(`${this.baseUrl}Stir/Update`, model);
  }
  scanMachine(buildingID: number, scanValue: string) {
    return this.http.get(`${this.baseUrl}Stir/scanMachine/${buildingID}/${scanValue}`, {} );
  }
  updateStartScanTime(mixingInfoID: number) {
    return this.http.put(`${this.baseUrl}Stir/UpdateStartScanTime/${mixingInfoID}`, {});
  }
}
