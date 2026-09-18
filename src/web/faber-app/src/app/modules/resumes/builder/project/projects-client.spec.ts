import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_ROUTES } from '../../../../core/routes/api-routes';
import { ProjectsClient } from './projects-client';

describe('ProjectsClient', () => {
  let client: ProjectsClient;
  let httpMock: HttpTestingController;

  const resumeId = 'resume-1';
  const project = {
    id: 'project-1', role: 'Lead developer', name: 'Faber', url: 'https://faber.example',
    startDate: '2025-01-01', endDate: '', description: '<p>Resume builder</p>',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    client = TestBed.inject(ProjectsClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('creates a blank project', () => {
    client.create(resumeId).subscribe(response => expect(response).toEqual({ ...project, order: 0 }));

    const request = httpMock.expectOne(API_ROUTES.resumes.projects.root(resumeId));
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({});
    request.flush({ ...project, order: 0 });
  });

  it('updates a project at its resource route', () => {
    client.update(resumeId, project).subscribe();

    const request = httpMock.expectOne(API_ROUTES.resumes.projects.byId(resumeId, project.id));
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(project);
    request.flush(null);
  });

  it('deletes a project at its resource route', () => {
    client.delete(resumeId, project.id).subscribe();

    const request = httpMock.expectOne(API_ROUTES.resumes.projects.byId(resumeId, project.id));
    expect(request.request.method).toBe('DELETE');
    request.flush(null);
  });

  it('reorders projects with their ordered ids', () => {
    client.reorder(resumeId, ['project-2', project.id]).subscribe();

    const request = httpMock.expectOne(API_ROUTES.resumes.projects.reorder(resumeId));
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ orderedIds: ['project-2', project.id] });
    request.flush(null);
  });
});
