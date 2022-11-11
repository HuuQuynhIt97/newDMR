import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { HierarchyNode, IGlueType } from '../_model/glue-type';
const httpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    Authorization: 'Bearer ' + localStorage.getItem('token')
  })
};
@Injectable({
  providedIn: 'root'
})
export class GlueTypeService {
  baseUrl = environment.apiUrlEC;
  messageSource = new BehaviorSubject<number>(0);
  currentMessage = this.messageSource.asObservable();
  // method này để change source message
  changeMessage(message) {
    this.messageSource.next(message);
  }
  constructor(private http: HttpClient) { }
  delete(id) { return this.http.delete(`${this.baseUrl}GlueType/Delete/${id}`); }
  update(model: IGlueType) { return this.http.put(`${this.baseUrl}GlueType/Update`, model); }

  getGlueTypesAsTreeView() {
    return this.http.get<HierarchyNode<IGlueType>[]>(`${this.baseUrl}GlueType/GetAllAsTreeView`);
  }
  getAll() {
    return this.http.get<IGlueType[]>(`${this.baseUrl}GlueType/GetAll`);
  }
  getAllByParentID() {
    return this.http.get<IGlueType[]>(`${this.baseUrl}GlueType/GetAllByParentID`);
  }
  getAllByLevel() {
    return this.http.get<IGlueType[]>(`${this.baseUrl}GlueType/GetAllByLevel`);
  }
  createParent(model: IGlueType) { return this.http.post(`${this.baseUrl}GlueType/createParent`, model); }
  createChild(model: IGlueType) { return this.http.post(`${this.baseUrl}GlueType/createChild`, model); }
}
