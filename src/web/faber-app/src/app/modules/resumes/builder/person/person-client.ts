import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_ROUTES } from '../../../../core/routes/api-routes';
import { Person } from './person-response';
import { UpdatePersonRequest } from './update-person-request';

@Injectable({ providedIn: 'root' })
export class PersonClient {
  private readonly http = inject(HttpClient);

  getByResumeId(resumeId: string): Observable<Person> {
    return this.http.get<Person>(API_ROUTES.resumes.persons.root(resumeId));
  }

  create(resumeId: string, data: UpdatePersonRequest): Observable<Person> {
    return this.http.post<Person>(API_ROUTES.resumes.persons.root(resumeId), data);
  }

  update(resumeId: string, personId: string, data: UpdatePersonRequest): Observable<void> {
    return this.http.put<void>(API_ROUTES.resumes.persons.byId(resumeId, personId), data);
  }
}
