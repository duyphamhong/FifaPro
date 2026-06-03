import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Api } from 'src/app/shared/constants/constant';
import { ServiceInvokerService } from './bases/service-invoker.service';

@Injectable({
  providedIn: 'root'
})
export class DataServiceService {

  constructor(private serviceInvoker: ServiceInvokerService) { }

  getNextMatch(data: any): Observable<any> {
    return this.serviceInvoker.get(data, Api.GET_NEXT_MATCH);
  }

  getPlayerPosition(data: any): Observable<any> {
    return this.serviceInvoker.get(data, Api.GET_PLAYER_POSITION);
  }

  predict(data: any): Observable<any> {
    return this.serviceInvoker.post(data, Api.PREDICT_MATCH);
  }

  getPreviousMatch(data: any): Observable<any> {
    return this.serviceInvoker.get(data, Api.GET_PREVIOUS_MATCH);
  }

  getMatches(data: any): Observable<any> {
    return this.serviceInvoker.get(data, Api.GET_MATCHES);
  }

  getUserInfo(data: any): Observable<any> {
    return this.serviceInvoker.get(data, Api.GET_USER_INFO);
  }

  addUserAdditionalInformation(data: any): Observable<any> {
    return this.serviceInvoker.get(data, Api.ADD_USER_ADDITIONAL_INFORMATION);
  }

  updateAvatar(data: any): Observable<any> {
    return this.serviceInvoker.post(data, Api.UPDATE_AVATAR);
  }

  setChampion(data: any): Observable<any> {
    return this.serviceInvoker.post(data, Api.SET_CHAMPION);
  }

  getHistory(data: any): Observable<any> {
    return this.serviceInvoker.get(data, Api.GET_HISTORY);
  }

  getChats(data: any): Observable<any> {
    return this.serviceInvoker.get(data, Api.GET_CHATS);
  }
  sendChats(data: any): Observable<any> {
    return this.serviceInvoker.post(data, Api.SEND_CHAT);
  }
  getTeams(data: any): Observable<any> {
    return this.serviceInvoker.get(data, Api.GET_TEAMS);
  }
  getMatchPredicts(data: any): Observable<any> {
    return this.serviceInvoker.get(data, Api.GET_MATCH_PREDICTS);
  }

  updateStandings(data: any): Observable<any> {
    return this.serviceInvoker.get(data, Api.UPDATE_STANDINGS);
  }

  updateMatches(data: any): Observable<any> {
    return this.serviceInvoker.get(data, Api.UPDATE_MATCHES);
  }

  updatePreviousMatches(data: any): Observable<any> {
    return this.serviceInvoker.get(data, Api.UPDATE_PREVIOUS_MATCHES);
  }
}
