import { Injectable } from '@angular/core';
import { PaginatedResult } from '../_model/pagination';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { IDispatchListDetail, IDispatchListForUpdate, IToDoList, IToDoListForCancel, IToDoListForReturn } from '../_model/IToDoList';
import { DispatchParams, IDispatch } from '../_model/plan';
import { IMixingDetailForResponse, IMixingInfo } from '../_model/IMixingInfo';
import { IGenerateSubpackageParams, IScanParams } from '../_model/scan-params';
import { IAddDispatchParams, IDispatchParams, IUpdateDispatchParams } from '../_model/dispatch';

const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    Authorization: `Bearer ${localStorage.getItem('token')}`
  })
};
@Injectable({
  providedIn: 'root'
})
export class BottomFactoryService {
  baseUrl = environment.apiUrlEC;
  data = new BehaviorSubject<boolean>(null);
  constructor(private http: HttpClient) {
   }
  setValue(message): void {
    this.data.next(message);
  }
  getValue(): Observable<boolean> {
    return this.data.asObservable();
  }

  cancel(todo: IToDoListForCancel) {
    return this.http.post(`${this.baseUrl}BottomFactory/Cancel`, todo);
  }
  cancelRange(todo: IToDoListForCancel[]) {
    return this.http.post(`${this.baseUrl}BottomFactory/cancelRange`, todo);
  }
  EVAUVList(building: number) {
    return this.http.get<IToDoListForReturn>(this.baseUrl + 'BottomFactory/EVAUVList/' + building, {});
  }
  done(building: number) {
    return this.http.get<IToDoListForReturn>(this.baseUrl + 'BottomFactory/Done/' + building, {});
  }
  todo(building: number) {
    return this.http.get<IToDoListForReturn>(this.baseUrl + 'BottomFactory/ToDo/' + building, {});
  }
  todoAddition(building: number) {
    return this.http.get<IToDoListForReturn>(this.baseUrl + 'BottomFactory/ToDoAddition/' + building, {});
  }
  dispatchAddition(building: number) {
    return this.http.get<IToDoListForReturn>(this.baseUrl + 'BottomFactory/dispatchAddition/' + building, {});
  }
  dispatchList(building: number) {
    return this.http.get(this.baseUrl + 'BottomFactory/DispatchList/' + building, {});
  }
  delay(building: number) {
    return this.http.get<IToDoListForReturn>(this.baseUrl + 'BottomFactory/Delay/' + building, {});
  }
  printGlue(mixingInfoID: number) {
    return this.http.get(this.baseUrl + 'BottomFactory/printGlue/' + mixingInfoID, {});
  }
  findPrintGlue(mixingInfoID: number) {
    return this.http.get<IMixingInfo>(this.baseUrl + 'BottomFactory/FindPrintGlue/' + mixingInfoID, {});
  }
  dispatch(obj: DispatchParams) {
    return this.http.post<IDispatch[]>(this.baseUrl + 'BottomFactory/Dispatch', obj);
  }
  updateStartStirTimeByMixingInfoID(building: number) {
    return this.http.put<IToDoList[]>(this.baseUrl + 'BottomFactory/updateStartStirTimeByMixingInfoID/' + building, {});
  }
  updateFinishStirTimeByMixingInfoID(building: number) {
    return this.http.put<IToDoList[]>(this.baseUrl + 'BottomFactory/updateFinishStirTimeByMixingInfoID/' + building, {});
  }
  generateToDoList(plans: number[]) {
    return this.http.post<IToDoList[]>(this.baseUrl + 'BottomFactory/GenerateToDoList', plans);
  }
  generateDispatchList(plans: number[]) {
    return this.http.post<IToDoList[]>(this.baseUrl + 'BottomFactory/GenerateDispatchList', plans);
  }

