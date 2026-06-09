import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AngularMaterialModule } from './angular-material/angular-material.module';
import { FlexLayoutModule } from '@angular/flex-layout';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { LanguageSwitcherComponent } from './i18n/language-switcher.component';
import { TranslatePipe } from './i18n/translate.pipe';

@NgModule({
  declarations: [
    LanguageSwitcherComponent,
    TranslatePipe
  ],
  imports: [
    AngularMaterialModule,
    CommonModule,
    //FlexLayoutModule,
    FormsModule, 
    ReactiveFormsModule
  ],
  exports: [
    AngularMaterialModule,
    //FlexLayoutModule,
    ReactiveFormsModule,
    FormsModule,
    LanguageSwitcherComponent,
    TranslatePipe
  ],
})
export class SharedModule { }
