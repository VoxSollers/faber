import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { AuthStore } from './auth-store';
import { TokenStore } from './token-store';
import { API_ROUTES } from '../routes/api-routes';

// Built from a JSON string, exactly as the API serializes MeResponse
// (camelCase keys, `username` as one word), so the frontend contract's
// spelling cannot leak into the fixture. See the #422 regression below.
function mePayload(username = 'johndoe'): object {
  return JSON.parse(
    `{"id":"user-1","username":"${username}","email":"john@example.com","firstName":"John","lastName":"Doe"}`,
  );
}

describe('AuthStore', () => {
  let service: AuthStore;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthStore);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('regression #426 (initial /auth/me with no stored token)', () => {
    it('should keep the resolved user when no access token is stored', async () => {
      // No token is seeded on purpose: this is the cookie-only path — a hard
      // reload, where the session lives entirely in the httpOnly cookie and
      // TokenStore starts back at null.
      TestBed.tick();

      const req = httpMock.expectOne(API_ROUTES.auth.me);
      expect(req.request.method).toBe('GET');
      expect(req.request.withCredentials).toBe(true);

      req.flush(mePayload());
      await Promise.resolve();
      TestBed.tick();

      expect(service.user()?.username).toBe('johndoe');
      expect(service.authenticated()).toBe(true);
    });

    it('should issue exactly one GET /auth/me for the initial check', async () => {
      TestBed.tick();
      httpMock.expectOne(API_ROUTES.auth.me).flush(mePayload());
      await Promise.resolve();
      TestBed.tick();

      httpMock.expectNone(API_ROUTES.auth.me);
      expect(service.authenticated()).toBe(true);
    });

    it('should issue exactly one GET /auth/me when an access token is stored', async () => {
      TestBed.inject(TokenStore).setAccessToken('test-token');
      TestBed.tick();

      httpMock.expectOne(API_ROUTES.auth.me).flush(mePayload());
      await Promise.resolve();
      TestBed.tick();

      httpMock.expectNone(API_ROUTES.auth.me);
      expect(service.authenticated()).toBe(true);
    });
  });

  describe('clearUser', () => {
    it('should drop the user and the token without issuing another request', async () => {
      const tokenStore = TestBed.inject(TokenStore);
      tokenStore.setAccessToken('test-token');
      TestBed.tick();
      httpMock.expectOne(API_ROUTES.auth.me).flush(mePayload());
      await Promise.resolve();
      TestBed.tick();
      expect(service.authenticated()).toBe(true);

      service.clearUser();
      await Promise.resolve();
      TestBed.tick();

      expect(service.user()).toBeNull();
      expect(service.authenticated()).toBe(false);
      expect(tokenStore.accessToken()).toBeNull();
      // Sign-out has already ended the session server-side; re-asking
      // /auth/me would be a wasted (and 401-ing) round trip.
      httpMock.expectNone(API_ROUTES.auth.me);
    });
  });

  describe('reload', () => {
    it('should re-resolve the user with exactly one further request', async () => {
      TestBed.tick();
      httpMock.expectOne(API_ROUTES.auth.me).flush(mePayload());
      await Promise.resolve();
      TestBed.tick();

      service.reload();
      TestBed.tick();

      httpMock.expectOne(API_ROUTES.auth.me).flush(mePayload('janedoe'));
      await Promise.resolve();
      TestBed.tick();

      expect(service.user()?.username).toBe('janedoe');
      httpMock.expectNone(API_ROUTES.auth.me);
    });

    it('should re-resolve the user after clearUser (sign in again)', async () => {
      TestBed.tick();
      httpMock.expectOne(API_ROUTES.auth.me).flush(mePayload());
      await Promise.resolve();
      TestBed.tick();

      service.clearUser();
      await Promise.resolve();
      TestBed.tick();
      expect(service.authenticated()).toBe(false);

      service.reload();
      TestBed.tick();

      httpMock.expectOne(API_ROUTES.auth.me).flush(mePayload('janedoe'));
      await Promise.resolve();
      TestBed.tick();

      expect(service.user()?.username).toBe('janedoe');
      expect(service.authenticated()).toBe(true);
    });
  });

  describe('regression #422 (real /auth/me payload shape)', () => {
    it('should expose username from a response body whose key is "username"', async () => {
      TestBed.inject(TokenStore).setAccessToken('test-token');
      TestBed.tick();

      const req = httpMock.expectOne(API_ROUTES.auth.me);
      expect(req.request.method).toBe('GET');

      req.flush(mePayload());
      await Promise.resolve();
      TestBed.tick();

      expect(service.user()?.username).toBe('johndoe');
      expect(service.authenticated()).toBe(true);
    });
  });
});
