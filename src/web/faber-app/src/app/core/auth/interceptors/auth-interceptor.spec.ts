import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';

import { authInterceptor } from './auth-interceptor';
import { TokenStore } from '../token-store';
import { RefreshState } from './refresh-state';
import { API_ROUTES } from '../../routes/api-routes';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let mockTokenStore: {
    accessToken: ReturnType<typeof signal<string | null>>;
    setAccessToken: ReturnType<typeof vi.fn>;
    removeAccessToken: ReturnType<typeof vi.fn>;
  };
  let refreshState: RefreshState;

  beforeEach(() => {
    mockTokenStore = {
      accessToken: signal<string | null>(null),
      setAccessToken: vi.fn(),
      removeAccessToken: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: TokenStore, useValue: mockTokenStore },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    refreshState = TestBed.inject(RefreshState);
  });

  afterEach(() => {
    httpMock.verify();
    refreshState.reset();
  });

  it('should attach bearer token when token exists', () => {
    mockTokenStore.accessToken.set('my-token');

    http.get('/api/data').subscribe();

    const req = httpMock.expectOne('/api/data');
    expect(req.request.headers.get('Authorization')).toBe('Bearer my-token');
    req.flush({});
  });

  it('should not attach bearer when no token', () => {
    http.get('/api/data').subscribe();

    const req = httpMock.expectOne('/api/data');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('should not clear token on 500 error', () => {
    http.get('/api/data').subscribe({ error: () => {} });

    httpMock.expectOne('/api/data').flush(null, { status: 500, statusText: 'Server Error' });

    expect(mockTokenStore.removeAccessToken).not.toHaveBeenCalled();
  });

  it('should refresh token and retry on 401', () => {
    mockTokenStore.accessToken.set('expired-token');

    http.get('/api/data').subscribe();

    // Original request fails with 401
    httpMock.expectOne('/api/data').flush(null, { status: 401, statusText: 'Unauthorized' });

    // Interceptor sends refresh request
    const refreshReq = httpMock.expectOne(API_ROUTES.auth.refresh);
    expect(refreshReq.request.withCredentials).toBe(true);
    refreshReq.flush({ accessToken: 'new-token', refreshToken: 'new-rt' });

    // Interceptor retries original request with new token
    const retryReq = httpMock.expectOne('/api/data');
    expect(retryReq.request.headers.get('Authorization')).toBe('Bearer new-token');
    retryReq.flush({ data: 'ok' });

    expect(mockTokenStore.setAccessToken).toHaveBeenCalledWith('new-token');
  });

  it('should clear token when refresh fails (redirect is left to route guards)', () => {
    mockTokenStore.accessToken.set('expired-token');

    http.get('/api/data').subscribe({ error: () => {} });

    httpMock.expectOne('/api/data').flush(null, { status: 401, statusText: 'Unauthorized' });

    httpMock.expectOne(API_ROUTES.auth.refresh).flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(mockTokenStore.removeAccessToken).toHaveBeenCalled();
  });

  it('should queue concurrent 401 requests and replay after refresh', () => {
    mockTokenStore.accessToken.set('expired-token');

    http.get('/api/one').subscribe();
    http.get('/api/two').subscribe();

    // Both get 401
    httpMock.match('/api/one')[0].flush(null, { status: 401, statusText: 'Unauthorized' });
    httpMock.match('/api/two')[0].flush(null, { status: 401, statusText: 'Unauthorized' });

    // Only one refresh request
    const refreshReqs = httpMock.match(API_ROUTES.auth.refresh);
    expect(refreshReqs.length).toBe(1);
    refreshReqs[0].flush({ accessToken: 'fresh-token', refreshToken: 'rt' });

    // Both original requests retried
    const retries = [...httpMock.match('/api/one'), ...httpMock.match('/api/two')];
    expect(retries.length).toBe(2);
    retries.forEach(req => {
      expect(req.request.headers.get('Authorization')).toBe('Bearer fresh-token');
      req.flush({});
    });
  });
});
