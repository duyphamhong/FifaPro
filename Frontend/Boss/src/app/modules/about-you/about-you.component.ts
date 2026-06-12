import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import * as signalR from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { forkJoin, Subscription } from 'rxjs';
import { AuthService } from 'src/app/core/services/auth.service';
import { LocalStorageService } from 'src/app/core/services/bases/local-storage.service';
import { DataServiceService } from 'src/app/core/services/data-service.service';
import { LanguageService } from 'src/app/shared/i18n/language.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-about-you',
  templateUrl: './about-you.component.html',
  styleUrls: ['./about-you.component.scss']
})
export class AboutYouComponent implements OnInit, OnDestroy {
  matches: any[];
  matchGroups: any[];
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
  avatarUrl: string;
  champion: string;
  samePrediction: number;
  teams: any[];
  matchPredictions: any;
  chatData: any[];
  onlineUsers: any[];
  chatContent: string;
  prophesyMessage: string;
  prophesyAuthor: string;
  isChatMuted: boolean;
  isChatExpanded: boolean;
  hasMutedUnreadChat: boolean;
  isChatNotificationShaking: boolean;
  hasReceivedOnlineUsers: boolean;
  isFreeRankingChoosen: boolean = true;
  isAdmin: boolean = false;

  baseUrl = '';
  defaultFlagUrl = 'assets/images/default-flag.svg';
  private readonly matchDurationInMinutes = 120;

