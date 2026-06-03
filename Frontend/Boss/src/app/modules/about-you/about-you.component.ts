import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { forkJoin } from 'rxjs';
import { AuthService } from 'src/app/core/services/auth.service';
import { LocalStorageService } from 'src/app/core/services/bases/local-storage.service';
import { DataServiceService } from 'src/app/core/services/data-service.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-about-you',
  templateUrl: './about-you.component.html',
  styleUrls: ['./about-you.component.scss']
})
export class AboutYouComponent implements OnInit, OnDestroy {
  matches: any[];
  public playerPosition: any;
  nextMatch: any;
  userInfo: any;
  currentUserName: string;
  histories: any[];
  ranking: any[];

  vipRanking: any[];
  freeRanking: any[];

  currentRank: any;
  totalUsers: any;
  rankingPaging: any[];
  paging: number[];
  currentPage: number;

  //password area
  oldPass: string;
  newPass: string;
  confirmPass: string;
  champion: string;
  samePrediction: number;
  teams: any[];
  matchPredictions: any;
  isFreeRankingChoosen: boolean = false;
  isAdmin: boolean = false;

  baseUrl = '';

  isShowPredictPopup: boolean;
  countdown = {
    days: '00',
    hours: '00',
    mins: '00',
    secs: '00'
  };
  private countdownInterval: any;

  constructor(private dataService: DataServiceService,
    private storage: LocalStorageService,
    private authenService: AuthService,
    private route: Router,
    private toasrt: ToastrService) {
    this.matches = [];
    this.currentUserName = this.storage.getUserName();
    this.oldPass = '';
    this.newPass = '';
    this.confirmPass = '';
    this.champion = '';
    this.histories = [];
    this.vipRanking = [];
    this.freeRanking = [];
    this.ranking = [];
    this.rankingPaging = [];
    this.paging = [];
    this.currentPage = 1;
    this.samePrediction = 0;
    this.teams = [];
    this.isShowPredictPopup = false;
    this.matchPredictions = {};
    this.isAdmin = this.storage.hasRole('Admin');

    //signR
    this.baseUrl = environment.baseApiUrl;

  }

  ngOnInit(): void {
    this.loadInfo();
    this.startCountdown();
  }

  ngOnDestroy(): void {
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }
  }

  loadInfo() {
    forkJoin({
      requestOne: this.dataService.getMatches({}),
      requestTwo: this.dataService.getPlayerPosition({}),
      requestThree: this.dataService.getNextMatch({}),
      requestFour: this.dataService.getUserInfo({}),
      requestFive: this.dataService.getHistory({}),
      requestSix: this.dataService.getTeams({}),
    })
      .subscribe(({ requestOne, requestTwo, requestThree, requestFour, requestFive, requestSix }) => {
        this.matches = requestOne.result;
        this.matches.forEach(element => {
          element.isEditing = false;
        });

        this.playerPosition = requestTwo.result;
        this.prepareForPaging(this.playerPosition.top3HighestUsers);
        this.nextMatch = requestThree.result;
        this.userInfo = requestFour.result;
        this.histories = requestFive.result;
        this.teams = requestSix.result;
        this.updateCountdown();
      });
  }

  onPredictClick(match: any) {
    if (!match.isEditing) {
      match.isEditing = true;
      return;
    } else {
      if (match.team1ScoredPredicted == null || match.team2ScoredPredicted == null) {
        this.toasrt.error("Invalid scores!");
        return;
      }

      let request =
      {
        MatchId: match.id,
        Team1Score: match.team1ScoredPredicted,
        Team2Score: match.team2ScoredPredicted
      }

      this.dataService.predict(request).subscribe(response => {
        this.toasrt.success(response.message);
      });

      match.isEditing = false;
    }
  }

  changePassword() {
    if (this.oldPass == '' || this.newPass == '' || this.confirmPass == '') {
      this.toasrt.error("Invalid data input");
      return;
    } else {
      let model = {
        OldPassword: this.oldPass,
        NewPassword: this.newPass,
        ConfirmPassword: this.confirmPass
      };
      this.authenService.changePass(model).subscribe(response => {
        this.toasrt.success(response.message);
      });
    }
  }

  setChampion() {
    if (this.champion == '') {
      this.toasrt.error("Invalid data input");
      return;
    } else {
      let model = {
        ChampionName: this.champion.trim(),
        SamePredictionCount: this.samePrediction
      };
      this.dataService.setChampion(model).subscribe(response => {
        this.toasrt.success(response.message);
        this.dataService.getUserInfo({}).subscribe(x => {
          this.userInfo = x.result;
        });
      });
    }
  }

  prepareForPaging(listUsers: any) {
    let i = 0;
    this.paging = [];
    this.ranking = listUsers;
    this.currentRank = this.playerPosition.currentRank;
    this.totalUsers = this.playerPosition.totalUsers;
    console.log(this.currentRank);
    this.ranking.forEach(item => {
      item.rank = i + 1;
      i++;
    });

    let range = Math.ceil(this.ranking.length / 10);
    for (let i = 0; i < range; i++) {
      this.paging.push(i);
    }
    this.rankingPaging = this.ranking.slice(0, 10);
    this.currentPage = 1;
  }

  logOut() {
    this.storage.clearStoreage();
    this.route.navigate(['/login']);
  }

  goToAdminDataSync() {
    this.route.navigate(['/admin/data-sync']);
  }

  scrollToFixtures() {
    document.getElementById('fixtures')?.scrollIntoView({ behavior: 'smooth' });
  }

  changePage(pageIndex: number) {
    this.currentPage = pageIndex + 1;
    this.rankingPaging = this.ranking.slice(pageIndex * 10, pageIndex * 10 + 10);
  }

  changeRankType() {
    this.isFreeRankingChoosen = !this.isFreeRankingChoosen;
    if (this.isFreeRankingChoosen) {
      this.prepareForPaging(this.playerPosition.topHighestFreeUsers);
    } else {
      this.prepareForPaging(this.playerPosition.top3HighestUsers);
    }
  }

  playAudio() {
    let audio = new Audio();
    audio.src = "../../../assets/audio/alert.wav";
    audio.autoplay = true;
    audio.load();
    audio.play();
  }

  getMatchPredicts(id: string) {
    this.isShowPredictPopup = true;
    this.dataService.getMatchPredicts({ id: id }).subscribe(response => {
      this.matchPredictions = response.result;
    });
  }
  hidePredict() {
    this.isShowPredictPopup = false;
  }

  private startCountdown(): void {
    this.updateCountdown();
    this.countdownInterval = setInterval(() => this.updateCountdown(), 1000);
  }

  private updateCountdown(): void {
    const kickOff = this.nextMatch?.kickOfDate
      ? new Date(this.nextMatch.kickOfDate).getTime()
      : new Date('2026-06-11T00:00:00').getTime();
    const diff = Math.max(kickOff - new Date().getTime(), 0);

    this.countdown.days = this.pad(Math.floor(diff / (1000 * 60 * 60 * 24)));
    this.countdown.hours = this.pad(Math.floor((diff / (1000 * 60 * 60)) % 24));
    this.countdown.mins = this.pad(Math.floor((diff / (1000 * 60)) % 60));
    this.countdown.secs = this.pad(Math.floor((diff / 1000) % 60));
  }

  private pad(value: number): string {
    return value < 10 ? `0${value}` : `${value}`;
  }
}
