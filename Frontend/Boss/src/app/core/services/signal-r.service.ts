import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr"; 
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {

  public data: any[];
  private hubConnection: signalR.HubConnection;
  private baseUrl : string = environment.baseApiUrl;

  constructor() { 
    this.data = [];
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.baseUrl + '/chat')
      .build();
  }

  public startConnection = () => {
    this.hubConnection
      .start()
      .then(() => console.log('Connection started'))
      .catch(err => console.log('Error while starting connection: ' + err))
  }

  public addTransferChartDataListener = () => {
    this.hubConnection.on('transferchatdata', (data) => {
      this.data = data;
    });
  }

  public broadcastChatData = () => {
    this.hubConnection.invoke('broadcastchatdata', this.data)
    .catch(err => console.error(err));
  }
  public addBroadcastChartDataListener = () => {
    this.hubConnection.on('broadcastchatdata', (data) => {
      this.data = data;
    })
  }
}
