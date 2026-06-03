import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/core/services/auth.service';
import { LocalStorageService } from 'src/app/core/services/bases/local-storage.service';

@Component({
  selector: 'app-log-in',
  templateUrl: './log-in.component.html',
  styleUrls: ['./log-in.component.scss']
})
export class LogInComponent implements OnInit {

  public form = new FormGroup({
    userName: new FormControl(''),
    password: new FormControl(''),
  });
  
  constructor(private authen : AuthService,
    private storage: LocalStorageService,
    private routes : Router
    ) { }

  ngOnInit(): void {
  }

  onSubmit() : void {
    this.authen.login(this.form.value).subscribe(response => {
      this.storage.setAccessToken(response.token);
      this.storage.setUserName(response.userName);
      this.routes.navigate(['/about-you']);
    });
  }

}
