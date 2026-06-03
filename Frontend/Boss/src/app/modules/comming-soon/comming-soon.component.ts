import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { interval } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
  selector: 'app-comming-soon',
  templateUrl: './comming-soon.component.html',
  styleUrls: ['./comming-soon.component.scss']
})
export class CommingSoonComponent implements OnInit {

  days: any;
  hours: any;
  minutes: any;
  seconds: any;

  constructor(private routes : Router) { }

  ngOnInit(): void {
    let countDownDate = new Date("Jun 11, 2021 21:00:0").getTime();

    let now = new Date().getTime();

    let distance = countDownDate - now;

    this.days = Math.floor(distance / (1000 * 60 * 60 * 24));
    this.hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    this.minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
    this.seconds = Math.floor((distance % (1000 * 60)) / 1000);

    if (distance < 0) {
      this.routes.navigate(['/dash-board']);
    }

    var x = setInterval(
      () => {
        now = new Date().getTime();

        distance = countDownDate - now;

        this.days = Math.floor(distance / (1000 * 60 * 60 * 24));
        this.hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        this.minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
        this.seconds = Math.floor((distance % (1000 * 60)) / 1000);

        if (distance < 0) {
          clearInterval(x);
        }
      }, 1000);
  }

  calculateDiff(dateSent: string) {
    let currentDate = new Date();
    let toDate = new Date(dateSent);

    return Math.floor((Date.UTC(currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate()) - Date.UTC(toDate.getFullYear(), toDate.getMonth(), toDate.getDate())) * -1 / (1000 * 60 * 60 * 24));
  }

}
