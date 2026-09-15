import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStore } from '../../core/auth/auth-store';
import { APP_ROUTES } from '../../core/routes/app-routes';
@Component({
  selector: 'app-home',
  imports: [RouterLink],
  templateUrl: './home.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'block' },
})
export class Home {
  private readonly authStore = inject(AuthStore);

  link = computed(() =>
    this.authStore.authenticated() ? APP_ROUTES.resumes : APP_ROUTES.auth.signIn,
  );
}
