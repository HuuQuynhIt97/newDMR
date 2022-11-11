import { Injectable } from '@angular/core';
import { PaginatedResult } from '../_model/pagination';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    Authorization: `Bearer ${localStorage.getItem('token')}`
  })
};
@Injectable({
  providedIn: 'root'
})
export class SubpackageCapacityService {
  baseUrl = environment.apiUrlEC;
  ModalNameSource = new BehaviorSubject<number>(0);
  currentModalName = this.ModalNameSource.asObservable();
  constructor(
    private http: HttpClient
  ) { }

  getAllSubpackageCapacity() {
    return this.http.get(this.baseUrl + 'SubpackageCapacity/GetAll', {});
  }

  create(model) {
    return this.http.post(this.baseUrl + 'SubpackageCapacity/Create', model);
  }
  update(model) {
    return this.http.put(this.baseUrl + 'SubpackageCapacity/Update', model);
  }
  delete(id: number) {
    return this.http.delete(this.baseUrl + 'SubpackageCapacity/Delete/' + id);
  }
}
