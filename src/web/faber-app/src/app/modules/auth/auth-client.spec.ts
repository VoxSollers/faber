import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';

import { AuthClient } from './auth-client';
import { TokenStore } from '../../core/auth/token-store';
import { AuthStore } from '../../core/auth/auth-store';
import { API_ROUTES } from '../../core/routes/api-routes';
import { Token } from '../../core/auth/contracts/token';
import { User } from '../../core/auth/contracts/user';
import { SignInRequest } from './sign-in/sign-in-request';
import { SignUpRequest } from './sign-up/sign-up-request';
import { ResetPasswordRequest } from './reset-password/reset-password-request';
import { VerifyEmailRequest } from './verify-email/verify-email-request';

describe('AuthClient', () => {
  let client: AuthClient;
  let httpMock: HttpTestingController;
  let mockTokenStore: { setAccessToken: ReturnType<typeof vi.fn>; removeAccessToken: ReturnType<typeof vi.fn>; accessToken: ReturnType<typeof signal<string | null>> };
  let mockAuthStore: { clearUser: ReturnType<typeof vi.fn>; reload: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    mockTokenStore = {
      setAccessToken: vi.fn(),
      removeAccessToken: vi.fn(),
      accessToken: signal(null),
    };

    mockAuthStore = {
      clearUser: vi.fn(),
      reload: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: TokenStore, useValue: mockTokenStore },
        { provide: AuthStore, useValue: mockAuthStore },
      ],
    });

    client = TestBed.inject(AuthClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('signIn', () => {
    const request: SignInRequest = { userName: 'testuser', password: 'password123' };
    const tokenResponse: Token = { accessToken: 'access-token', refreshToken: 'refresh-token' };

    it('should send POST to sign-in URL with request body and withCredentials', () => {
      client.signIn(request).subscribe();

      const req = httpMock.expectOne(API_ROUTES.auth.signIn);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(request);
      expect(req.request.withCredentials).toBe(true);
      req.flush(tokenResponse);
    });

    it('should store access token on success', () => {
      client.signIn(request).subscribe();

      httpMock.expectOne(API_ROUTES.auth.signIn).flush(tokenResponse);

      expect(mockTokenStore.setAccessToken).toHaveBeenCalledWith('access-token');
    });

    it('should ask AuthStore to re-resolve the user on success', () => {
      client.signIn(request).subscribe();

      httpMock.expectOne(API_ROUTES.auth.signIn).flush(tokenResponse);

      // AuthStore no longer keys its /auth/me request off the stored token
      // (that feedback loop discarded the resolved user — issue #426), so a
      // freshly established session has to be re-resolved explicitly.
      expect(mockAuthStore.reload).toHaveBeenCalled();
    });

    it('should not re-resolve the user when sign-in fails', () => {
      client.signIn(request).subscribe({ error: () => undefined });

      httpMock
        .expectOne(API_ROUTES.auth.signIn)
        .flush({ message: 'Invalid credentials' }, { status: 401, statusText: 'Unauthorized' });

      expect(mockAuthStore.reload).not.toHaveBeenCalled();
      expect(mockTokenStore.setAccessToken).not.toHaveBeenCalled();
    });
  });

  describe('signUp', () => {
    const request: SignUpRequest = {
      userName: 'newuser',
      firstName: 'John',
      lastName: 'Doe',
      email: 'john@example.com',
      password: 'password123',
      confirmPassword: 'password123',
    };

    it('should send POST to sign-up URL with request body', () => {
      client.signUp(request).subscribe();

      const req = httpMock.expectOne(API_ROUTES.auth.signUp);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(request);
      req.flush({});
    });

    it('should return User from response', () => {
      const userResponse: User = {
        username: 'newuser',
        firstName: 'John',
        lastName: 'Doe',
        email: 'john@example.com',
      };

      let result: User | undefined;
      client.signUp(request).subscribe(user => (result = user));

      httpMock.expectOne(API_ROUTES.auth.signUp).flush(userResponse);

      expect(result).toEqual(userResponse);
    });
  });

  describe('signOut', () => {
    it('should send POST with withCredentials and X-Client-Type header', () => {
      client.signOut().subscribe();

      const req = httpMock.expectOne(API_ROUTES.auth.signOut);
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBe(true);
      expect(req.request.headers.get('X-Client-Type')).toBe('Web');
      req.flush({});
    });

    it('should clear user on completion', () => {
      client.signOut().subscribe();

      httpMock.expectOne(API_ROUTES.auth.signOut).flush({});

      expect(mockAuthStore.clearUser).toHaveBeenCalled();
    });

    it('should clear user even on error', () => {
      client.signOut().subscribe({ error: () => {} });

      httpMock
        .expectOne(API_ROUTES.auth.signOut)
        .flush(null, { status: 500, statusText: 'Internal Server Error' });

      expect(mockAuthStore.clearUser).toHaveBeenCalled();
    });
  });

  describe('refresh', () => {
    const tokenResponse: Token = { accessToken: 'new-access-token', refreshToken: 'new-refresh-token' };

    it('should send POST to refresh URL with withCredentials', () => {
      client.refresh().subscribe();

      const req = httpMock.expectOne(API_ROUTES.auth.refresh);
      expect(req.request.method).toBe('POST');
      expect(req.request.withCredentials).toBe(true);
      req.flush(tokenResponse);
    });

    it('should store access token on success', () => {
      client.refresh().subscribe();

      httpMock.expectOne(API_ROUTES.auth.refresh).flush(tokenResponse);

      expect(mockTokenStore.setAccessToken).toHaveBeenCalledWith('new-access-token');
    });
  });

  describe('verifyEmail', () => {
    it('should send POST with verify-email body', () => {
      const request: VerifyEmailRequest = { combinedKey: { value: 'verify-key-123' } };

      client.verifyEmail(request).subscribe();

      const req = httpMock.expectOne(API_ROUTES.auth.verifyEmail);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(request);
      req.flush({});
    });
  });

  describe('forgotPassword', () => {
    it('should send POST with email in body', () => {
      client.forgotPassword('john@example.com').subscribe();

      const req = httpMock.expectOne(API_ROUTES.auth.forgotPassword);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ email: 'john@example.com' });
      req.flush({});
    });
  });

  describe('resetPassword', () => {
    it('should send PUT to reset-password URL with request body', () => {
      const request: ResetPasswordRequest = {
        combinedKey: { value: 'reset-key-456' },
        newPassword: 'newPassword123',
        confirmNewPassword: 'newPassword123',
      };

      client.resetPassword(request).subscribe();

      const req = httpMock.expectOne(API_ROUTES.auth.resetPassword);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(request);
      req.flush({});
    });
  });
});
