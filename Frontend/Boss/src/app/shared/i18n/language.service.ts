import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { SupportedLanguage, TRANSLATIONS } from './translations';

@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  readonly storageKey = 'appLanguage';
  readonly languages: SupportedLanguage[] = ['en', 'vi'];
  private readonly languageSubject = new BehaviorSubject<SupportedLanguage>(this.getInitialLanguage());
  readonly language$ = this.languageSubject.asObservable();

  get currentLanguage(): SupportedLanguage {
    return this.languageSubject.value;
  }

  get dateLocale(): string {
    return this.currentLanguage === 'vi' ? 'vi' : 'en-US';
  }

  setLanguage(language: SupportedLanguage): void {
    if (this.languages.indexOf(language) < 0) {
      return;
    }

    localStorage.setItem(this.storageKey, language);
    document.documentElement.lang = language;
    this.languageSubject.next(language);
  }

  translate(key: string, params?: Record<string, string | number>, fallback?: string): string {
    const dictionary = TRANSLATIONS[this.currentLanguage] || TRANSLATIONS.en;
    const englishDictionary = TRANSLATIONS.en;
    const value = dictionary[key] || englishDictionary[key] || fallback || key;

    return this.interpolate(value, params);
  }

  private getInitialLanguage(): SupportedLanguage {
    const storedLanguage = localStorage.getItem(this.storageKey) as SupportedLanguage;
    if (this.languages.indexOf(storedLanguage) >= 0) {
      document.documentElement.lang = storedLanguage;
      return storedLanguage;
    }

    document.documentElement.lang = 'en';
    return 'en';
  }

  private interpolate(value: string, params?: Record<string, string | number>): string {
    if (!params) {
      return value;
    }

    return Object.keys(params).reduce((result, paramKey) =>
      result.replace(new RegExp(`{{\\s*${paramKey}\\s*}}`, 'g'), `${params[paramKey]}`), value);
  }
}
