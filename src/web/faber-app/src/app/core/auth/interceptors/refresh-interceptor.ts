import { HttpInterceptorFn } from '@angular/common/http';
import { API_ROUTES } from '../../routes/api-routes';

export const refreshInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.url === API_ROUTES.auth.refresh) {
    return next(req.clone({ headers: req.headers.set('X-Client-Type', 'Web') }));
  }
  return next(req);
};
