import { Injectable } from '@angular/core';
import { HttpHeaders, HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { IDispatch, IDispatchForCreate } from '../_model/plan';

@Injectable({
  providedIn: 'root'
})
export class DispatchService {

  baseUrl = environment.apiUrlEC;
  ModalDispatchSource = new BehaviorSubject<number>(0);
  currentModalDispatch = this.ModalDispatchSource.asObservable();
  constructor(
    private http: HttpClient
  ) { }

  add(obj: IDispatchForCreate) {
    return this.http.post(this.baseUrl + 'Dispatch/Add', obj);
  }
  update(obj: IDispatchForCreate) {
    return this.http.post(this.baseUrl + 'Dispatch/Update', obj);
  }
  updateAmount(id: number, amount: number) {
    return this.http.put(this.baseUrl + `Dispatch/updateAmount/${id}/${amount}`, {});
  }
  addDispatch(obj: IDispatchForCreate[]) {
    return this.http.post(this.baseUrl + 'Dispatch/Dispatch', obj);
  }
  updateStartDispatchingTime(id: number) {
    return this.http.put(this.baseUrl + `Dispatch/UpdateStartDispatchingTime/${id}`, {});
  }
}