  exportExcel(buildingID: number) {
    return this.http.get(`${this.baseUrl}BottomFactory/ExportExcel/${buildingID}`, { responseType: 'blob' });
  }
  exportExcel2(buildingID: number) {
    return this.http.get(`${this.baseUrl}BottomFactory/GetNewReport/${buildingID}`, { responseType: 'blob' });
  }
  getMixingDetail(glueName: string) {
    return this.http.post<IMixingDetailForResponse>(this.baseUrl + 'BottomFactory/GetMixingDetail', { glueName });
  }
  // Đã chỉnh sửa ngày 1/30/2021 11:27
  finishDispatch(obj) {
    return this.http.put(`${this.baseUrl}BottomFactory/FinishDispatch`, obj);
  }
  getDispatchDetail(building, glueNameID, estimatedStartTime, estimatedFinishTime) {
    return this.http.get<any[]>(`${this.baseUrl}BottomFactory/GetDispatchDetail/${building}/${glueNameID}/${estimatedStartTime}/${estimatedFinishTime}`);
  }
  getMixingInfoHistory(building, glueName, estimatedStartTime, estimatedFinishTime) {
    return this.http.get<IMixingInfo[]>(`${this.baseUrl}BottomFactory/GetMixingInfoHistory/${building}/${glueName}/${estimatedStartTime}/${estimatedFinishTime}`);
  }
  updateDispatchDetail(obj: any[]) {
    return this.http.put(this.baseUrl + 'BottomFactory/UpdateDispatchDetail', obj);
  }
  getDispatchListDetail(glueNameID, estimatedStartTime, estimatedFinishTime) {
    return this.http.get(`${this.baseUrl}BottomFactory/GetDispatchListDetail/${glueNameID}/${estimatedStartTime}/${estimatedFinishTime}`);
  }
  printGlueDispatchList(mixingInfoID: number, glueNameID: number, estimatedStartTime, estimatedFinishTime) {
    return this.http.get(`${this.baseUrl}BottomFactory/printGlueDispatchList/${mixingInfoID}/${glueNameID}/${estimatedStartTime}/${estimatedFinishTime}`, {});
  }
  updateMixingInfoDispatchList(mixingInfoID: number, glueNameID: number, estimatedStartTime, estimatedFinishTime) {
    return this.http.get(`${this.baseUrl}BottomFactory/UpdateMixingInfoDispatchList/${mixingInfoID}/${glueNameID}/${estimatedStartTime}/${estimatedFinishTime}`, {});
  }
  addOvertime(plans: number[]) {
    return this.http.post(this.baseUrl + 'BottomFactory/AddOvertime', plans);
  }

  removeOvertime(plans: number[]) {
    return this.http.post(this.baseUrl + 'BottomFactory/RemoveOvertime', plans);
  }

  dispatchListDelay(building: number) {
    return this.http.get(this.baseUrl + 'BottomFactory/DispatchListDelay/' + building, {});
  }
  scanQRCode(obj: IScanParams) {
    return this.http.post(this.baseUrl + 'BottomFactory/scanQRCode/', obj);
  }
  generateScanByNumber(obj: IGenerateSubpackageParams) {
    return this.http.post(this.baseUrl + 'BottomFactory/GenerateScanByNumber/', obj);
  }
  print(subpackages: number[]) {
    return this.http.post(this.baseUrl + 'BottomFactory/Print', { subpackages} );
  }

  saveSubpackage(model) {
    return this.http.post(this.baseUrl + 'BottomFactory/SaveSubpackage', model);
  }
  addDispatch(obj: IAddDispatchParams) {
    return this.http.post(this.baseUrl + 'BottomFactory/addDispatch', obj);
  }
  updateDispatch(obj: IUpdateDispatchParams) {
    return this.http.put(this.baseUrl + 'BottomFactory/UpdateDispatch', obj);
  }
  getAllDispatch(obj: IDispatchParams) {
    const params = new HttpParams()
      .set('buildingID', obj.buildingID + '')
      .set('mixingInfoID', obj.mixingInfoID + '')
      .set('glueNameID', obj.glueNameID + '');
    return this.http.get(this.baseUrl + 'BottomFactory/GetAllDispatch', { params });
  }

  getMixingInfo(building: number) {
    return this.http.get(this.baseUrl + 'BottomFactory/GetMixingInfo/' + building, {});
  }
  getSubpackageCapacity() {
    return this.http.get(this.baseUrl + 'BottomFactory/GetSubpackageCapacity' , {});
  }
  getSubpackageLatestSequence(batch: string, glueNameID: number, buildingID: number) {
    return this.http.get(`${this.baseUrl}BottomFactory/GetSubpackageLatestSequence/${batch}/${glueNameID}/${buildingID}`, {});
  }
}
