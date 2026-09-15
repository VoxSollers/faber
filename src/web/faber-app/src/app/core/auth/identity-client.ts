import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ROUTES } from '../routes/api-routes';

export interface VerifyActionTokenResponse {
  isValid: boolean;
}

@Injectable({ providedIn: 'root' })
export class IdentityClient {
  private readonly httpClient = inject(HttpClient);

  verifyActionToken(key: string): Observable<VerifyActionTokenResponse> {
    return this.httpClient.post<VerifyActionTokenResponse>(
      API_ROUTES.identity.verifyActionToken,
      { combinedKey: { value: key }, type: 'VerifyEmail' },
    );
  }
}
