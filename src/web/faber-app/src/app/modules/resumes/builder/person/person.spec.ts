import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Person } from './person';
import { ResumesStore } from '../../resumes-store';
import { mockResumesStore } from '../../../../shared/testing/mock-resumes-store';

describe('Person', () => {
  let component: Person;
  let fixture: ComponentFixture<Person>;

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [Person],
      providers: [
        { provide: ResumesStore, useValue: mockResumesStore() },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Person);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('sizes entry fields so a second column only appears once there is room for it', () => {
    const form = fixture.nativeElement.querySelector('form') as HTMLElement;

    expect(form.classList.contains('grid-cols-[repeat(auto-fit,minmax(14rem,1fr))]')).toBe(true);
    // Both the old container-query and viewport-prefixed variants ignored the
    // form's own rendered width, nested inside accordion padding; neither
    // should remain.
    expect(form.classList.contains('@sm:grid-cols-2')).toBe(false);
    expect(form.classList.contains('sm:grid-cols-2')).toBe(false);
  });
});
