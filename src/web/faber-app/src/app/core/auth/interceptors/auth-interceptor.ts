import { inject } from '@angular/core';
import {
  HttpClient,
  HttpErrorResponse,
  HttpEvent,
  HttpInterceptorFn,
  HttpRequest,
  HttpHandlerFn,
} from '@angular/common/http';
import { catchError, filter, Observable, switchMap, take, throwError } from 'rxjs';
import { TokenStore } from '../token-store';
import { Token } from '../contracts/token';
import { API_ROUTES } from '../../routes/api-routes';
import { RefreshState } from './refresh-state';

function addBearer(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

function handle401(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  tokenStore: TokenStore,
  http: HttpClient,
  refreshState: RefreshState,
): Observable<HttpEvent<unknown>> {
  if (refreshState.isRefreshing) {
    return refreshState.refreshSubject.pipe(
      filter((t): t is string => t !== null),
      take(1),
      switchMap((token) => next(addBearer(req, token))),
    );
  }

  refreshState.isRefreshing = true;
  refreshState.refreshSubject.next(null);

  return http.post<Token>(API_ROUTES.auth.refresh, {}, { withCredentials: true }).pipe(
    switchMap((token) => {
      refreshState.isRefreshing = false;
      tokenStore.setAccessToken(token.accessToken);
      refreshState.refreshSubject.next(token.accessToken);
      return next(addBearer(req, token.accessToken));
    }),
    catchError((err) => {
      refreshState.isRefreshing = false;
      tokenStore.removeAccessToken();
      return throwError(() => err);
    }),
  );
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenStore = inject(TokenStore);
  const http = inject(HttpClient);
  const refreshState = inject(RefreshState);

  const token = tokenStore.accessToken();
  const isRefreshRequest = req.url === API_ROUTES.auth.refresh;
  const authReq = token && !isRefreshRequest ? addBearer(req, token) : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse): Observable<HttpEvent<unknown>> => {
      if (error.status === 401 && !isRefreshRequest) {
        const currentToken = tokenStore.accessToken();
        if (currentToken !== token && currentToken !== null) {
          return next(addBearer(req, currentToken));
        }

        return handle401(req, next, tokenStore, http, refreshState);
      }
      return throwError(() => error);
    }),
  );
};
