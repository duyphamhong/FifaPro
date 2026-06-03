import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AngularMaterialModule } from './angular-material/angular-material.module';
import { FlexLayoutModule } from '@angular/flex-layout';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [],
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
    FormsModule
  ],
})
export class SharedModule { }
