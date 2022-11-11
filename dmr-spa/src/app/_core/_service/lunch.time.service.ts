import { Injectable } from '@angular/core';
import { HttpHeaders, HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
@Injectable({
  providedIn: 'root'
})
export class LunchTimeService {
  baseUrl = environment.apiUrlEC;
  ModalNameSource = new BehaviorSubject<number>(0);
  currentModalName = this.ModalNameSource.asObservable();
  constructor(
    private http: HttpClient
  ) { }

  getAllLunchTime() {
    return this.http.get(this.baseUrl + 'LunchTime/GetAll', {});
  }

  create(model) {
    return this.http.post(this.baseUrl + 'LunchTime/Create', model);
  }
  update(model) {
    return this.http.put(this.baseUrl + 'LunchTime/Update', model);
  }
  delete(id: number) {
    return this.http.delete(this.baseUrl + 'LunchTime/Delete/' + id);
  }
}
