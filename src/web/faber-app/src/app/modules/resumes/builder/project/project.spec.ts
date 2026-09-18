import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { By } from '@angular/platform-browser';
import { Project } from './project';
import { Project as ProjectModel } from './project-response';
import { ResumesStore } from '../../resumes-store';
import { Orderly } from '../../orderly';
import { OrderableList } from '../orderable-list/orderable-list';
import { mockResumesStore } from '../../../../shared/testing/mock-resumes-store';
import { expectNoAxeViolations } from '../../../../shared/testing/axe';

describe('Project', () => {
  let fixture: ComponentFixture<Project>;
  let store: Partial<ResumesStore>;

  const seed: ProjectModel[] = [
    {
      id: 'p1', order: 0, name: 'Faber', role: 'Lead developer', url: 'https://faber.example',
      startDate: '2025-01-01', endDate: '', description: '<p>Resume builder</p>',
    },
    {
      id: 'p2', order: 1, name: 'Portfolio', role: 'Designer', url: '',
      startDate: '', endDate: '', description: '',
    },
  ];

  beforeEach(async () => {
    vi.clearAllMocks();
    store = { ...mockResumesStore(), projects: signal(seed) };
    await TestBed.configureTestingModule({
      imports: [Project],
      providers: [{ provide: ResumesStore, useValue: store }],
    }).compileComponents();
    fixture = TestBed.createComponent(Project);
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('creates an accessible project editor with rich text', () => {
    expect(fixture.componentInstance).toBeTruthy();
    expect(fixture.nativeElement.querySelectorAll('form').length).toBe(seed.length);
    expect(fixture.nativeElement.querySelector('fb-rich-text')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('fb-field[type="url"]')).toBeTruthy();
  });

  it('labels a project with its name and role', () => {
    const title = fixture.componentInstance['projectTitle'];
    expect(title(seed[0])).toBe('Faber at Lead developer');
    expect(title({ ...seed[0], name: '', role: 'Designer' })).toBe('Designer');
  });

  it('forwards form changes to the confirmed-state store action', () => {
    fixture.componentInstance['getForm'](seed[0]).patchValue({ url: 'https://updated.example' });
    expect(store.updateProject).toHaveBeenCalledWith(expect.objectContaining({
      id: 'p1', url: 'https://updated.example',
    }));
  });

  it('adds, deletes and reorders projects through the store', () => {
    (fixture.nativeElement.querySelector('button[aria-label="Add project"]') as HTMLButtonElement).click();
    expect(store.addProject).toHaveBeenCalledTimes(1);

    (fixture.nativeElement.querySelector('button[aria-label^="Delete"]') as HTMLButtonElement).click();
    expect(store.deleteProject).toHaveBeenCalledWith(seed[0]);

    const list = fixture.debugElement.query(By.directive(OrderableList))
      .componentInstance as OrderableList<ProjectModel>;
    const event = { previousIndex: 1, currentIndex: 0 } as CdkDragDrop<Orderly[]>;
    list.itemDrop.emit(event);
    expect(store.reorderProjects).toHaveBeenCalledWith(event);
  });

  it('has no AXE violations', async () => {
    await expectNoAxeViolations(fixture);
  }, 20_000);
});
