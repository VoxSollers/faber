import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from '../auth-store';
import { APP_ROUTES } from '../../routes/app-routes';
import { toObservable } from '@angular/core/rxjs-interop';
import { filter, map, take } from 'rxjs';

export const notAuthGuard: CanActivateFn = () => {
  const authStore = inject(AuthStore);
  const router = inject(Router);

  if (!authStore.authenticated() && !authStore.loading()) {
    return true;
  }

  return toObservable(authStore.loading).pipe(
    filter((loading) => !loading),
    take(1),
    map(() => {
      if (!authStore.authenticated()) {
        return true;
      }
      return router.createUrlTree([APP_ROUTES.resumes]);
    }),
  );
};
