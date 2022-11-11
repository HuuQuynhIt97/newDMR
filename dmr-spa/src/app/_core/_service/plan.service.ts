import { Consumtion, DispatchParams, IDispatch, Plan } from './../_model/plan';
import { Injectable } from '@angular/core';
import { HttpHeaders, HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ModalName } from '../_model/modal-name';
import { Line } from '../_model/line';
import { IGlue } from '../_model/glue';
import { AlertifyService } from './alertify.service';
import { ResponseDetail } from '../_model/responseDetail';
@Injectable({
  providedIn: 'root'
})
export class PlanService {

  baseUrl = environment.apiUrlEC;
  ModalPlanSource = new BehaviorSubject<number>(0);
  currentModalPlan = this.ModalPlanSource.asObservable();
  constructor(
    private http: HttpClient
  ) { }
  TroubleShootingSearch(value: string , batchValue: string) {
    return this.http.get(this.baseUrl + `Plan/TroubleShootingSearch/${value}/${batchValue}` );
  }
  GetBatchByIngredientID(id: number) {
    return this.http.get<Plan[]>(this.baseUrl + 'Plan/GetBatchByIngredientID/' + id, {});
  }
  getBPFCByGlue(glue) {
    return this.http.post(`${this.baseUrl}Plan/GetBPFCByGlue/`, {glue});
  }
  getAll() {
    return this.http.get<Plan[]>(this.baseUrl + 'Plan/GetAll', {});
  }
  search(lineId, min, max) {
    return this.http.get<Plan[]>(`${this.baseUrl}Plan/Search/${lineId}/${min}/${max}`, {});
  }
  getAllPlanByDefaultRange() {
    return this.http.get<Plan[]>(this.baseUrl + 'Plan/GetAllPlanByDefaultRange', {});
  }
  getAllModalName() {
    return this.http.get<ModalName[]>(this.baseUrl + 'ModelName/GetAll', {});
  }
  getAllLine() {
    return this.http.get<Line[]>(this.baseUrl + 'Line/GetAll', {});
  }
  getAllGlue() {
    return this.http.get<IGlue[]>(this.baseUrl + 'Glue/GetAll', {});
  }

  create(modal: Plan) {
    return this.http.post(this.baseUrl + 'Plan/Create', modal);
  }
  update(modal: Plan) {
    return this.http.put(this.baseUrl + 'Plan/Update', modal);
  }
  delete(id: number) {
    return this.http.delete(this.baseUrl + 'Plan/Delete/' + id);
  }
  achievementRate(buildingID: number) {
    return this.http.get(this.baseUrl + 'Plan/AchievementRate/' + buildingID);
  }
  deleteRange(plans) {
    return this.http.post(this.baseUrl + 'Plan/DeleteRange', plans);
  }
  getLines(buildingID) {
    return this.http.get<Plan[]>(this.baseUrl + 'Plan/getLines/' + buildingID, {});
  }
  getModelNames() {
    return this.http.get<Plan[]>(this.baseUrl + 'Plan/GetModelNames', {});
  }
  getGlueByBuilding(buildingID) {
    return this.http.get(this.baseUrl + 'Plan/GetGlueByBuilding/' + buildingID);
  }
  getGlueByBuildingModelName(buildingID, modelname) {
    return this.http.get(`${this.baseUrl}Plan/getGlueByBuildingModelName/${buildingID}/${modelname}`);
  }
  summary(buildingID) {
    return this.http.get(this.baseUrl + 'Plan/summary/' + buildingID);
  }
  consumptionByLineCase1(buildingID: number, startDate: Date, endDate: Date) {
    const params = { buildingID, startDate, endDate};
    return this.http.post<Consumtion[]>(this.baseUrl + 'Plan/ConsumptionByLineCase1', params);
  }
  consumptionByLineCase2(buildingID: number, startDate: Date, endDate: Date) {
    const params = { buildingID, startDate, endDate };
    return this.http.post<Consumtion[]>(this.baseUrl + 'Plan/ConsumptionByLineCase2', params );
  }
  reportConsumptionCase1(buildingID: number, startDate: Date, endDate: Date) {
    const params = { buildingID, startDate, endDate };
    return this.http.post(this.baseUrl + 'Plan/ReportConsumptionCase1', params, { responseType: 'blob' });
  }
  reportConsumptionCase2(buildingID: number, startDate: Date, endDate: Date) {
    const params = { buildingID, startDate, endDate };
    return this.http.post(this.baseUrl + 'Plan/ReportConsumptionCase2', params, { responseType: 'blob' });
  }
  dispatchGlue(obj) {
    return this.http.post(this.baseUrl + 'Plan/DispatchGlue', obj);
  }
  clonePlan(obj) {
    return this.http.post(this.baseUrl + 'Plan/ClonePlan', obj);
  }
  editDelivered(id, qty) {
    return this.http.get(`${this.baseUrl}Plan/EditDelivered/${id}/${qty}`, {});
  }
  editQuantity(id, qty) {
    return this.http.get(`${this.baseUrl}Plan/EditQuantity/${id}/${qty}`, {});
  }
  deleteDelivered(id) {
    return this.http.delete(`${this.baseUrl}Plan/DeleteDelivered/${id}`, {});
  }
  getReport(obj: { startDate: Date, endDate: Date}) {
    return this.http.post(`${this.baseUrl}Plan/GetReport`, obj, { responseType: 'blob' });
  }
  getReportByBuilding(obj: { startDate: Date, endDate: Date , buildingID: number}) {
    return this.http.post(`${this.baseUrl}Plan/GetReportByBuilding`, obj, { responseType: 'blob' });
  }
  getNewReportByBuilding(obj: { startDate: Date, endDate: Date, buildingID: number }) {
    return this.http.post(`${this.baseUrl}Plan/getNewReportByBuilding`, obj, { responseType: 'blob' });
  }
  report(url) {
    return this.http.get(url, {
      responseType: 'arraybuffer'
    });
  }
  print(obj: DispatchParams) {
    return this.http.post(this.baseUrl + 'Plan/Print', obj);
  }
  finish(mixingID: number) {
    return this.http.put(this.baseUrl + 'Plan/Finish/' + mixingID, {});
  }
  getStartTimeFromPeriod(buildingID) {
    return this.http.get<ResponseDetail<any>>(this.baseUrl + 'Plan/GetStartTimeFromPeriod/' + buildingID);
  }
  changeBPFC(planID: number, bpfcID: number) {
    return this.http.get(`${this.baseUrl}Plan/ChangeBPFC/${planID}/${bpfcID}` );
  }
  online(planID: number) { // v102
    return this.http.get(`${this.baseUrl}Plan/Online/${planID}`);
  }
  offline(planID: number) { // v102
    return this.http.get(`${this.baseUrl}Plan/Offline/${planID}`);
  }
  exportExcel(obj: { plans: number[], buildingID: number }) {
    return this.http.post(`${this.baseUrl}Plan/ExportExcel`, obj, { responseType: 'blob' });
  }
  exportExcelWorkPlanWholeBuilding(buildingID: number, startDate: string, endDate: string) {
    return this.http.get(`${this.baseUrl}Plan/ExportExcelWorkPlanWholeBuilding/${buildingID}/${startDate}/${endDate}`, { responseType: 'blob' });
  }
}

