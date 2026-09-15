import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ROUTES } from '../../../../core/routes/api-routes';
import { Language } from './language-response';
import { UpdateLanguageRequest } from './update-language-request';

@Injectable({ providedIn: 'root' })
export class LanguagesClient {
  private readonly http = inject(HttpClient);

  create(resumeId: string): Observable<Language> {
    return this.http.post<Language>(API_ROUTES.resumes.languages.root(resumeId), {});
  }

  update(resumeId: string, data: UpdateLanguageRequest & { id: string }): Observable<Language> {
    return this.http.put<Language>(API_ROUTES.resumes.languages.byId(resumeId, data.id), data);
  }

  delete(resumeId: string, id: string): Observable<void> {
    return this.http.delete<void>(API_ROUTES.resumes.languages.byId(resumeId, id));
  }

  reorder(resumeId: string, orderedIds: string[]): Observable<void> {
    return this.http.put<void>(API_ROUTES.resumes.languages.reorder(resumeId), { orderedIds });
  }
}
