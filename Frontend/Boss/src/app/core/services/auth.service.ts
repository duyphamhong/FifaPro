import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Api } from 'src/app/shared/constants/constant';
import { environment } from 'src/environments/environment';
import { ServiceInvokerService } from './bases/service-invoker.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private baseAuthenUrl: string;

  constructor(private serviceInvoker: ServiceInvokerService,
    private http: HttpClient) {
    this.baseAuthenUrl = environment.baseAuthenUrl;
  }

  login(user: any): Observable<any> {
    return this.http.post(this.baseAuthenUrl + Api.LOGIN, user);
  }

  register(user: any): Observable<any> {
    return this.http.post(this.baseAuthenUrl + Api.REGISTER, user);
  }

  changePass(data: any): Observable<any> {
    return this.http.post(this.baseAuthenUrl + Api.CHANGE_PASS, data);
  }
}
