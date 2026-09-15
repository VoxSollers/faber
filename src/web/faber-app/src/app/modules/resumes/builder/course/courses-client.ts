import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ROUTES } from '../../../../core/routes/api-routes';
import { Course } from './course-response';
import { UpdateCourseRequest } from './update-course-request';

@Injectable({ providedIn: 'root' })
export class CoursesClient {
  private readonly http = inject(HttpClient);

  create(resumeId: string): Observable<Course> {
    return this.http.post<Course>(API_ROUTES.resumes.courses.root(resumeId), {});
  }

  update(resumeId: string, data: UpdateCourseRequest & { id: string }): Observable<Course> {
    return this.http.put<Course>(API_ROUTES.resumes.courses.byId(resumeId, data.id), data);
  }

  delete(resumeId: string, id: string): Observable<void> {
    return this.http.delete<void>(API_ROUTES.resumes.courses.byId(resumeId, id));
  }

  reorder(resumeId: string, orderedIds: string[]): Observable<void> {
    return this.http.put<void>(API_ROUTES.resumes.courses.reorder(resumeId), { orderedIds });
  }
}
