import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { TokenStore } from './token-store';
import { User } from './contracts/user';
import { API_ROUTES } from '../routes/api-routes';

@Injectable({
  providedIn: 'root',
})
export class AuthStore {
  private readonly tokenStore = inject(TokenStore);

  /**
   * The only signal the `/auth/me` request computation is allowed to read.
   *
   * `httpResource` rebuilds its `HttpRequest` whenever the computation's
   * dependencies change, and treats an `undefined` request as "no active
   * request" — resetting to `idle` and throwing away whatever it had already
   * resolved. Keeping this counter as the sole dependency means nothing but an
   * explicit `reload()` can re-run the computation, so the resolved user is
   * never silently discarded (issue #426).
   *
   * In particular the access token is deliberately *not* read here: on a hard
   * reload the session lives only in the httpOnly cookie, so a null token is a
   * normal state and not a reason to skip or drop the call.
   */
  private readonly sessionEpoch = signal(0);

  private readonly userResource = httpResource<User>(() => {
    // Tracked purely to key the request on the session epoch.
    this.sessionEpoch();
    return { url: API_ROUTES.auth.me, withCredentials: true };
  });

  readonly user = computed(() => {
    if (this.userResource.status() === 'error') return null;
    return this.userResource.value() ?? null;
  });
  readonly authenticated = computed(() => !!this.user());
  readonly loading = this.userResource.isLoading;

  readonly error = computed(() => {
    const err = this.userResource.error();
    if (!err) return null;
    return err instanceof HttpErrorResponse ? err.message : 'Error getting user.';
  });

  /**
   * Drops the signed-in user locally. Deliberately issues no request: the
   * caller has already ended the session server-side, so re-asking `/auth/me`
   * would only produce a wasted 401.
   */
  clearUser(): void {
    this.tokenStore.removeAccessToken();
    this.userResource.set(undefined);
  }

  /**
   * Re-resolves the current user from `/auth/me`. Bumping the epoch (rather
   * than calling `userResource.reload()`) re-issues the request from every
   * resource status — `reload()` is a no-op while a load is still in flight.
   */
  reload(): void {
    this.sessionEpoch.update((epoch) => epoch + 1);
  }
}
