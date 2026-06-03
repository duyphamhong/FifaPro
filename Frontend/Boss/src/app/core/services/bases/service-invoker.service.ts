import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Inject, Injectable, Injector } from '@angular/core';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { Observable, throwError } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { LocalStorageService } from './local-storage.service';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})
export class ServiceInvokerService {

  private baseUrl: string;

  constructor(private http: HttpClient,
    private storeageService: LocalStorageService) {
    this.baseUrl = environment.baseApiUrl;
  }

  /* Get array */
  get(element: any, url: string): Observable<any> {
    const httpPackage = {
      params: element
    };

    return this.http.get(this.baseUrl + url, httpPackage);
  }

  /* Put */
  put<T>(element: T, url: string): Observable<any> {
    return this.http.put<T>(this.baseUrl + url, element);
  }

  /* Post */
  post(element: any, url: string): Observable<any> {
    if (element == undefined || element == '') {
      element = {};
    }
    return this.http.post(this.baseUrl + url, element);
  }

  /*Delete*/
  delete<T>(url: string): Observable<any> {
    //const deleteApi = `${AppSettings.API_ADDRESS}/${id}`;
    return this.http.delete<T>(this.baseUrl + url);
  }
}
