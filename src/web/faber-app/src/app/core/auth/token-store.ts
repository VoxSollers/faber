import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class TokenStore {
  private readonly accessTokenSignal = signal<string | null>(null);

  readonly accessToken = this.accessTokenSignal.asReadonly();

  setAccessToken(token: string): void {
    this.accessTokenSignal.set(token);
  }

  removeAccessToken(): void {
    this.accessTokenSignal.set(null);
  }
}
