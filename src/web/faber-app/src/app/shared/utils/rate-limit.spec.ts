import { HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { defer, firstValueFrom, of, throwError } from 'rxjs';

import { isRateLimited, retryAfterMs, retryOnRateLimit } from './rate-limit';

function rateLimited(retryAfter?: string): HttpErrorResponse {
  return new HttpErrorResponse({
    status: 429,
    statusText: 'Too Many Requests',
    headers: new HttpHeaders(retryAfter === undefined ? {} : { 'Retry-After': retryAfter }),
  });
}

describe('isRateLimited', () => {
  it('recognises a 429 HttpErrorResponse', () => {
    expect(isRateLimited(rateLimited('1'))).toBe(true);
  });

  it('rejects other HTTP failures', () => {
    expect(isRateLimited(new HttpErrorResponse({ status: 500 }))).toBe(false);
  });

  it('rejects non-HTTP errors', () => {
    expect(isRateLimited(new Error('boom'))).toBe(false);
    expect(isRateLimited(null)).toBe(false);
  });
});

describe('retryAfterMs', () => {
  it('converts the Retry-After header from seconds to milliseconds', () => {
    expect(retryAfterMs(rateLimited('3'))).toBe(3000);
  });

  it('falls back to 2000ms when the header is missing', () => {
    expect(retryAfterMs(rateLimited())).toBe(2000);
  });

  it('falls back to 2000ms when the header is not a positive number', () => {
    expect(retryAfterMs(rateLimited('Wed, 21 Oct 2026 07:28:00 GMT'))).toBe(2000);
    expect(retryAfterMs(rateLimited('0'))).toBe(2000);
    expect(retryAfterMs(rateLimited('-5'))).toBe(2000);
  });

  it('clamps an excessive Retry-After to 10000ms', () => {
    expect(retryAfterMs(rateLimited('600'))).toBe(10000);
  });
});

describe('retryOnRateLimit', () => {
  beforeEach(() => vi.useFakeTimers());
  afterEach(() => vi.useRealTimers());

  it('retries a rate-limited request and emits the eventual success', async () => {
    let attempts = 0;
    const source = defer(() => {
      attempts++;
      return attempts === 1 ? throwError(() => rateLimited('1')) : of('ok');
    });

    const result = firstValueFrom(source.pipe(retryOnRateLimit()));
    // 1000ms base wait plus up to 500ms of jitter — advance past the ceiling.
    await vi.advanceTimersByTimeAsync(1600);

    await expect(result).resolves.toBe('ok');
    expect(attempts).toBe(2);
  });

  it('waits the server-supplied Retry-After before retrying', async () => {
    let attempts = 0;
    const source = defer(() => {
      attempts++;
      return attempts === 1 ? throwError(() => rateLimited('3')) : of('ok');
    });

    const result = firstValueFrom(source.pipe(retryOnRateLimit()));

    // Still short of even the un-jittered 3000ms wait — proves the delay is genuinely
    // derived from the header, not fired immediately.
    await vi.advanceTimersByTimeAsync(2999);
    expect(attempts).toBe(1);

    // Jitter adds up to another 500ms on top; advance past that ceiling before asserting.
    await vi.advanceTimersByTimeAsync(600);
    await expect(result).resolves.toBe('ok');
    expect(attempts).toBe(2);
  });

  it('gives up after two retries and rethrows the 429', async () => {
    let attempts = 0;
    const source = defer(() => {
      attempts++;
      return throwError(() => rateLimited('1'));
    });

    const result = firstValueFrom(source.pipe(retryOnRateLimit())).catch((e: unknown) => e);
    await vi.advanceTimersByTimeAsync(5000);

    expect(isRateLimited(await result)).toBe(true);
    expect(attempts).toBe(3);
  });

  it('does not retry a non-429 failure', async () => {
    let attempts = 0;
    const source = defer(() => {
      attempts++;
      return throwError(() => new HttpErrorResponse({ status: 500 }));
    });

    const result = firstValueFrom(source.pipe(retryOnRateLimit())).catch((e: unknown) => e);
    await vi.advanceTimersByTimeAsync(5000);

    expect((await result as HttpErrorResponse).status).toBe(500);
    expect(attempts).toBe(1);
  });
});
