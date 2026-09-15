import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ROUTES } from '../../../../core/routes/api-routes';
import { Link } from './link-response';
import { UpdateLinkRequest } from './update-link-request';

@Injectable({ providedIn: 'root' })
export class LinksClient {
  private readonly http = inject(HttpClient);

  create(resumeId: string): Observable<Link> {
    return this.http.post<Link>(API_ROUTES.resumes.links.root(resumeId), {});
  }

  update(resumeId: string, data: UpdateLinkRequest & { id: string }): Observable<Link> {
    return this.http.put<Link>(API_ROUTES.resumes.links.byId(resumeId, data.id), data);
  }

  delete(resumeId: string, id: string): Observable<void> {
    return this.http.delete<void>(API_ROUTES.resumes.links.byId(resumeId, id));
  }

  reorder(resumeId: string, orderedIds: string[]): Observable<void> {
    return this.http.put<void>(API_ROUTES.resumes.links.reorder(resumeId), { orderedIds });
  }
}
