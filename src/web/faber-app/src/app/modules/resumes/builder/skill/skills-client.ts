import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ROUTES } from '../../../../core/routes/api-routes';
import { Skill } from './skill-response';
import { UpdateSkillRequest } from './update-skill-request';

@Injectable({ providedIn: 'root' })
export class SkillsClient {
  private readonly http = inject(HttpClient);

  create(resumeId: string): Observable<Skill> {
    return this.http.post<Skill>(API_ROUTES.resumes.skills.root(resumeId), {});
  }

  update(resumeId: string, data: UpdateSkillRequest & { id: string }): Observable<Skill> {
    return this.http.put<Skill>(API_ROUTES.resumes.skills.byId(resumeId, data.id), data);
  }

  delete(resumeId: string, id: string): Observable<void> {
    return this.http.delete<void>(API_ROUTES.resumes.skills.byId(resumeId, id));
  }

  reorder(resumeId: string, orderedIds: string[]): Observable<void> {
    return this.http.put<void>(API_ROUTES.resumes.skills.reorder(resumeId), { orderedIds });
  }
}
