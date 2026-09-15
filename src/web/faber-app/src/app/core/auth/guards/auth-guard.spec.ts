import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, provideRouter, RouterStateSnapshot, UrlTree } from '@angular/router';
import { signal } from '@angular/core';
import { lastValueFrom, Observable } from 'rxjs';

import { authGuard } from './auth-guard';
import { AuthStore } from '../auth-store';
import { APP_ROUTES } from '../../routes/app-routes';

describe('authGuard', () => {
  let mockAuthStore: {
    authenticated: ReturnType<typeof signal<boolean>>;
    loading: ReturnType<typeof signal<boolean>>;
  };

  beforeEach(() => {
    mockAuthStore = {
      authenticated: signal<boolean>(false),
      loading: signal<boolean>(false),
    };

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthStore, useValue: mockAuthStore },
      ],
    });
  });

  it('should return true when authenticated', () => {
    mockAuthStore.authenticated.set(true);

    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as unknown as ActivatedRouteSnapshot, {} as unknown as RouterStateSnapshot),
    );

    expect(result).toBe(true);
  });

  it('should redirect to sign-in when not authenticated and not loading', async () => {
    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as unknown as ActivatedRouteSnapshot, {} as unknown as RouterStateSnapshot),
    );

    const value = result instanceof Observable ? await lastValueFrom(result) : result;
    expect(value).toBeInstanceOf(UrlTree);
    expect((value as UrlTree).toString()).toBe(APP_ROUTES.auth.signIn);
  });

  it('should wait for loading to finish', async () => {
    mockAuthStore.loading.set(true);

    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as unknown as ActivatedRouteSnapshot, {} as unknown as RouterStateSnapshot),
    );

    expect(result).not.toBe(true);

    mockAuthStore.authenticated.set(true);
    mockAuthStore.loading.set(false);

    const value = await lastValueFrom(result as Observable<boolean | UrlTree>);
    expect(value).toBe(true);
  });
});
