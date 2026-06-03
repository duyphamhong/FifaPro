import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from 'src/app/core/services/auth.service';
import { LocalStorageService } from 'src/app/core/services/bases/local-storage.service';

@Component({
  selector: 'app-log-in',
  templateUrl: './log-in.component.html',
  styleUrls: ['./log-in.component.scss']
})
export class LogInComponent implements OnInit {

  public isRegisterMode = false;

  public form = new FormGroup({
    userName: new FormControl('', Validators.required),
    password: new FormControl('', Validators.required),
  });

  public registerForm = new FormGroup({
    userName: new FormControl('', Validators.required),
    email: new FormControl('', [Validators.required, Validators.email]),
    password: new FormControl('', Validators.required),
    confirmPassword: new FormControl('', Validators.required),
  });
  
  constructor(private authen : AuthService,
    private storage: LocalStorageService,
    private routes : Router,
    private toastr: ToastrService
    ) { }

  ngOnInit(): void {
  }

  onSubmit() : void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.authen.login(this.form.value).subscribe(response => {
      this.completeLogin(response);
    });
  }

  register(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    if (this.registerForm.value.password !== this.registerForm.value.confirmPassword) {
      this.toastr.error('Password confirmation does not match.', 'Create account');
      return;
    }

    const user = {
      username: this.registerForm.value.userName,
      email: this.registerForm.value.email,
      password: this.registerForm.value.password
    };

    this.authen.register(user).subscribe(() => {
      this.toastr.success('Account created. Welcome to the game!', 'Create account');
      this.authen.login({
        userName: this.registerForm.value.userName,
        password: this.registerForm.value.password
      }).subscribe(response => {
        this.completeLogin(response);
      });
    });
  }

  setMode(isRegisterMode: boolean): void {
    this.isRegisterMode = isRegisterMode;
  }

  private completeLogin(response: any): void {
    this.storage.setAccessToken(response.token);
    this.storage.setUserName(response.userName);
    this.routes.navigate(['/about-you']);
  }

}
