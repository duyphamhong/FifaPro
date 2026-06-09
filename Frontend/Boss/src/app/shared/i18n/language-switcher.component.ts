import { Component, HostBinding, Input } from '@angular/core';
import { LanguageService } from './language.service';
import { SupportedLanguage } from './translations';

@Component({
  selector: 'app-language-switcher',
  template: `
    <div class="language-switcher" role="group" [attr.aria-label]="'language.label' | translate">
      <button type="button" [class.is-active]="languageService.currentLanguage === 'en'" (click)="setLanguage('en')">
        EN
      </button>
      <button type="button" [class.is-active]="languageService.currentLanguage === 'vi'" (click)="setLanguage('vi')">
        VI
      </button>
    </div>
  `,
  styleUrls: ['./language-switcher.component.scss']
})
export class LanguageSwitcherComponent {
  @Input() tone: 'light' | 'dark' = 'light';

  constructor(public languageService: LanguageService) { }

  @HostBinding('class.is-dark')
  get isDark(): boolean {
    return this.tone === 'dark';
  }

  setLanguage(language: SupportedLanguage): void {
    this.languageService.setLanguage(language);
  }
}
