import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router, ActivatedRoute } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { of, throwError } from 'rxjs';

import { ResetPassword } from './reset-password';
import { AuthClient } from '../auth-client';
import { APP_ROUTES } from '../../../core/routes/app-routes';

describe('ResetPassword', () => {
  let component: ResetPassword;
  let fixture: ComponentFixture<ResetPassword>;
  let router: Router;

  const mockAuthClient = {
    resetPassword: vi.fn(),
  };

  const mockActivatedRoute = {
    snapshot: { queryParams: { key: 'test-reset-key' } },
  };

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [ResetPassword],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        { provide: AuthClient, useValue: mockAuthClient },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ResetPassword);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate').mockResolvedValue(true);
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have loading false initially', () => {
    expect(component.loading()).toBe(false);
  });

  it('should submit with key from query params', () => {
    mockAuthClient.resetPassword.mockReturnValue(of(null));

    component.form.setValue({ newPassword: 'newPass123', confirmNewPassword: 'newPass123' });
    component.onSubmit();

    expect(mockAuthClient.resetPassword).toHaveBeenCalledWith({
      combinedKey: { value: 'test-reset-key' },
      newPassword: 'newPass123',
      confirmNewPassword: 'newPass123',
    });
  });

  it('should navigate to sign-in on success', () => {
    mockAuthClient.resetPassword.mockReturnValue(of(null));

    component.form.setValue({ newPassword: 'newPass123', confirmNewPassword: 'newPass123' });
    component.onSubmit();

    expect(router.navigate).toHaveBeenCalledWith([APP_ROUTES.auth.signIn]);
  });

  it('should set invalidReset on error', () => {
    mockAuthClient.resetPassword.mockReturnValue(throwError(() => new Error('fail')));

    component.form.setValue({ newPassword: 'newPass123', confirmNewPassword: 'newPass123' });
    component.onSubmit();

    expect(component.invalidReset()).toBe(true);
  });

  it('should mark all controls as touched when submitting an invalid form', () => {
    component.onSubmit();

    expect(component.form.controls.newPassword.touched).toBe(true);
    expect(component.form.controls.confirmNewPassword.touched).toBe(true);
  });
});
