import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, provideRouter, RouterStateSnapshot, UrlTree } from '@angular/router';
import { signal } from '@angular/core';
import { lastValueFrom, Observable } from 'rxjs';

import { notAuthGuard } from './not-auth-guard';
import { AuthStore } from '../auth-store';
import { APP_ROUTES } from '../../routes/app-routes';

describe('notAuthGuard', () => {
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

  it('should return true when not authenticated', async () => {
    const result = TestBed.runInInjectionContext(() =>
      notAuthGuard({} as unknown as ActivatedRouteSnapshot, {} as unknown as RouterStateSnapshot),
    );

    const value = result instanceof Observable ? await lastValueFrom(result) : result;
    expect(value).toBe(true);
  });

  it('should redirect to resumes when authenticated', async () => {
    mockAuthStore.authenticated.set(true);

    const result = TestBed.runInInjectionContext(() =>
      notAuthGuard({} as unknown as ActivatedRouteSnapshot, {} as unknown as RouterStateSnapshot),
    );

    const value = result instanceof Observable ? await lastValueFrom(result) : result;
    expect(value).toBeInstanceOf(UrlTree);
    expect((value as UrlTree).toString()).toBe(APP_ROUTES.resumes);
  });

  it('should wait for loading to finish', async () => {
    mockAuthStore.loading.set(true);

    const result = TestBed.runInInjectionContext(() =>
      notAuthGuard({} as unknown as ActivatedRouteSnapshot, {} as unknown as RouterStateSnapshot),
    );

    expect(result).not.toBe(true);

    mockAuthStore.authenticated.set(false);
    mockAuthStore.loading.set(false);

    const value = await lastValueFrom(result as Observable<boolean | UrlTree>);
    expect(value).toBe(true);
  });
});
