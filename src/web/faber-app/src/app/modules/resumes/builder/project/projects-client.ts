import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ROUTES } from '../../../../core/routes/api-routes';
import { Project } from './project-response';
import { UpdateProjectRequest } from './update-project-request';

@Injectable({ providedIn: 'root' })
export class ProjectsClient {
  private readonly http = inject(HttpClient);

  create(resumeId: string): Observable<Project> {
    return this.http.post<Project>(API_ROUTES.resumes.projects.root(resumeId), {});
  }

  update(resumeId: string, data: UpdateProjectRequest & { id: string }): Observable<void> {
    return this.http.put<void>(API_ROUTES.resumes.projects.byId(resumeId, data.id), data);
  }

  delete(resumeId: string, id: string): Observable<void> {
    return this.http.delete<void>(API_ROUTES.resumes.projects.byId(resumeId, id));
  }

  reorder(resumeId: string, orderedIds: string[]): Observable<void> {
    return this.http.put<void>(API_ROUTES.resumes.projects.reorder(resumeId), { orderedIds });
  }
}
