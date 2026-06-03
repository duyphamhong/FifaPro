import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { GuardService } from './core/services/bases/guard-service';
import { AboutYouComponent } from './modules/about-you/about-you.component';
import { AdminDataSyncComponent } from './modules/admin-data-sync/admin-data-sync.component';
import { CommingSoonComponent } from './modules/comming-soon/comming-soon.component';
import { DashBoardComponent } from './modules/dash-board/dash-board.component';
import { LogInComponent } from './modules/log-in/log-in.component';

const routes: Routes = [
  {
    path: 'login', component: LogInComponent,
    canActivate: [
      GuardService
    ],
    data: {path: 'login'}
  },
  {
    path: 'about-you', component: AboutYouComponent,
    canActivate: [
      GuardService
    ],
  },
  {
    path: 'admin/data-sync', component: AdminDataSyncComponent,
    canActivate: [
      GuardService
    ],
    data: { role: 'Admin' }
  },
  { path: '', redirectTo: '/about-you', pathMatch: 'full' }, // redirect to `first-component`
  // { path: '**', component: PageNotFoundComponent },  // Wildcard route for a 404 page
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
