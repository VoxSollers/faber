import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ROUTES } from '../../../../core/routes/api-routes';
import { Education } from './education-response';
import { UpdateEducationRequest } from './update-education-request';

@Injectable({ providedIn: 'root' })
export class EducationsClient {
  private readonly http = inject(HttpClient);

  create(resumeId: string): Observable<Education> {
    return this.http.post<Education>(API_ROUTES.resumes.educations.root(resumeId), {});
  }

  update(resumeId: string, data: UpdateEducationRequest & { id: string }): Observable<Education> {
    return this.http.put<Education>(API_ROUTES.resumes.educations.byId(resumeId, data.id), data);
  }

  delete(resumeId: string, id: string): Observable<void> {
    return this.http.delete<void>(API_ROUTES.resumes.educations.byId(resumeId, id));
  }

  reorder(resumeId: string, orderedIds: string[]): Observable<void> {
    return this.http.put<void>(API_ROUTES.resumes.educations.reorder(resumeId), { orderedIds });
  }
}
