import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { of, throwError } from 'rxjs';

import { ForgotPassword } from './forgot-password';
import { AuthClient } from '../auth-client';

describe('ForgotPassword', () => {
  let component: ForgotPassword;
  let fixture: ComponentFixture<ForgotPassword>;

  const mockAuthClient = {
    forgotPassword: vi.fn(),
  };

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [ForgotPassword],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        { provide: AuthClient, useValue: mockAuthClient },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ForgotPassword);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have loading false initially', () => {
    expect(component.loading()).toBe(false);
  });

  it('should have sent false initially', () => {
    expect(component.sent()).toBe(false);
  });

  it('should set sent on success', () => {
    mockAuthClient.forgotPassword.mockReturnValue(of(null));

    component.form.controls.email.setValue('john@example.com');
    component.onSubmit();

    expect(component.sent()).toBe(true);
    expect(component.error()).toBe(false);
  });

  it('should set error on failure', () => {
    mockAuthClient.forgotPassword.mockReturnValue(throwError(() => new Error('fail')));

    component.form.controls.email.setValue('john@example.com');
    component.onSubmit();

    expect(component.error()).toBe(true);
    expect(component.sent()).toBe(false);
  });

  it('should reset sent and error on resubmit', () => {
    component.sent.set(true);
    component.error.set(true);
    mockAuthClient.forgotPassword.mockReturnValue(of(null));

    component.form.controls.email.setValue('john@example.com');
    component.onSubmit();

    expect(component.sent()).toBe(true);
    expect(component.error()).toBe(false);
  });
});
