import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ROUTES } from '../../../../core/routes/api-routes';
import { Experience } from './experience-response';
import { UpdateExperienceRequest } from './update-experience-request';

@Injectable({ providedIn: 'root' })
export class ExperiencesClient {
  private readonly http = inject(HttpClient);

  create(resumeId: string): Observable<Experience> {
    return this.http.post<Experience>(API_ROUTES.resumes.experience.root(resumeId), {});
  }

  update(resumeId: string, data: UpdateExperienceRequest & { id: string }): Observable<Experience> {
    return this.http.put<Experience>(
      API_ROUTES.resumes.experience.byId(resumeId, data.id),
      data,
    );
  }

  delete(resumeId: string, id: string): Observable<void> {
    return this.http.delete<void>(API_ROUTES.resumes.experience.byId(resumeId, id));
  }

  reorder(resumeId: string, orderedIds: string[]): Observable<void> {
    return this.http.put<void>(API_ROUTES.resumes.experience.reorder(resumeId), { orderedIds });
  }
}
