import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { ResumesClient } from './resumes-client';
import { API_ROUTES } from '../../core/routes/api-routes';

describe('ResumesClient single-field updates', () => {
  let client: ResumesClient;
  let httpMock: HttpTestingController;
  const id = 'resume-1';

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    client = TestBed.inject(ResumesClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('updateSummary sends the backend-contract field "summary"', () => {
    client.updateSummary(id, '<p>hello</p>').subscribe();

    const req = httpMock.expectOne(API_ROUTES.resumes.summary(id));
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ summary: '<p>hello</p>' });
    req.flush(null);
  });

  it('updateHobbies sends the backend-contract field "hobbies"', () => {
    client.updateHobbies(id, 'reading').subscribe();

    const req = httpMock.expectOne(API_ROUTES.resumes.hobbies(id));
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ hobbies: 'reading' });
    req.flush(null);
  });

  it('updateLocalization sends the backend-contract field "localization"', () => {
    client.updateLocalization(id, 'en-us').subscribe();

    const req = httpMock.expectOne(API_ROUTES.resumes.localization(id));
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ localization: 'en-us' });
    req.flush(null);
  });
});
