import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ROUTES } from '../../core/routes/api-routes';
import { UpdateProfileRequest } from './update-profile-request';
import { UpdateProfileResponse } from './update-profile-response';

@Injectable({
  providedIn: 'root',
})
export class UsersClient {
  private readonly httpClient = inject(HttpClient);

  updateProfile(userId: string, request: UpdateProfileRequest): Observable<UpdateProfileResponse> {
    return this.httpClient.put<UpdateProfileResponse>(API_ROUTES.users.byId(userId), request);
  }
}
