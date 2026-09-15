import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, finalize } from 'rxjs';
import { API_ROUTES } from '../../core/routes/api-routes';
import { TokenStore } from '../../core/auth/token-store';
import { AuthStore } from '../../core/auth/auth-store';
import { Token } from '../../core/auth/contracts/token';
import { User } from '../../core/auth/contracts/user';
import { SignInRequest } from './sign-in/sign-in-request';
import { SignUpRequest } from './sign-up/sign-up-request';
import { ResetPasswordRequest } from './reset-password/reset-password-request';
import { VerifyEmailRequest } from './verify-email/verify-email-request';

@Injectable({
  providedIn: 'root',
})
export class AuthClient {
  private readonly httpClient = inject(HttpClient);
  private readonly tokenStore = inject(TokenStore);
  private readonly authStore = inject(AuthStore);

  signIn(request: SignInRequest): Observable<Token> {
    return this.httpClient
      .post<Token>(API_ROUTES.auth.signIn, request, { withCredentials: true })
      .pipe(
        tap(token => {
          this.tokenStore.setAccessToken(token.accessToken);
          // `AuthStore` deliberately does not key its `/auth/me` request off
          // the stored token (issue #426), so a newly established session has
          // to be re-resolved explicitly — the mirror of the `clearUser()`
          // call in `signOut`.
          this.authStore.reload();
        }),
      );
  }

  signUp(request: SignUpRequest): Observable<User> {
    return this.httpClient.post<User>(API_ROUTES.auth.signUp, request);
  }

  signOut(): Observable<unknown> {
    return this.httpClient
      .post(API_ROUTES.auth.signOut, {}, {
        withCredentials: true,
        headers: { 'X-Client-Type': 'Web' },
      })
      .pipe(finalize(() => this.authStore.clearUser()));
  }

  refresh(): Observable<Token> {
    return this.httpClient
      .post<Token>(API_ROUTES.auth.refresh, {}, { withCredentials: true })
      .pipe(tap(token => this.tokenStore.setAccessToken(token.accessToken)));
  }

  verifyEmail(request: VerifyEmailRequest): Observable<unknown> {
    return this.httpClient.post(API_ROUTES.auth.verifyEmail, request);
  }

  forgotPassword(email: string): Observable<unknown> {
    return this.httpClient.post(API_ROUTES.auth.forgotPassword, { email });
  }

  resetPassword(request: ResetPasswordRequest): Observable<unknown> {
    return this.httpClient.put(API_ROUTES.auth.resetPassword, request);
  }
}
