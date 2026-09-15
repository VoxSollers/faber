import { HttpErrorResponse } from '@angular/common/http';
import { MonoTypeOperatorFunction, throwError, timer } from 'rxjs';
import { retry } from 'rxjs/operators';

/** Wait applied when a 429 carries no usable `Retry-After` header. */
const FALLBACK_RETRY_AFTER_MS = 2000;

/** Upper bound on a server-suggested wait, so a large `Retry-After` cannot stall the UI. */
const MAX_RETRY_AFTER_MS = 10000;

/** Attempts made after the first rejection before the caller sees the 429. */
const MAX_RATE_LIMIT_RETRIES = 2;

/**
 * Upper bound of random jitter added on top of the server's `Retry-After` wait. The issue's own
 * motivating scenario is several tabs generating a preview at once — without jitter they are all
 * throttled at the same moment and retry in perfect lockstep, re-contending for the same limited
 * budget and re-triggering the next 429 together. A small random spread desynchronises them.
 */
const MAX_RETRY_JITTER_MS = 500;

/**
 * Narrows a caught value to the backend's throttled-request response — the 429
 * `application/problem+json` produced by `RateLimitRejection.Problem` on the API side.
 */
export function isRateLimited(error: unknown): error is HttpErrorResponse {
  return error instanceof HttpErrorResponse && error.status === 429;
}

/**
 * Milliseconds to wait before retrying a rate-limited request. The API always sends
 * `Retry-After` in whole seconds (the failing lease's window reset), so it is honoured as-is
 * rather than guessed at — but clamped, because a caller must never freeze on a hostile or
 * misconfigured value, and floored to a sane default when the header is missing or unparseable.
 */
export function retryAfterMs(error: HttpErrorResponse): number {
  const header = error.headers.get('Retry-After');
  const seconds = Number(header);

  if (header === null || !Number.isFinite(seconds) || seconds <= 0) {
    return FALLBACK_RETRY_AFTER_MS;
  }

  return Math.min(seconds * 1000, MAX_RETRY_AFTER_MS);
}

/**
 * Retries a request the server throttled, waiting the `Retry-After` it asked for, and gives up
 * after `MAX_RATE_LIMIT_RETRIES` so a sustained limit surfaces to the caller instead of looping.
 * Any other failure is rethrown untouched — this operator only knows about rate limiting.
 *
 * A 429 means the request was rejected before it ran, so retrying is safe even for the
 * non-idempotent create endpoints.
 */
export function retryOnRateLimit<T>(): MonoTypeOperatorFunction<T> {
  return retry<T>({
    count: MAX_RATE_LIMIT_RETRIES,
    delay: (error: unknown) =>
      isRateLimited(error)
        ? timer(retryAfterMs(error) + Math.random() * MAX_RETRY_JITTER_MS)
        : throwError(() => error),
  });
}
