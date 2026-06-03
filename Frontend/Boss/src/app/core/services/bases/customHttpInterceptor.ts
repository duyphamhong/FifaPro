import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpHeaders,
  HttpErrorResponse,
  HttpResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, finalize, map } from 'rxjs/operators';
import { LocalStorageService } from './local-storage.service';
import { LoadingIndicatorService } from './loading-indicator.service';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Injectable()
export class CustomHttpInterceptor implements HttpInterceptor {

  constructor(public storeageService: LocalStorageService,
    private loadingService: LoadingIndicatorService,
    private _router: Router,
    private toastr: ToastrService) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    this.loadingService.display(true);
    request = request.clone({
      headers: new HttpHeaders({
        //'Content-Type': 'application/json; charset=utf-8',
        'Access-Control-Allow-Origin': '*',
        'Authorization': `Bearer ${this.storeageService.getAccessToken()}`
      })
    });
    return next.handle(request).pipe(
      map((event: HttpEvent<any>) => {
        return event;
      }),
      catchError((err: HttpErrorResponse) => {
        if (err instanceof HttpErrorResponse) {
          if (err.status === 401) {
            this.storeageService.clearStoreage();
            this._router.navigate(['/login']);
            this.toastr.error('', 'Something went wrong',);
          } else if (err.status === 403) {
            this._router.navigate(['/forbidden']);
            this.toastr.error(err.message, 'Error!');
          } else if (err.status === 423) {
            this.toastr.error(err.error.message, 'Something went wrong',);
          } else {
            this.toastr.error(err.error.message, 'Something went wrong',);
          }
        }
        return throwError(err);
      })) as any;
  }
}

