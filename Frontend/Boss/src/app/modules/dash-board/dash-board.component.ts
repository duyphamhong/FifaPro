import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import * as signalR from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { forkJoin } from 'rxjs';
import { LocalStorageService } from 'src/app/core/services/bases/local-storage.service';
import { DataServiceService } from 'src/app/core/services/data-service.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-dash-board',
  templateUrl: './dash-board.component.html',
  styleUrls: ['./dash-board.component.scss']
})
export class DashBoardComponent implements OnInit {
  public nextMatch: any;
  public previousMatch: any;
  public playerPosition: any;
  public chatData: any[];
  public userAvatar: string;

  private hubConnection: signalR.HubConnection;

  predictionNumber = 0;
  teamWin = '';
  chatContent = '';
  baseUrl = '';
  players: any[];

  public form = new FormGroup({
    team1Score: new FormControl(''),
    team2Score: new FormControl(''),
    blammer: new FormControl(''),
  });

  constructor(private dataService: DataServiceService,
    private storage: LocalStorageService,
    private toasrt: ToastrService) {
    this.players = [];
    this.chatData = [];
    this.userAvatar = '';

    //signR
    this.baseUrl = environment.baseApiUrl;
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.baseUrl + '/chat', {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .build();

    this.hubConnection.start()
      .then(() => console.log('Connection started'))
      .catch(err => this.toasrt.error('Error while starting connection: ' + err));

    this.hubConnection.on('broadcastchatdata', (data) => {
      this.playAudio();
      this.toasrt.info('vừa chém: ' + data.content, data.userName, { positionClass: 'toast-top-right' });
      this.chatData.push(data);
    })
  }

  ngOnInit(): void {
    //Called after the constructor, initializing input properties, and the first call to ngOnChanges.
    //Add 'implements OnInit' to the class.
    this.loadInformation();
  }

  loadInformation() {
    forkJoin({
      requestOne: this.dataService.getPreviousMatch({}),
      requestTwo: this.dataService.getNextMatch({}),
      requestThree: this.dataService.getPlayerPosition({})

    })
      .subscribe(({ requestOne, requestTwo, requestThree }) => {
        this.previousMatch = requestOne.result;
        this.nextMatch = requestTwo.result;
        this.playerPosition = requestThree.result;
        this.players = this.playerPosition.top3HighestUsers;
        this.userAvatar = this.players.find(x => x.name == this.storage.getUserName().trim()).avatarUrl;

        this.dataService.getChats({ matchId: this.nextMatch.id }).subscribe(response => {
          this.chatData = response.result;
        });
      });
  }

  //#region Prediction area
  predictClick(pos: number) {
    this.predictionNumber = pos;
    if (pos === 1) {
      this.teamWin = this.nextMatch.team1;
    } else if (pos === 2) {
      this.teamWin = this.nextMatch.team2;
    } else {
      this.teamWin = '';
    }
  }

  onPredict() {
    if (this.form.get('team1Score')?.value === "" || this.form.get('team2Score')?.value === "") {
      this.toasrt.error('?????');
      return;
    }

    if (this.form.get('team1Score')?.value !== this.form.get('team2Score')?.value && this.predictionNumber == 3) {
      this.toasrt.error('Hòa mà tỉ số khác nhau, ngáo à???');
      return;
    }

    if (this.form.get('blammer')?.value === "") {
      this.toasrt.error('Không blame gì à?');
      return;
    }

    let request =
    {
      UserName: this.storage.getUserName(),
      MatchId: this.nextMatch.id,
      TeamWinCode: this.teamWin,
      Team1Score: this.form.get('team1Score')?.value,
      Team2Score: this.form.get('team2Score')?.value,
      Blammer: this.form.get('blammer')?.value
    }

    this.dataService.predict(request).subscribe(response => {
      this.toasrt.success(response.message);
    });
  }
  //#endregion

  //#region Position rank
  loadRankingInfo() {
    this.dataService.getPlayerPosition({}).subscribe(response => {
      this.playerPosition = response.result;
    });
  }
  //#endregion

  loadPreviousMatch() {
    this.dataService.getPreviousMatch({}).subscribe(response => {
      this.previousMatch = response.result;
    });
  }

  postChat() {
    if (this.chatContent == '') {
      this.toasrt.error('???', 'Content????????');
      return;
    }

    var chatModel = {
      matchId: this.nextMatch.id,
      userName: this.storage.getUserName().trim(),
      avatar: this.players.find(x => x.name == this.storage.getUserName().trim()).avatarUrl,
      content: this.chatContent
    };

    this.dataService.sendChats(chatModel).subscribe(result => {
    });
  }

  playAudio(){
    let audio = new Audio();
    audio.src = "../../../assets/audio/alert.wav";
    audio.autoplay = true;
    audio.load();
    audio.play();
  }
}
