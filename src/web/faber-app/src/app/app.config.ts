import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { PreloadAllModules, provideRouter, withPreloading, withViewTransitions } from '@angular/router';

import { routes } from './app.routes';
import { refreshInterceptor } from './core/auth/interceptors/refresh-interceptor';
import { authInterceptor } from './core/auth/interceptors/auth-interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withPreloading(PreloadAllModules), withViewTransitions()),
    provideHttpClient(withInterceptors([refreshInterceptor, authInterceptor])),
  ],
};
