import { ChangeDetectorRef, OnDestroy, Pipe, PipeTransform } from '@angular/core';
import { Subscription } from 'rxjs';
import { LanguageService } from './language.service';

@Pipe({
  name: 'translate',
  pure: false
})
export class TranslatePipe implements PipeTransform, OnDestroy {
  private subscription: Subscription;

  constructor(
    private languageService: LanguageService,
    private changeDetector: ChangeDetectorRef
  ) {
    this.subscription = this.languageService.language$.subscribe(() => {
      this.changeDetector.markForCheck();
    });
  }

  transform(key: string, params?: Record<string, string | number>, fallback?: string): string {
    return this.languageService.translate(key, params, fallback);
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }
}
