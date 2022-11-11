import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { IStation  } from '../_model/station';
import { PaginatedResult } from '../_model/pagination';
@Injectable({
  providedIn: 'root'
})
export class StationService {
  baseUrl = environment.apiUrlEC;
  messageSource = new BehaviorSubject<number>(0);
  info = new BehaviorSubject<boolean>(null);
  currentMessage = this.messageSource.asObservable();
  // method này để change source message
  changeMessage(message) {
    this.messageSource.next(message);
  }
  constructor(private http: HttpClient) { }

  setValue(message): void {
    this.info.next(message);
  }
  getValue(): Observable<boolean> {
    return this.info.asObservable();
  }
  getAll() {
    return this.http
      .get<IStation[]>(`${this.baseUrl}Station/GetAll`);
  }
  getAllByPlanID(planID: number) {
    return this.http
      .get<IStation[]>(`${this.baseUrl}Station/GetAllByPlanID/${planID}`);
  }
  create(model: IStation) {
    return this.http.post(`${this.baseUrl}Station/create`, model );
  }
  createRange(model: IStation[]) {
    return this.http.post(`${this.baseUrl}Station/AddRange`, model);
  }
  update(model: IStation) {
    return this.http.put(`${this.baseUrl}Station/update`, model);
  }
  updateRange(model: IStation[]) {
    return this.http.post(`${this.baseUrl}Station/updateRange`, model);
  }
  delete(stationID: number) {
    return this.http.delete(`${this.baseUrl}Station/Delete/${stationID}`, {});
  }
}
