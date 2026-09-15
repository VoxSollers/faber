import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';

import { Account } from './account';
import { AuthStore } from '../../../core/auth/auth-store';
import { UsersClient } from '../users-client';
import { User } from '../../../core/auth/contracts/user';
import { expectNoAxeViolations } from '../../../shared/testing/axe';

describe('Account', () => {
  let component: Account;
  let fixture: ComponentFixture<Account>;

  const mockUser: User = {
    id: 'user-1',
    username: 'johndoe',
    firstName: 'John',
    lastName: 'Doe',
    email: 'john@example.com',
  };

  const mockAuthStore = {
    user: signal<User | null>(mockUser),
    reload: vi.fn(),
  };

  const mockUsersClient = {
    updateProfile: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
    mockAuthStore.user.set(mockUser);
  });

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Account],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        { provide: AuthStore, useValue: mockAuthStore },
        { provide: UsersClient, useValue: mockUsersClient },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Account);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have loading false initially', () => {
    expect(component.loading()).toBe(false);
  });

  it('should pre-fill form with user firstName and lastName', () => {
    expect(component.form.value.firstName).toBe('John');
    expect(component.form.value.lastName).toBe('Doe');
  });

  describe('onSubmit', () => {
    it('should not call updateProfile when form is invalid', () => {
      component.form.controls.firstName.setValue('');
      component.onSubmit();
      expect(mockUsersClient.updateProfile).not.toHaveBeenCalled();
    });

    it('should call updateProfile and set saved on success', async () => {
      mockUsersClient.updateProfile.mockReturnValue(of({ firstName: 'John', lastName: 'Doe' }));

      component.onSubmit();
      await fixture.whenStable();

      expect(mockUsersClient.updateProfile).toHaveBeenCalledWith('user-1', { firstName: 'John', lastName: 'Doe' });
      expect(mockAuthStore.reload).toHaveBeenCalled();
      expect(component.saved()).toBe(true);
      expect(component.loading()).toBe(false);
    });

    it('should mark form pristine after successful save', async () => {
      mockUsersClient.updateProfile.mockReturnValue(of({ firstName: 'John', lastName: 'Doe' }));

      component.form.markAsDirty();
      component.onSubmit();
      await fixture.whenStable();

      expect(component.form.pristine).toBe(true);
    });

    it('should clear saved message when form value changes after save', async () => {
      mockUsersClient.updateProfile.mockReturnValue(of({ firstName: 'John', lastName: 'Doe' }));

      component.onSubmit();
      await fixture.whenStable();
      expect(component.saved()).toBe(true);

      component.form.controls.firstName.setValue('Jane');

      expect(component.saved()).toBe(false);
    });

    it('should set error signal on HTTP failure', async () => {
      mockUsersClient.updateProfile.mockReturnValue(throwError(() => new Error('500')));

      component.onSubmit();
      await fixture.whenStable();

      expect(component.error()).toBe('Failed to update profile. Please try again.');
      expect(component.saved()).toBe(false);
      expect(component.loading()).toBe(false);
    });

    it('should keep saved true when the user signal refreshes after a successful save', async () => {
      mockUsersClient.updateProfile.mockReturnValue(of({ firstName: 'Jane', lastName: 'Doe' }));

      component.form.controls.firstName.setValue('Jane');
      component.onSubmit();
      await fixture.whenStable();
      expect(component.saved()).toBe(true);

      // authStore.reload() resolves: the user signal emits the freshly persisted
      // profile, which re-runs the sync effect against the now-pristine form.
      mockAuthStore.user.set({ ...mockUser, firstName: 'Jane' });
      fixture.detectChanges();
      await fixture.whenStable();

      expect(component.saved()).toBe(true);
      expect(component.form.value.firstName).toBe('Jane');
    });
  });

  describe('template (fb-input migration)', () => {
    it('should render both name fields as fb-input', () => {
      fixture.detectChanges();
      const fbInputs = fixture.nativeElement.querySelectorAll('fb-input');
      expect(fbInputs.length).toBe(2);
      expect(fixture.nativeElement.querySelectorAll('form input').length).toBe(2);
    });

    it('should not use raw error classes from the old markup', () => {
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('.border-red-500')).toBeNull();
      expect(fixture.nativeElement.querySelector('form .bg-white')).toBeNull();
    });

    it('should show required error via fb-input when first name is cleared and touched', async () => {
      fixture.detectChanges();
      component.form.controls.firstName.setValue('');
      component.form.controls.firstName.markAsTouched();
      fixture.detectChanges();
      await fixture.whenStable();
      const errors = Array.from<Element>(fixture.nativeElement.querySelectorAll('.fb-error'));
      expect(errors.map(el => el.textContent?.trim())).toContain('First name is required.');
    });

    it('should have no AXE violations (light, error state)', async () => {
      component.form.controls.firstName.setValue('');
      component.form.controls.firstName.markAsTouched();
      fixture.detectChanges();
      await fixture.whenStable();
      await expectNoAxeViolations(fixture);
    }, 20000);

    it('should have no AXE violations (dark, error state)', async () => {
      document.documentElement.classList.add('dark');
      try {
        component.form.controls.firstName.setValue('');
        component.form.controls.firstName.markAsTouched();
        fixture.detectChanges();
        await fixture.whenStable();
        await expectNoAxeViolations(fixture);
      } finally {
        document.documentElement.classList.remove('dark');
      }
    }, 20000);
  });

  describe('template (toast feedback)', () => {
    it('should render no toast before any save attempt', () => {
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('fb-toast [role="alert"]')).toBeNull();
    });

    it('should show a success toast after a successful save', async () => {
      mockUsersClient.updateProfile.mockReturnValue(of({ firstName: 'John', lastName: 'Doe' }));

      component.onSubmit();
      await fixture.whenStable();
      fixture.detectChanges();

      const pill = fixture.nativeElement.querySelector('fb-toast .text-success-text');
      expect(pill).not.toBeNull();
      expect(pill.textContent).toContain('Profile updated successfully.');
    });

    it('should show an error toast when the save fails', async () => {
      mockUsersClient.updateProfile.mockReturnValue(throwError(() => new Error('500')));

      component.onSubmit();
      await fixture.whenStable();
      fixture.detectChanges();

      const pill = fixture.nativeElement.querySelector('fb-toast .text-destructive-text');
      expect(pill).not.toBeNull();
      expect(pill.textContent).toContain('Failed to update profile. Please try again.');
    });

    it('should not render inline status paragraphs inside the form', async () => {
      mockUsersClient.updateProfile.mockReturnValue(of({ firstName: 'John', lastName: 'Doe' }));

      component.onSubmit();
      await fixture.whenStable();
      fixture.detectChanges();

      expect(fixture.nativeElement.querySelector('form p[role="status"]')).toBeNull();
      expect(fixture.nativeElement.querySelector('form p[role="alert"]')).toBeNull();
    });

    it('should clear the success toast when it is dismissed', async () => {
      mockUsersClient.updateProfile.mockReturnValue(of({ firstName: 'John', lastName: 'Doe' }));

      component.onSubmit();
      await fixture.whenStable();
      fixture.detectChanges();

      fixture.nativeElement
        .querySelector('fb-toast .text-success-text button[aria-label="Dismiss"]')
        .click();
      fixture.detectChanges();

      expect(component.saved()).toBe(false);
      expect(fixture.nativeElement.querySelector('fb-toast .text-success-text')).toBeNull();
    });

    it('should clear the error toast when it is dismissed', async () => {
      mockUsersClient.updateProfile.mockReturnValue(throwError(() => new Error('500')));

      component.onSubmit();
      await fixture.whenStable();
      fixture.detectChanges();

      fixture.nativeElement
        .querySelector('fb-toast .text-destructive-text button[aria-label="Dismiss"]')
        .click();
      fixture.detectChanges();

      expect(component.error()).toBeNull();
      expect(fixture.nativeElement.querySelector('fb-toast .text-destructive-text')).toBeNull();
    });

    it('should have no AXE violations while the success toast is visible', async () => {
      mockUsersClient.updateProfile.mockReturnValue(of({ firstName: 'John', lastName: 'Doe' }));

      component.onSubmit();
      await fixture.whenStable();
      fixture.detectChanges();

      await expectNoAxeViolations(fixture);
    }, 20000);
  });

  describe('regression #422 (real /auth/me payload shape)', () => {
    it('should render the username from a payload whose key is "username"', async () => {
      // The API serializes MeResponse with the default camelCase policy:
      // the JSON key is "username" (one word), not "userName". Build the
      // object from raw JSON so the frontend contract spelling cannot leak in.
      const apiPayload = JSON.parse(
        '{"id":"user-1","username":"johndoe","email":"john@example.com","firstName":"John","lastName":"Doe"}',
      ) as User;
      mockAuthStore.user.set(apiPayload);
      fixture.detectChanges();
      await fixture.whenStable();

      const spans = Array.from<HTMLElement>(fixture.nativeElement.querySelectorAll('span'));
      const usernameLabel = spans.find(el => el.textContent?.trim() === 'Username');
      const valueSpan = usernameLabel?.nextElementSibling as HTMLElement | null;
      expect(valueSpan?.textContent?.trim()).toBe('johndoe');
    });
  });
});
