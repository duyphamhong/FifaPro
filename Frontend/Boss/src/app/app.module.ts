import { NgModule, CUSTOM_ELEMENTS_SCHEMA, APP_INITIALIZER } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { SharedModule } from './shared/shared.module';
import { DashBoardComponent } from './modules/dash-board/dash-board.component';
import { LogInComponent } from './modules/log-in/log-in.component';
import { AppCoreModule } from './core/app-core.module';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { CustomHttpInterceptor } from './core/services/bases/customHttpInterceptor';
import { ToastrModule  } from 'ngx-toastr';
import { CommingSoonComponent } from './modules/comming-soon/comming-soon.component';
import { AboutYouComponent } from './modules/about-you/about-you.component';
import { AdminDataSyncComponent } from './modules/admin-data-sync/admin-data-sync.component';

@NgModule({
  declarations: [
    AppComponent,
    DashBoardComponent,
    LogInComponent,
    CommingSoonComponent,
    AboutYouComponent,
    AdminDataSyncComponent
  ],
  imports: [
    AppRoutingModule,
    BrowserModule,
    SharedModule,
    AppCoreModule,
    BrowserAnimationsModule,
    ToastrModule.forRoot({
      timeOut: 2000,
      positionClass: 'toast-bottom-right',
      preventDuplicates: true,
    }),
  ],
  providers: [
    {provide: HTTP_INTERCEPTORS, useClass: CustomHttpInterceptor, multi: true},
  ],
  bootstrap: [AppComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA]
})
export class AppModule { }
