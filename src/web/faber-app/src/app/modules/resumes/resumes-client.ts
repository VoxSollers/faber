import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { API_ROUTES } from '../../core/routes/api-routes';
import { Resume } from './resume-response';

@Injectable({ providedIn: 'root' })
export class ResumesClient {
  private readonly http = inject(HttpClient);

  getAll(): Observable<Resume[]> {
    return this.http.get<{ items: Resume[] }>(API_ROUTES.resumes.root).pipe(map(r => r.items));
  }

  getById(id: string): Observable<Resume> {
    return this.http.get<Resume>(API_ROUTES.resumes.byId(id));
  }

  create(): Observable<Resume> {
    return this.http.post<Resume>(API_ROUTES.resumes.root, { localization: 'en-us' });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(API_ROUTES.resumes.byId(id));
  }

  generate(id: string): Observable<string> {
    return this.http
      .post<{ base64Pdf: string }>(API_ROUTES.resumes.generate(id), {})
      .pipe(map(r => r.base64Pdf));
  }

  download(id: string): Observable<Blob> {
    return this.http.get(API_ROUTES.resumes.download(id), { responseType: 'blob' });
  }

  updateTitle(id: string, value: string): Observable<void> {
    return this.http.put<void>(API_ROUTES.resumes.title(id), { title: value });
  }

  updateSummary(id: string, value: string): Observable<void> {
    return this.http.put<void>(API_ROUTES.resumes.summary(id), { summary: value });
  }

  updateHobbies(id: string, value: string): Observable<void> {
    return this.http.put<void>(API_ROUTES.resumes.hobbies(id), { hobbies: value });
  }

  updateLocalization(id: string, value: string): Observable<void> {
    return this.http.put<void>(API_ROUTES.resumes.localization(id), { localization: value });
  }
}
