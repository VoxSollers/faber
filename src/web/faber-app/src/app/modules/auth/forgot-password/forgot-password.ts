import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { LUCIDE_ICONS, LucideAngularModule, LucideIconProvider, X } from 'lucide-angular';
import { AuthClient } from '../auth-client';
import { FbInput } from '../../../shared/components/fb-input/fb-input';
import { FbAuthCard } from '../../../shared/components/fb-auth-card/fb-auth-card';
import { FbButton } from '../../../shared/components/fb-button/fb-button';
import { FbIconButton } from '../../../shared/components/fb-icon-button/fb-icon-button';
import { FbToast } from '../../../shared/components/fb-toast/fb-toast';
import { FbThemeToggle } from '../../../shared/components/fb-theme-toggle/fb-theme-toggle';

@Component({
  selector: 'app-forgot-password',
  imports: [ReactiveFormsModule, RouterLink, FbInput, FbAuthCard, FbButton, FbIconButton, FbThemeToggle, FbToast, LucideAngularModule],
  providers: [{ provide: LUCIDE_ICONS, multi: true, useValue: new LucideIconProvider({ X }) }],
  templateUrl: './forgot-password.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'flex w-full justify-center' },
})
export class ForgotPassword {
  private readonly authClient = inject(AuthClient);
  private readonly fb = inject(NonNullableFormBuilder);

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  readonly loading = signal(false);
  readonly sent = signal(false);
  readonly error = signal(false);

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.sent.set(false);
    this.error.set(false);

    this.authClient
      .forgotPassword(this.form.controls.email.value)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => this.sent.set(true),
        error: () => this.error.set(true),
      });
  }
}
