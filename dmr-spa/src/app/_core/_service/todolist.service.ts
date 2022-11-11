import { Injectable } from '@angular/core';
import { PaginatedResult } from '../_model/pagination';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { IDispatchListDetail, IDispatchListForUpdate, IToDoList, IToDoListForCancel, IToDoListForReturn } from '../_model/IToDoList';
import { DispatchParams, IDispatch } from '../_model/plan';
import { IMixingDetailForResponse, IMixingInfo } from '../_model/IMixingInfo';

import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    Authorization: `Bearer ${localStorage.getItem('token')}`
  })
};
@Injectable({
  providedIn: 'root'
})
export class TodolistService {
  baseUrl = environment.apiUrlEC;
  data = new BehaviorSubject<boolean>(null);
  constructor(private http: HttpClient) {
   }
  // Thêm bởi Quỳnh (Leo 1/28/2021 11:46)
  addition(glueNameID, mixingID, start, end) {
    return this.http.get(`${this.baseUrl}ToDoList/addition/${glueNameID}/${mixingID}/${start}/${end}`);
  }

  additionDispatch(glueNameID) {
    return this.http.get(`${this.baseUrl}ToDoList/additionDispatch/${glueNameID}`);
  }
  setValue(message): void {
    this.data.next(message);
  }
  getValue(): Observable<boolean> {
    return this.data.asObservable();
  }

  cancel(todo: IToDoListForCancel) {
    return this.http.post(`${this.baseUrl}ToDoList/Cancel`, todo);
  }
  cancelRange(todo: IToDoListForCancel[]) {
    return this.http.post(`${this.baseUrl}ToDoList/cancelRange`, todo);
  }
  done(building: number) {
    return this.http.get<IToDoListForReturn>(this.baseUrl + 'ToDoList/Done/' + building, {});
  }
  todo(building: number) {
    return this.http.get<IToDoListForReturn>(this.baseUrl + 'ToDoList/ToDo/' + building, {});
  }
  todoAddition(building: number) {
    return this.http.get<IToDoListForReturn>(this.baseUrl + 'ToDoList/ToDoAddition/' + building, {});
  }
  dispatchAddition(building: number) {
    return this.http.get<IToDoListForReturn>(this.baseUrl + 'ToDoList/dispatchAddition/' + building, {});
  }
  dispatchList(building: number) {
    return this.http.get(this.baseUrl + 'ToDoList/DispatchList/' + building, {});
  }
  delay(building: number) {
    return this.http.get<IToDoListForReturn>(this.baseUrl + 'ToDoList/Delay/' + building, {});
  }
  printGlue(mixingInfoID: number) {
    return this.http.get(this.baseUrl + 'ToDoList/printGlue/' + mixingInfoID, {});
  }
  findPrintGlue(mixingInfoID: number) {
    return this.http.get<IMixingInfo>(this.baseUrl + 'ToDoList/FindPrintGlue/' + mixingInfoID, {});
  }
  dispatch(obj: DispatchParams) {
    return this.http.post<IDispatch[]>(this.baseUrl + 'ToDoList/Dispatch', obj);
  }
  updateStartStirTimeByMixingInfoID(building: number) {
    return this.http.put<IToDoList[]>(this.baseUrl + 'ToDoList/updateStartStirTimeByMixingInfoID/' + building, {});
  }
  updateFinishStirTimeByMixingInfoID(building: number) {
    return this.http.put<IToDoList[]>(this.baseUrl + 'ToDoList/updateFinishStirTimeByMixingInfoID/' + building, {});
  }
  generateToDoList(plans: number[]) {
    return this.http.post<IToDoList[]>(this.baseUrl + 'ToDoList/GenerateToDoList', plans);
  }
  generateDispatchList(plans: number[]) {
    return this.http.post<IToDoList[]>(this.baseUrl + 'ToDoList/GenerateDispatchList', plans);
  }

  exportExcel(buildingID: number) {
    return this.http.get(`${this.baseUrl}ToDoList/ExportExcel/${buildingID}`, { responseType: 'blob' });
  }
  exportExcel2(buildingID: number) {
    return this.http.get(`${this.baseUrl}ToDoList/GetNewReport/${buildingID}`, { responseType: 'blob' });
  }
  getMixingDetail(glueName: string) {
    return this.http.post<IMixingDetailForResponse>(this.baseUrl + 'ToDoList/GetMixingDetail', { glueName });
  }
  // Đã chỉnh sửa ngày 1/30/2021 11:27
  finishDispatch(obj) {
    return this.http.put(`${this.baseUrl}ToDoList/FinishDispatch`, obj);
  }
  getDispatchDetail(building, glueNameID, estimatedStartTime, estimatedFinishTime) {
    return this.http.get<any[]>(`${this.baseUrl}ToDoList/GetDispatchDetail/${building}/${glueNameID}/${estimatedStartTime}/${estimatedFinishTime}`);
  }
  getMixingInfoHistory(building, glueName, estimatedStartTime, estimatedFinishTime) {
    return this.http.get<IMixingInfo[]>(`${this.baseUrl}ToDoList/GetMixingInfoHistory/${building}/${glueName}/${estimatedStartTime}/${estimatedFinishTime}`);
  }
  // updateDispatchDetail(obj: IDispatchListForUpdate) {
  //   return this.http.put(this.baseUrl + 'ToDoList/UpdateDispatchDetail', obj);
  // }
  updateDispatchDetail(obj: any[]) {
    return this.http.put(this.baseUrl + 'ToDoList/UpdateDispatchDetail', obj);
  }
  getDispatchListDetail(glueNameID, estimatedStartTime, estimatedFinishTime) {
    return this.http.get(`${this.baseUrl}ToDoList/GetDispatchListDetail/${glueNameID}/${estimatedStartTime}/${estimatedFinishTime}`);
  }
  /// them code moi
  // printGlueDispatchList(mixingInfoID: number, dispatchListID: number) {
  //   return this.http.get(`${this.baseUrl}ToDoList/printGlueDispatchList/${mixingInfoID}/${dispatchListID}`, {});
  // }
  printGlueDispatchList(mixingInfoID: number, glueNameID: number, estimatedStartTime, estimatedFinishTime) {
    return this.http.get(`${this.baseUrl}ToDoList/printGlueDispatchList/${mixingInfoID}/${glueNameID}/${estimatedStartTime}/${estimatedFinishTime}`, {});
  }
  updateMixingInfoDispatchList(mixingInfoID: number, glueNameID: number, estimatedStartTime, estimatedFinishTime) {
    return this.http.get(`${this.baseUrl}ToDoList/UpdateMixingInfoDispatchList/${mixingInfoID}/${glueNameID}/${estimatedStartTime}/${estimatedFinishTime}`, {});
  }
  addOvertime(plans: number[]) {
    return this.http.post(this.baseUrl + 'ToDoList/AddOvertime', plans);
  }

  removeOvertime(plans: number[]) {
    return this.http.post(this.baseUrl + 'ToDoList/RemoveOvertime', plans);
  }

  dispatchListDelay(building: number) {
    return this.http.get(this.baseUrl + 'ToDoList/DispatchListDelay/' + building, {});
  }
  // leo update 11:46 AM 2/2/2021
  MixedHistory(mixingID) {
    return this.http.get(`${this.baseUrl}ToDoList/MixedHistory/${mixingID}`, {});
  }
// leo update 11:46 AM 2/2/2021
}
