import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { of, throwError } from 'rxjs';

import { SignIn } from './sign-in';
import { AuthClient } from '../auth-client';
import { APP_ROUTES } from '../../../core/routes/app-routes';

describe('SignIn', () => {
  let component: SignIn;
  let fixture: ComponentFixture<SignIn>;
  let router: Router;

  const mockAuthClient = {
    signIn: vi.fn(),
  };

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [SignIn],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        { provide: AuthClient, useValue: mockAuthClient },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SignIn);
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

  it('should have invalidLogin false initially', () => {
    expect(component.invalidLogin()).toBe(false);
  });

  it('should not call signIn when form is invalid', () => {
    component.onSubmit();
    expect(mockAuthClient.signIn).not.toHaveBeenCalled();
  });

  it('should navigate to home on successful sign-in', () => {
    mockAuthClient.signIn.mockReturnValue(of({ accessToken: 'token', refreshToken: 'rt' }));

    component.form.setValue({ userName: 'testuser', password: 'password123' });
    component.onSubmit();

    expect(mockAuthClient.signIn).toHaveBeenCalledWith({ userName: 'testuser', password: 'password123' });
    expect(router.navigate).toHaveBeenCalledWith([APP_ROUTES.home]);
  });

  it('should set invalidLogin on error', () => {
    mockAuthClient.signIn.mockReturnValue(throwError(() => new Error('Unauthorized')));

    component.form.setValue({ userName: 'testuser', password: 'password123' });
    component.onSubmit();

    expect(component.invalidLogin()).toBe(true);
  });

  it('should mark all controls as touched when submitting an invalid form', () => {
    component.onSubmit();

    expect(component.form.controls.userName.touched).toBe(true);
    expect(component.form.controls.password.touched).toBe(true);
  });
});
