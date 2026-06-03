import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';
import { LocalStorageService } from 'src/app/core/services/bases/local-storage.service';
import { DataServiceService } from 'src/app/core/services/data-service.service';

@Component({
  selector: 'app-admin-data-sync',
  templateUrl: './admin-data-sync.component.html',
  styleUrls: ['./admin-data-sync.component.scss']
})
export class AdminDataSyncComponent {
  isUpdatingStandings = false;
  isUpdatingMatches = false;
  isUpdatingPreviousMatches = false;
  isUpdatingUsers = false;
  lastStandingsResult = '';
  lastMatchesResult = '';
  lastPreviousMatchesResult = '';
  lastUsersResult = '';

  constructor(
    private dataService: DataServiceService,
    private storage: LocalStorageService,
    private router: Router,
    private toastr: ToastrService
  ) { }

  updateStandings(): void {
    this.isUpdatingStandings = true;

    this.dataService.updateStandings({})
      .pipe(finalize(() => this.isUpdatingStandings = false))
      .subscribe(response => {
        this.lastStandingsResult = this.formatResult(response?.message);
        this.toastr.success(response?.message || 'Updated', 'Standings');
      });
  }

  updateMatches(): void {
    this.isUpdatingMatches = true;

    this.dataService.updateMatches({})
      .pipe(finalize(() => this.isUpdatingMatches = false))
      .subscribe(response => {
        this.lastMatchesResult = this.formatResult(response?.message);
        this.toastr.success(response?.message || 'Updated', 'Matches');
      });
  }

  updatePreviousMatches(): void {
    this.isUpdatingPreviousMatches = true;

    this.dataService.updatePreviousMatches({})
      .pipe(finalize(() => this.isUpdatingPreviousMatches = false))
      .subscribe(response => {
        this.lastPreviousMatchesResult = this.formatResult(response?.message);
        this.toastr.success(response?.message || 'Updated', 'Previous matches');
      });
  }

  updateUserAdditionalInformation(): void {
    this.isUpdatingUsers = true;

    this.dataService.addUserAdditionalInformation({})
      .pipe(finalize(() => this.isUpdatingUsers = false))
      .subscribe(response => {
        this.lastUsersResult = this.formatResult(response?.message);
        this.toastr.success(response?.message || 'Updated', 'User additional information');
      });
  }

  backToGame(): void {
    this.router.navigate(['/about-you']);
  }

  logOut(): void {
    this.storage.clearStoreage();
    this.router.navigate(['/login']);
  }

  private formatResult(message: string): string {
    return `${new Date().toLocaleString()} - ${message || 'Updated'}`;
  }
}
