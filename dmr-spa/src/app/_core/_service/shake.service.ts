import { Injectable } from '@angular/core';
import { HttpHeaders, HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
@Injectable({
  providedIn: 'root'
})
export class ShakeService {
  baseUrl = environment.apiUrlEC;
  ModalNameSource = new BehaviorSubject<number>(0);
  currentModalName = this.ModalNameSource.asObservable();
  constructor(
    private http: HttpClient
  ) { }

  getAllshake() {
    return this.http.get(this.baseUrl + 'shake/GetAll', {});
  }
  getShakesByMixingInfoID(mixingInfoID: number) {
    return this.http.get(this.baseUrl + 'shake/GetShakesByMixingInfoID/' + mixingInfoID, {});
  }
  create(model) {
    return this.http.post(this.baseUrl + 'shake/Create', model);
  }
  update(model) {
    return this.http.put(this.baseUrl + 'shake/Update', model);
  }
  delete(id: number) {
    return this.http.delete(this.baseUrl + 'shake/Delete/' + id);
  }
  generateShakes(mixingInfoID: number) {
    return this.http.get(this.baseUrl + 'shake/GenerateShakes/' + mixingInfoID, {});
  }
}
