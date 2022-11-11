import { Injectable } from '@angular/core';
import { HttpHeaders, HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { IMailing } from '../_model/mailing';
@Injectable({
  providedIn: 'root'
})
export class MailingService {
  baseUrl = environment.apiUrlEC;
  mailingSource = new BehaviorSubject<number>(0);
  currentModalName = this.mailingSource.asObservable();
  constructor(
    private http: HttpClient
  ) { }

  getAllMailing() {
    return this.http.get<IMailing[]>(this.baseUrl + 'Mailing/GetAll', {});
  }

  create(model: IMailing) {
    return this.http.post(this.baseUrl + 'Mailing/Create', model);
  }
  createRange(model) {
    return this.http.post(this.baseUrl + 'Mailing/createRange', model);
  }
  updateRange(model) {
    return this.http.put(this.baseUrl + 'Mailing/updateRange', model);
  }
  update(model: IMailing) {
    return this.http.put(this.baseUrl + 'Mailing/Update', model);
  }
  delete(id: number) {
    return this.http.delete(this.baseUrl + 'Mailing/Delete/' + id);
  }
  deleteRange(obj) {
    return this.http.post(this.baseUrl + 'Mailing/DeleteRange/', obj);
  }
}
