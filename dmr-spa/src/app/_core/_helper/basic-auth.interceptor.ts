import { Injectable } from '@angular/core';
import { HttpRequest, HttpErrorResponse, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { AlertifyService } from '../_service/alertify.service';
import { AuthService } from '../_service/auth.service';

@Injectable()
export class BasicAuthInterceptor implements HttpInterceptor {
    constructor(private alertify: AlertifyService, private authSerive: AuthService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // add authorization header with basic auth credentials if available
        if (localStorage.getItem('token')) {
            request = request.clone({
                setHeaders: {
                    Authorization: `Bearer ${localStorage.getItem('token')}`
                }
            });
        }
        return next.handle(request);
        // return next.handle(request).pipe(
        //     retry(1),
        //     catchError((error: HttpErrorResponse) => {
        //         let errorMessage = '';
        //         if (error.error instanceof ErrorEvent) {
        //             // client-side error
        //             errorMessage = `Error: ${error.error.message}`;
        //         } else {
        //             // server-side error
        //             errorMessage = `Error Status: ${error.status}\nMessage: ${error.message}`;
        //             switch (error.status) {
        //                 case 0: case 500: case 403: case 401:
        //                 this.authSerive.logOut();
        //                 this.alertify.error(`Lỗi máy chủ vui lòng tải lại trang (nhấn F5) và chờ trong ít phút!
        //                 <br> Server Error! Please refresh page (press F5) and wait in a few minutes`, true);
        //                 return throwError(errorMessage);
        //                 case 400:
        //                     return throwError(errorMessage);
        //             }
        //         }
        //         return throwError(errorMessage);
        //     })
        // );
    }
}
