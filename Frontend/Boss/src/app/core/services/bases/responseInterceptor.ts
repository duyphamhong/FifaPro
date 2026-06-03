import { Injectable } from '@angular/core'
import { HttpInterceptor, HttpHandler, HttpRequest, HttpEvent, HttpResponse, HttpErrorResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap, finalize, catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { LocalStorageService } from './local-storage.service';
import { LoadingIndicatorService } from './loading-indicator.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable()
export class ResponseInterceptor implements HttpInterceptor {

  constructor(private _router: Router, private storeageService: LocalStorageService,
    private loadingService: LoadingIndicatorService, private toastr: MatSnackBar) { }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    //const started = Date.now();
    //let ok: string;
    return (next.handle(req)
      .pipe(
        tap(data => {

        }
        ),
        catchError(err => {
          console.log(err);
          if (err instanceof HttpErrorResponse) {
            this.loadingService.display(false);
            if (err.status === 401) {
              this.storeageService.clearStoreage();
              this._router.navigate(['/login']);
            } else if (err.status === 403) {
              this._router.navigate(['/forbidden']);
              this.toastr.open('Cannot access to this API!', 'OK', {
                duration: 3000
              });
            } else if (err.status === 423) {
              this.toastr.open(err.message, 'OK', {
                duration: 3000
              });
            }
          }
          return new Observable<any>(err);
        }),
        // Log when response observable either completes or errors
        finalize(() => {
          this.loadingService.display(false);
          //const elapsed = Date.now() - started;
          //const msg = `${req.method} "${req.urlWithParams}"
          //   ${ok} in ${elapsed} ms.`;
          //alert(msg);
        })
      )) as any;
  }
}
