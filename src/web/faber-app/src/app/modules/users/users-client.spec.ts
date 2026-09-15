import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { UsersClient } from './users-client';
import { API_ROUTES } from '../../core/routes/api-routes';
import { UpdateProfileRequest } from './update-profile-request';
import { UpdateProfileResponse } from './update-profile-response';

describe('UsersClient', () => {
  let client: UsersClient;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });

    client = TestBed.inject(UsersClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  describe('updateProfile', () => {
    const userId = 'user-123';
    const request: UpdateProfileRequest = { firstName: 'Jane', lastName: 'Doe' };

    it('should send PUT to users/{id} with request body', () => {
      client.updateProfile(userId, request).subscribe();

      const req = httpMock.expectOne(API_ROUTES.users.byId(userId));
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(request);
      req.flush({ firstName: 'Jane', lastName: 'Doe' });
    });

    it('should return updated name from response', () => {
      const response = { firstName: 'Jane', lastName: 'Doe' };
      let result: UpdateProfileResponse | undefined;

      client.updateProfile(userId, request).subscribe(r => (result = r));
      httpMock.expectOne(API_ROUTES.users.byId(userId)).flush(response);

      expect(result).toEqual(response);
    });
  });
});
