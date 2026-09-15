import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  effect,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthStore } from '../../../core/auth/auth-store';
import { UsersClient } from '../users-client';
import { FbButton } from '../../../shared/components/fb-button/fb-button';
import { FbInput } from '../../../shared/components/fb-input/fb-input';
import { FbToast } from '../../../shared/components/fb-toast/fb-toast';

@Component({
  selector: 'app-account',
  imports: [ReactiveFormsModule, FbButton, FbInput, FbToast],
  templateUrl: './account.html',
  styleUrl: './account.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Account implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly authStore = inject(AuthStore);
  private readonly usersClient = inject(UsersClient);
  private readonly destroyRef = inject(DestroyRef);

  readonly user = this.authStore.user;
  readonly loading = signal(false);
  readonly saved = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
  });

  constructor() {
    effect(() => {
      const user = this.user();
      if (user && this.form.pristine) {
        // Sync from the store without emitting valueChanges: resetSavedOnChange()
        // must react to real user edits only. Otherwise the AuthStore.reload()
        // that follows a successful save immediately clears the success state.
        this.form.patchValue(
          { firstName: user.firstName, lastName: user.lastName },
          { emitEvent: false },
        );
      }
    });
  }

  ngOnInit(): void {
    this.resetSavedOnChange();
  }

  private resetSavedOnChange(): void {
    this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.saved.set(false));
  }

  onSubmit(): void {
    const user = this.user();
    if (this.form.invalid) return;
    if (!user?.id) {
      this.error.set('Failed to update profile. Please try again.');
      return;
    }

    this.loading.set(true);
    this.saved.set(false);
    this.error.set(null);

    this.usersClient
      .updateProfile(user.id, this.form.getRawValue())
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => {
          this.authStore.reload();
          this.form.markAsPristine();
          this.saved.set(true);
        },
        error: () => this.error.set('Failed to update profile. Please try again.'),
      });
  }
}
