import { effect, Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'faber-theme';
  private readonly _isDark = signal(this.getStored());
  readonly isDark = this._isDark.asReadonly();

  constructor() {
    effect(() => {
      document.documentElement.classList.toggle('dark', this._isDark());
    });
  }

  toggle(): void {
    const next = !this._isDark();
    this._isDark.set(next);
    localStorage.setItem(this.storageKey, next ? 'dark' : 'light');
  }

  private getStored(): boolean {
    try {
      return localStorage.getItem(this.storageKey) === 'dark';
    } catch {
      return false;
    }
  }
}