  isShowPredictPopup: boolean;
  isShowPasswordPopup: boolean;
  isShowAvatarPopup: boolean;
  isShowSponsorPopup: boolean;
  private hubConnection: signalR.HubConnection;
  private languageSubscription: Subscription | null = null;
  private sponsorDropTimeout: any;
  private unreadHighlightTimeout: any;
  private chatShakeStartTimeout: any;
  private chatShakeTimeout: any;
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
    private toasrt: ToastrService,
    public languageService: LanguageService) {
    this.matches = [];
    this.matchGroups = [];
    this.currentUserName = this.storage.getUserName();
    this.oldPass = '';
    this.newPass = '';
    this.confirmPass = '';
    this.avatarUrl = '';
    this.champion = '';
    this.chatData = [];
    this.onlineUsers = [];
    this.chatContent = '';
    this.prophesyMessage = '';
    this.prophesyAuthor = '';
    this.isChatMuted = false;
    this.isChatExpanded = false;
    this.hasMutedUnreadChat = false;
    this.isChatNotificationShaking = false;
    this.hasReceivedOnlineUsers = false;
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
    this.isShowPasswordPopup = false;
    this.isShowAvatarPopup = false;
    this.isShowSponsorPopup = false;
    this.matchPredictions = {};
    this.isAdmin = this.storage.hasRole('Admin');

    //signR
    this.baseUrl = environment.baseApiUrl;
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.baseUrl}/chat?userName=${encodeURIComponent(this.currentUserName)}`, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .build();

  }

  ngOnInit(): void {
    this.languageSubscription = this.languageService.language$.subscribe(() => {
      if (this.nextMatch) {
        this.generateProphesy();
      }
    });
    this.loadInfo();
    this.startCountdown();
    this.startChatConnection();
    this.scheduleSponsorDialog();
  }

  ngOnDestroy(): void {
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }

    if (this.sponsorDropTimeout) {
      clearTimeout(this.sponsorDropTimeout);
    }

    this.clearChatNotificationTimers();

    if (this.hubConnection) {
      this.hubConnection.stop();
    }

    if (this.languageSubscription) {
      this.languageSubscription.unsubscribe();
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
        this.prepareMatchGroups();

        this.playerPosition = requestTwo.result;
        this.prepareForPaging(this.playerPosition.topHighestFreeUsers);
        this.nextMatch = requestThree.result;
        this.userInfo = requestFour.result;
        this.histories = requestFive.result;
        this.teams = requestSix.result;
        this.generateProphesy();
        this.updateCountdown();
        this.loadChats();
      });
  }

  onPredictClick(match: any) {
    if (!match.isEditing) {
      match.isEditing = true;
      return;
    } else {
      if (match.team1ScoredPredicted == null || match.team2ScoredPredicted == null) {
        this.toasrt.error(this.languageService.translate('toast.invalidScores'));
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
      this.toasrt.error(this.languageService.translate('toast.invalidDataInput'));
      return;
    } else {
      let model = {
        OldPassword: this.oldPass,
        NewPassword: this.newPass,
        ConfirmPassword: this.confirmPass
      };
      this.authenService.changePass(model).subscribe(response => {
        this.toasrt.success(response.message);
        this.hidePasswordDialog();
      });
    }
  }

  updateAvatar() {
    if (!this.avatarUrl || this.avatarUrl.trim() == '') {
      this.toasrt.error(this.languageService.translate('toast.invalidAvatarUrl'));
      return;
    }

    const avatarUrl = this.avatarUrl.trim();
    this.dataService.updateAvatar({ AvatarUrl: avatarUrl }).subscribe(response => {
      this.toasrt.success(response.message);
      this.userInfo.avatarUrl = avatarUrl;
      this.hideAvatarDialog();
      this.refreshRanking();
    });
  }

  setChampion() {
    if (this.champion == '') {
      this.toasrt.error(this.languageService.translate('toast.invalidDataInput'));
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

  getFlagUrl(flagUrl: string): string {
    return flagUrl || this.defaultFlagUrl;
  }

  useDefaultFlag(event: Event): void {
    const image = event.target as HTMLImageElement;
    image.src = this.defaultFlagUrl;
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
    if (this.isChatMuted) {
      return;
    }

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

  toggleMatchGroup(group: any) {
    group.isExpanded = !group.isExpanded;
  }

  translateRound(description: string): string {
    if (!description) {
      return '';
    }

    const roundKey = this.getRoundTranslationKey(description);
    return roundKey ? this.languageService.translate(roundKey, undefined, description) : description;
  }

  postChat() {
    this.clearChatNotification();

    if (!this.chatContent || this.chatContent.trim() == '') {
      this.toasrt.error(
        this.languageService.translate('toast.messageRequired'),
        this.languageService.translate('toast.chatRoom')
      );
      return;
    }

    if (!this.nextMatch?.id) {
      this.toasrt.error(
        this.languageService.translate('toast.noActiveMatch'),
        this.languageService.translate('toast.chatRoom')
      );
      return;
    }

    const chatModel = {
      matchId: this.nextMatch.id,
      userName: this.currentUserName,
      avatar: this.userInfo?.avatarUrl || 'https://www.w3schools.com/w3css/img_avatar2.png',
      content: this.chatContent.trim()
    };

    this.dataService.sendChats(chatModel).subscribe(() => {
      this.chatContent = '';
      this.clearChatNotification();
    });
  }

  onChatEnter(event: Event) {
    const keyboardEvent = event as KeyboardEvent;
    if (keyboardEvent.shiftKey) {
      return;
    }

    keyboardEvent.preventDefault();
    this.postChat();
  }

  toggleChatMute() {
    this.isChatMuted = !this.isChatMuted;
    if (!this.isChatMuted) {
      this.clearChatNotification();
    }
  }

  expandChatRoom() {
    this.isChatExpanded = true;
    this.clearChatNotification();
    this.scrollChatToBottom();
  }

  collapseChatRoom() {
    this.isChatExpanded = false;
    this.scrollChatToBottom();
  }

  generateProphesy() {
    const team1 = this.nextMatch?.team1 || this.languageService.translate('common.tbd');
    const team2 = this.nextMatch?.team2 || this.languageService.translate('common.tbd');
    const winner = Math.random() > 0.5 ? team1 : team2;
    const loser = winner === team1 ? team2 : team1;
    const winnerScore = this.randomScore(1, 5);
    const loserScore = this.randomScore(0, Math.min(winnerScore - 1, 3));
    const score = winner === team1
      ? `${team1} ${winnerScore}-${loserScore} ${team2}`
      : `${team1} ${loserScore}-${winnerScore} ${team2}`;
    const prophesyTemplates = this.languageService.currentLanguage === 'vi'
      ? this.prophesyTemplatesVi
      : this.prophesyTemplates;
    const template = prophesyTemplates[Math.floor(Math.random() * prophesyTemplates.length)];
    const author = this.prophesyAuthors[Math.floor(Math.random() * this.prophesyAuthors.length)];
    const message = template
      .replace(/{winner}/g, winner)
      .replace(/{loser}/g, loser)
      .replace(/{team1}/g, team1)
      .replace(/{team2}/g, team2)
      .replace(/{score}/g, score);

    this.prophesyMessage = `"${message}"`;
    this.prophesyAuthor = author;
  }

  mentionOnlineUser(user: any) {
    const mention = `@${user?.userName || ''}`;
    if (!mention.trim()) {
      return;
    }

    this.chatContent = this.chatContent
      ? `${this.chatContent.trim()} ${mention} `
      : `${mention} `;
  }

  clearChatNotification() {
    this.hasMutedUnreadChat = false;
    this.isChatNotificationShaking = false;
    this.clearUnreadMessages();
    this.clearChatNotificationTimers();
  }

  showPasswordDialog() {
    this.isShowPasswordPopup = true;
  }

  hidePasswordDialog() {
    this.isShowPasswordPopup = false;
    this.oldPass = '';
    this.newPass = '';
    this.confirmPass = '';
  }

  showAvatarDialog() {
    this.avatarUrl = this.userInfo?.avatarUrl || '';
    this.isShowAvatarPopup = true;
  }

  hideAvatarDialog() {
    this.isShowAvatarPopup = false;
    this.avatarUrl = '';
  }

  showSponsorDialog() {
    if (this.sponsorDropTimeout) {
      clearTimeout(this.sponsorDropTimeout);
      this.sponsorDropTimeout = null;
    }

    this.isShowSponsorPopup = true;
  }

  hideSponsorDialog() {
    this.isShowSponsorPopup = false;
  }

  private scheduleSponsorDialog(): void {
    this.sponsorDropTimeout = setTimeout(() => this.showSponsorDialog(), 10000);
  }

  private refreshRanking(): void {
    this.dataService.getPlayerPosition({}).subscribe(response => {
      this.playerPosition = response.result;
      this.prepareForPaging(this.isFreeRankingChoosen
        ? this.playerPosition.topHighestFreeUsers
        : this.playerPosition.top3HighestUsers);
    });
  }

  private prepareMatchGroups(): void {
    this.updateMatchStatuses();

    const orderedDescriptions = [
      'Group stage',
      'Round of 32',
      'Round of 16',
      'Quarter-finals',
      'Semi-finals',
      'Final'
    ];

    const groups = new Map<string, any[]>();
    this.matches.forEach(match => {
      const groupName = match.description || 'Other';
      if (!groups.has(groupName)) {
        groups.set(groupName, []);
      }

      groups.get(groupName)?.push(match);
    });

    this.matchGroups = Array.from(groups.keys())
      .sort((a, b) => {
        const firstIndex = orderedDescriptions.indexOf(a);
        const secondIndex = orderedDescriptions.indexOf(b);
        if (firstIndex === -1 && secondIndex === -1) {
          return a.localeCompare(b);
        }
        if (firstIndex === -1) {
          return 1;
        }
        if (secondIndex === -1) {
          return -1;
        }
        return firstIndex - secondIndex;
      })
      .map((description, index) => ({
        description: description,
        isExpanded: index === 0,
        matches: (groups.get(description) || []).sort((a, b) =>
          new Date(a.kickOfDate).getTime() - new Date(b.kickOfDate).getTime())
      }));
  }

  private startCountdown(): void {
    this.updateCountdown();
    this.countdownInterval = setInterval(() => this.updateCountdown(), 1000);
  }

  private updateCountdown(): void {
    this.updateMatchStatuses();

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

  private randomScore(min: number, max: number): number {
    return Math.floor(Math.random() * (max - min + 1)) + min;
  }

  private updateMatchStatuses(): void {
    if (!this.matches?.length) {
      return;
    }

    const now = new Date().getTime();
    const matchDuration = this.matchDurationInMinutes * 60 * 1000;

    this.matches.forEach(match => {
      const kickOff = match?.kickOfDate ? new Date(match.kickOfDate).getTime() : NaN;
      if (Number.isNaN(kickOff)) {
        match.isHappening = false;
        return;
      }

      const finishTime = kickOff + matchDuration;
      match.isHappening = now >= kickOff && now <= finishTime;
      match.isPassed = match.isPassed || now > finishTime;
    });
  }

  private startChatConnection(): void {
    this.hubConnection.start()
      .catch(err => this.toasrt.error(this.languageService.translate('toast.chatConnectionError', { error: err })));

    this.hubConnection.on('broadcastchatdata', (data) => {
      if (!this.isCurrentChatRoom(data)) {
        return;
      }

      if (this.isIncomingUserChat(data)) {
        if (this.isChatMuted) {
          this.hasMutedUnreadChat = true;
          this.shakeChatNotification();
        } else {
          this.isChatExpanded = true;
          this.markUnreadMessage(data);
        }

        this.playAudio();
      }

      this.chatData.push(data);
      this.scrollChatToBottom();
    });

    this.hubConnection.on('broadcastonlineusers', (data) => {
      const previousUsers = this.onlineUsers || [];
      const nextUsers = data || [];
      this.addPresenceMessages(previousUsers, nextUsers);
      this.onlineUsers = nextUsers;
    });
  }

  private loadChats(): void {
    if (!this.nextMatch?.id) {
      this.chatData = [];
      return;
    }

    this.dataService.getChats({ matchId: this.nextMatch.id }).subscribe(response => {
      this.chatData = response.result || [];
      this.clearChatNotification();
      this.scrollChatToBottom();
    });
  }

  private scrollChatToBottom(): void {
    setTimeout(() => {
      const chatBoxes = document.querySelectorAll('.wc-chat-messages');
      chatBoxes.forEach((chatBox: any) => chatBox.scrollTop = chatBox.scrollHeight);
    });
  }

  private isCurrentChatRoom(data: any): boolean {
    const messageMatchId = `${data?.matchId || data?.MatchId || ''}`;
    const currentMatchId = `${this.nextMatch?.id || ''}`;
    return messageMatchId === currentMatchId;
  }

  private isIncomingUserChat(data: any): boolean {
    return !!data?.userName && data.userName !== this.currentUserName && !data?.isSystem;
  }

  private markUnreadMessage(message: any): void {
    message.isUnread = true;

    if (this.unreadHighlightTimeout) {
      clearTimeout(this.unreadHighlightTimeout);
    }

    this.unreadHighlightTimeout = setTimeout(() => {
      this.clearUnreadMessages();
      this.unreadHighlightTimeout = null;
    }, 10000);
  }

  private clearUnreadMessages(): void {
    this.chatData.forEach(message => {
      if (message) {
        message.isUnread = false;
      }
    });
  }

  private shakeChatNotification(): void {
    this.isChatNotificationShaking = false;

    if (this.chatShakeTimeout) {
      clearTimeout(this.chatShakeTimeout);
    }

    this.chatShakeStartTimeout = setTimeout(() => {
      this.chatShakeStartTimeout = null;
      this.isChatNotificationShaking = true;

      this.chatShakeTimeout = setTimeout(() => {
        this.isChatNotificationShaking = false;
        this.chatShakeTimeout = null;
      }, 10000);
    });
  }

  private clearChatNotificationTimers(): void {
    if (this.unreadHighlightTimeout) {
      clearTimeout(this.unreadHighlightTimeout);
      this.unreadHighlightTimeout = null;
    }

    if (this.chatShakeStartTimeout) {
      clearTimeout(this.chatShakeStartTimeout);
      this.chatShakeStartTimeout = null;
    }

    if (this.chatShakeTimeout) {
      clearTimeout(this.chatShakeTimeout);
      this.chatShakeTimeout = null;
    }
  }

  private addPresenceMessages(previousUsers: any[], nextUsers: any[]): void {
    if (!this.hasReceivedOnlineUsers) {
      this.hasReceivedOnlineUsers = true;
      return;
    }

    const previousNames = previousUsers.map(x => x.userName);
    const nextNames = nextUsers.map(x => x.userName);

    nextUsers
      .filter(x => !previousNames.includes(x.userName) && x.userName !== this.currentUserName)
      .forEach(x => this.addSystemChatMessage(this.languageService.translate('chat.userOnline', { userName: x.userName })));

    previousUsers
      .filter(x => !nextNames.includes(x.userName) && x.userName !== this.currentUserName)
      .forEach(x => this.addSystemChatMessage(this.languageService.translate('chat.userOffline', { userName: x.userName })));
  }

  private getRoundTranslationKey(description: string): string {
    switch (description) {
      case 'Group stage':
        return 'round.groupStage';
      case 'Round of 32':
        return 'round.roundOf32';
      case 'Round of 16':
        return 'round.roundOf16';
      case 'Quarter-finals':
        return 'round.quarterFinals';
      case 'Semi-finals':
        return 'round.semiFinals';
      case 'Final':
        return 'round.final';
      case 'Other':
        return 'round.other';
      default:
        return '';
    }
  }

  private addSystemChatMessage(content: string): void {
    this.chatData.push({
      isSystem: true,
      content: content,
      createdDate: new Date()
    });
    this.scrollChatToBottom();
  }

  private readonly prophesyTemplates: string[] = [
    '{winner} are clearly the favorites in this match. What are you waiting for? Sell the house and go all-in on {winner}! {score}.',
    'The football spirits checked the weather, the grass, and someone’s lucky socks. They say {winner} will handle {loser}. {score}.',
    '{loser} may arrive with hope, but {winner} arrive with plot armor. My brave nonsense says {score}.',
    'I asked the universe nicely and it whispered, “stop overthinking, pick {winner}.” Final prophecy: {score}.',
    '{winner} look like they borrowed confidence from the future. {loser} should be worried. {score}.',
    'The ball told me it prefers {winner}. I do not question talking footballs. {score}.',
    '{loser} can bring tactics, snacks, and emotional support. {winner} still take this one. {score}.',
    'My calculator started sweating and printed only one answer: {score}. Trust the machine, maybe.',
    '{winner} have main-character energy today. {loser} are just in the episode for tension. {score}.',
    'The stars have aligned, then immediately pointed at {winner}. That usually means {score}.',
    'Someone in {winner} woke up feeling dangerous. Someone in {loser} woke up wishing this was volleyball. {score}.',
    'This prediction has been approved by zero licensed experts and one very confident browser tab: {score}.',
    '{winner} should win this unless football decides to be dramatic again. I say {score}.',
    'The vibes department filed its report: {winner} over {loser}. Recommended bet slip: {score}.',
    '{loser} might start well, but {winner} will bring the plot twist. {score}.',
    'I simulated this match in my head for four seconds. Very scientific. {score}.',
    '{winner} are cooking today. {loser} are mostly holding the menu. {score}.',
    'The prophecy chicken has crossed the road toward {winner}. That can only mean {score}.',
    '{loser} have a chance, technically. My fortune cookie still says {winner}. {score}.',
    'A mysterious spreadsheet appeared and ranked {winner} first in “probably not embarrassing us.” {score}.',
    '{winner} will win because the scriptwriter needs a clean arc. {score}.',
    'The moon is in offside position, which strongly favors {winner}. {score}.',
    '{loser} can park the bus, but {winner} brought a tow truck. {score}.',
    'The smart money says be careful. The fun money says {winner}. The chaotic money says {score}.',
    'I asked a coin. It landed on its edge and still somehow picked {winner}. {score}.',
    '{winner} have the sharper boots, louder fans, and better fictional destiny. {score}.',
    '{loser} may defend with honor. {winner} may score with disrespect. {score}.',
    'My crystal ball is cracked, but even cracked glass can see {winner} winning. {score}.',
    'The match preview says balance. My nonsense radar says {winner}. {score}.',
    '{winner} are giving “we practiced this” energy. {loser} are giving “we hope this works” energy. {score}.',
    'The safest prediction is never safe, so naturally I’m yelling {score}.',
    '{winner} will score first, celebrate loudly, and make {loser} question breakfast choices. {score}.',
    'Today’s horoscope: if you support {winner}, breathe easy. If you support {loser}, hydrate. {score}.',
    'The data is incomplete, the confidence is illegal, and the pick is {winner}. {score}.',
    '{loser} are not bad. {winner} are just carrying the suspicious glow of destiny. {score}.',
    'If football were a cooking show, {winner} would be plating dessert while {loser} peel onions. {score}.',
    'My imaginary assistant coach says {winner} by pure tactical wizardry. {score}.',
    'The crowd will gasp, the commentators will shout, and {winner} will probably survive. {score}.',
    '{winner} have that “don’t text your ex, bet on us” energy. {score}.',
    'A tiny committee of vibes has voted unanimously for {winner}. Final answer: {score}.',
    '{loser} might bring the passion, but {winner} bring the receipt. {score}.',
    'This is either genius or comedy. Either way, the prophecy says {score}.',
    '{winner} will make the game look simple, which is rude but effective. {score}.',
    'The football gods love drama, but today they also love {winner}. {score}.',
    '{loser} should not panic. Actually, maybe panic a little. {score}.',
    'I read the tea leaves, then spilled the tea. The stain looked like {winner}. {score}.',
    '{winner} are favorites because I said so with confidence. That counts online. {score}.',
    'The locker room mirror allegedly winked at {winner}. Strong omen. {score}.',
    'If this prediction fails, blame gravity. If it wins, praise {winner}. {score}.',
    'No spreadsheet, no scouting report, just raw tournament chaos: {score}.',
    '{winner} take this one, {loser} learn character development, everybody gets content. {score}.'
  ];

  private readonly prophesyTemplatesVi: string[] = [
    '{winner} đang sáng cửa hơn ở trận này. Đừng nghĩ nhiều quá, dự đoán của tôi là {score}.',
    'Bộ phận cảm tính đã nộp báo cáo: {winner} vượt qua {loser}. Phiếu gợi ý: {score}.',
    '{loser} có thể nhập cuộc đầy hy vọng, nhưng {winner} trông giống đội đang cầm kịch bản. {score}.',
    'Tôi mô phỏng trận này trong đầu đúng bốn giây. Rất khoa học. Kết quả là {score}.',
    '{winner} có vẻ mượn được sự tự tin từ tương lai. {loser} nên hơi lo một chút. {score}.',
    'Dữ liệu chưa đủ, niềm tin thì hơi quá tay, nhưng lựa chọn vẫn là {winner}. {score}.',
    '{loser} có cơ hội trên lý thuyết. Linh cảm của tôi vẫn chỉ về {winner}. {score}.',
    'Nếu bóng đá thích drama thì mọi thứ có thể đảo chiều, nhưng hôm nay tôi chọn {winner}. {score}.',
    'Bảng tính bí ẩn vừa xuất hiện và xếp {winner} ở vị trí đáng tin nhất. {score}.',
    '{winner} sẽ ghi bàn, ăn mừng lớn, rồi khiến {loser} phải suy nghĩ lại kế hoạch. {score}.',
    'Dự đoán này không được chuyên gia nào bảo chứng, nhưng nghe vẫn rất tự tin: {score}.',
    '{winner} mang năng lượng của đội đã chuẩn bị kỹ. {loser} mang năng lượng của đội hy vọng mọi thứ ổn. {score}.',
    'Trận này hoặc là thiên tài, hoặc là hài kịch. Dù sao lời tiên tri vẫn nói {score}.',
    'Lựa chọn an toàn nhất không bao giờ thật sự an toàn, nên tôi mạnh dạn gọi {score}.',
    '{winner} thắng trận này, {loser} nhận thêm bài học, còn chúng ta có chuyện để nói. {score}.'
  ];

  private readonly prophesyAuthors: string[] = [
    'Albert Einstein',
    'Elon Musk',
    'Baba Vanga',
    'Nostradamus',
    'Cristiano Ronaldo',
    'Lionel Messi',
    'Pep Guardiola',
    'Jose Mourinho',
    'Sir Alex Ferguson',
    'Zinedine Zidane',
    'Diego Maradona',
    'Pele',
    'Kylian Mbappe',
    'Erling Haaland',
    'Taylor Swift',
    'MrBeast',
    'The Rock',
    'Snoop Dogg',
    'Barack Obama',
    'Gordon Ramsay'
  ];
}
