import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Course } from './course';
import { Course as CourseModel } from './course-response';
import { ResumesStore } from '../../resumes-store';
import { Orderly } from '../../orderly';
import { OrderableList } from '../orderable-list/orderable-list';
import { mockResumesStore } from '../../../../shared/testing/mock-resumes-store';
import { expectNoAxeViolations } from '../../../../shared/testing/axe';

describe('Course', () => {
  let fixture: ComponentFixture<Course>;
  let store: Partial<ResumesStore>;

  const seed: CourseModel[] = [
    {
      id: 'c1', order: 0, name: 'Advanced React', school: 'Frontend Masters',
      startDate: '2021-02-01', endDate: '2021-04-01', description: '',
    },
    {
      id: 'c2', order: 1, name: 'Rust Fundamentals', school: 'Udemy',
      startDate: '2022-01-01', endDate: '', description: '',
    },
  ];

  beforeEach(async () => {
    vi.clearAllMocks();
    store = { ...mockResumesStore(), courses: signal(seed) };

    await TestBed.configureTestingModule({
      imports: [Course],
      providers: [{ provide: ResumesStore, useValue: store }],
    }).compileComponents();

    fixture = TestBed.createComponent(Course);
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('sizes entry fields so a second column only appears once there is room for it', () => {
    const rows = Array.from(
      fixture.nativeElement.querySelectorAll('form > div.grid'),
    ) as HTMLElement[];
    // Two grid rows per entry: name/institution, then the date pair. The date
    // row uses a smaller track so Start/End date pair up at a narrower width
    // than Course name/Institution do — dates need far less room.
    expect(rows.length).toBe(seed.length * 2);

    const textRows = rows.filter((_, i) => i % 2 === 0);
    const dateRows = rows.filter((_, i) => i % 2 === 1);

    for (const row of textRows) {
      expect(row.classList.contains('grid-cols-[repeat(auto-fit,minmax(11rem,1fr))]')).toBe(true);
    }
    for (const row of dateRows) {
      expect(row.classList.contains('grid-cols-[repeat(auto-fit,minmax(9rem,1fr))]')).toBe(true);
    }

    for (const row of rows) {
      expect(row.classList.contains('grid-cols-2')).toBe(false);
    }
  });

  it('should render a form per course with fb-field, fb-date-picker and fb-rich-text', () => {
    expect(fixture.nativeElement.querySelectorAll('form').length).toBe(seed.length);
    expect(fixture.nativeElement.querySelector('fb-field')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('fb-date-picker')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('fb-rich-text')).toBeTruthy();
  });

  it('should compose the collapsed title as "name at school"', () => {
    expect(fixture.componentInstance['courseTitle'](seed[0])).toBe(
      'Advanced React at Frontend Masters',
    );
    expect(fixture.componentInstance['courseTitle'](seed[1])).toBe('Rust Fundamentals at Udemy');
  });

  it('updates the title synchronously and forwards the value to the debouncing store', () => {
    const form = fixture.componentInstance['getForm'](seed[0]);
    form.patchValue({ name: 'Advanced React Patterns' });

    expect(fixture.componentInstance['courseTitle'](seed[0])).toBe(
      'Advanced React Patterns at Frontend Masters',
    );
    expect(store.updateCourse).toHaveBeenCalledTimes(1);
  });

  it('leaves debounce timing to the store', () => {
    vi.useFakeTimers();
    try {
      const form = fixture.componentInstance['getForm'](seed[0]);
      form.patchValue({ name: 'Advanced React Patterns' });
      vi.advanceTimersByTime(1999);

      expect(store.updateCourse).toHaveBeenCalledTimes(1);
    } finally {
      vi.useRealTimers();
    }
  });

  it('joins name and school, dropping whichever part is empty', () => {
    const titleFn = fixture.componentInstance['courseTitle'];
    const base = seed[0];

    expect(titleFn({ ...base, name: 'React', school: 'Udemy' })).toBe('React at Udemy');
    expect(titleFn({ ...base, name: 'React', school: '' })).toBe('React');
    expect(titleFn({ ...base, name: '', school: 'Udemy' })).toBe('Udemy');
    expect(titleFn({ ...base, name: '', school: '' })).toBe('Untitled');
  });

  it('withholds "Untitled" while focused and applies it once focusout fires for real', () => {
    const item = seed[0];
    const form = fixture.componentInstance['getForm'](item);
    form.patchValue({ name: 'Advanced React', school: 'Frontend Masters' });

    fixture.componentInstance['onTitleFocusIn'](item);
    form.patchValue({ name: '', school: '' });

    expect(fixture.componentInstance['courseTitle'](item)).toBe('Advanced React at Frontend Masters');

    const wrapper = document.createElement('div');
    const event = { currentTarget: wrapper, relatedTarget: null } as unknown as FocusEvent;
    fixture.componentInstance['onTitleFocusOut'](item, event);

    expect(fixture.componentInstance['courseTitle'](item)).toBe('Untitled');
  });

  it('falls back to the store model for an entry whose form has not been built yet', () => {
    const unbuilt: CourseModel = {
      id: 'c3', order: 2, name: 'Kubernetes Basics', school: 'Pluralsight',
      startDate: '', endDate: '', description: '',
    };

    expect(fixture.componentInstance['courseTitle'](unbuilt)).toBe('Kubernetes Basics at Pluralsight');
  });

  it('should add a course when the add control is clicked', () => {
    const addBtn = fixture.nativeElement.querySelector(
      'button[aria-label="Add course"]',
    ) as HTMLButtonElement;
    addBtn.click();
    expect(store.addCourse).toHaveBeenCalledTimes(1);
  });

  it('should delete the course when its delete control is clicked', () => {
    const delBtn = fixture.nativeElement.querySelector(
      'button[aria-label^="Delete"]',
    ) as HTMLButtonElement;
    delBtn.click();
    expect(store.deleteCourse).toHaveBeenCalledWith(seed[0]);
  });

  it('should reorder courses when the list emits itemDrop', () => {
    const list = fixture.debugElement.query(By.directive(OrderableList))
      .componentInstance as OrderableList<CourseModel>;
    const event = { previousIndex: 1, currentIndex: 0 } as CdkDragDrop<Orderly[]>;
    list.itemDrop.emit(event);
    expect(store.reorderCourses).toHaveBeenCalledWith(event);
  });

  it('does not persist any entry when the section is opened with existing descriptions', async () => {
    const described: CourseModel[] = seed.map(c => ({
      ...c,
      description: `<p>Course at ${c.school}</p>`,
    }));

    TestBed.resetTestingModule();
    vi.useFakeTimers();
    try {
      const isolated = { ...mockResumesStore(), courses: signal(described) };
      await TestBed.configureTestingModule({
        imports: [Course],
        providers: [{ provide: ResumesStore, useValue: isolated }],
      }).compileComponents();

      const f = TestBed.createComponent(Course);
      f.detectChanges();
      await f.whenStable();

      vi.advanceTimersByTime(3000);

      expect(isolated.updateCourse).not.toHaveBeenCalled();
    } finally {
      vi.useRealTimers();
    }
  }, 20_000);

  it('should have no AXE violations', async () => {
    await expectNoAxeViolations(fixture);
  }, 20000);
});
