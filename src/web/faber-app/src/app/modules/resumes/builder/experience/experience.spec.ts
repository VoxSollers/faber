import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Experience } from './experience';
import { Experience as ExperienceModel } from './experience-response';
import { ResumesStore } from '../../resumes-store';
import { Orderly } from '../../orderly';
import { OrderableList } from '../orderable-list/orderable-list';
import { mockResumesStore } from '../../../../shared/testing/mock-resumes-store';
import { expectNoAxeViolations } from '../../../../shared/testing/axe';

describe('Experience', () => {
  let fixture: ComponentFixture<Experience>;
  let store: Partial<ResumesStore>;

  const seed: ExperienceModel[] = [
    {
      id: 'e1', order: 0, jobTitle: 'Senior Engineer', employer: 'Acme Corp',
      startDate: '2020-01-01', endDate: '', city: 'London', description: '',
    },
    {
      id: 'e2', order: 1, jobTitle: 'Engineer', employer: 'Globex',
      startDate: '2018-03-01', endDate: '2019-12-01', city: 'Berlin', description: '',
    },
  ];

  beforeEach(async () => {
    vi.clearAllMocks();
    store = { ...mockResumesStore(), experiences: signal(seed) };

    await TestBed.configureTestingModule({
      imports: [Experience],
      providers: [{ provide: ResumesStore, useValue: store }],
    }).compileComponents();

    fixture = TestBed.createComponent(Experience);
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('sizes entry fields so extra columns only appear once there is room for them', () => {
    const rows = Array.from(
      fixture.nativeElement.querySelectorAll('form > div.grid'),
    ) as HTMLElement[];
    // Two grid rows per entry: the text pair, then the date/city row. The date
    // row uses a smaller track than the text row so Start/End date pair up at
    // a narrower width than Job title/Employer do — dates need far less room.
    expect(rows.length).toBe(seed.length * 2);

    const textRows = rows.filter((_, i) => i % 2 === 0);
    const dateRows = rows.filter((_, i) => i % 2 === 1);

    for (const row of textRows) {
      expect(row.classList.contains('grid-cols-[repeat(auto-fit,minmax(11rem,1fr))]')).toBe(true);
    }
    for (const row of dateRows) {
      expect(row.classList.contains('grid-cols-[repeat(auto-fit,minmax(9rem,1fr))]')).toBe(true);
    }

    // The old fixed-column classes ignored the form's own rendered width;
    // none of them should remain.
    for (const row of rows) {
      expect(row.classList.contains('grid-cols-2')).toBe(false);
      expect(row.classList.contains('grid-cols-3')).toBe(false);
    }
  });

  it('spans City across both columns only while the date row itself has exactly 2', () => {
    const dateRows = Array.from(
      fixture.nativeElement.querySelectorAll('form > div.grid'),
    ).filter((_, i) => i % 2 === 1) as HTMLElement[];

    for (const row of dateRows) {
      // Scoped to this row so the [300px, 456px) range below matches its own
      // width — the window where auto-fit gives exactly 2 columns, leaving
      // City alone in row 2 with an empty second track beside it otherwise.
      expect(row.classList.contains('@container')).toBe(true);

      const city = row.querySelector('[formControlName="city"]') as HTMLElement;
      expect(city.classList.contains('@[300px]:@max-[455px]:col-span-2')).toBe(true);
    }
  });

  it('should render a form per experience with fb-field, fb-date-picker and fb-rich-text', () => {
    expect(fixture.nativeElement.querySelectorAll('form').length).toBe(seed.length);
    expect(fixture.nativeElement.querySelector('fb-field')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('fb-date-picker')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('fb-rich-text')).toBeTruthy();
  });

  it('should compose the collapsed title as "job title at employer"', () => {
    expect(fixture.componentInstance['experienceTitle'](seed[0])).toBe('Senior Engineer at Acme Corp');
    expect(fixture.componentInstance['experienceTitle'](seed[1])).toBe('Engineer at Globex');
  });

  it('updates the title synchronously and forwards the value to the debouncing store', () => {
    const form = fixture.componentInstance['getForm'](seed[0]);
    form.patchValue({ jobTitle: 'Staff Engineer' });

    expect(fixture.componentInstance['experienceTitle'](seed[0])).toBe('Staff Engineer at Acme Corp');
    expect(store.updateExperience).toHaveBeenCalledTimes(1);
  });

  it('leaves debounce timing to the store', () => {
    vi.useFakeTimers();
    try {
      const form = fixture.componentInstance['getForm'](seed[0]);
      form.patchValue({ jobTitle: 'Staff Engineer' });
      vi.advanceTimersByTime(1999);

      expect(store.updateExperience).toHaveBeenCalledTimes(1);
    } finally {
      vi.useRealTimers();
    }
  });

  it('joins job title and employer, dropping whichever part is empty', () => {
    const titleFn = fixture.componentInstance['experienceTitle'];
    const base = seed[0];

    expect(titleFn({ ...base, jobTitle: 'Developer', employer: 'Acme' })).toBe('Developer at Acme');
    expect(titleFn({ ...base, jobTitle: 'Developer', employer: '' })).toBe('Developer');
    expect(titleFn({ ...base, jobTitle: '', employer: 'Acme' })).toBe('Acme');
    expect(titleFn({ ...base, jobTitle: '', employer: '' })).toBe('Untitled');
  });

  it('withholds "Untitled" while focused and applies it once focusout fires for real', () => {
    const item = seed[0];
    const form = fixture.componentInstance['getForm'](item);
    form.patchValue({ jobTitle: 'Senior Engineer', employer: 'Acme Corp' });

    fixture.componentInstance['onTitleFocusIn'](item);
    form.patchValue({ jobTitle: '', employer: '' });

    expect(fixture.componentInstance['experienceTitle'](item)).toBe('Senior Engineer at Acme Corp');

    const wrapper = document.createElement('div');
    const event = { currentTarget: wrapper, relatedTarget: null } as unknown as FocusEvent;
    fixture.componentInstance['onTitleFocusOut'](item, event);

    expect(fixture.componentInstance['experienceTitle'](item)).toBe('Untitled');
  });

  it('falls back to the store model for an entry whose form has not been built yet', () => {
    const unbuilt: ExperienceModel = {
      id: 'e3', order: 2, jobTitle: 'Consultant', employer: 'Initech',
      startDate: '', endDate: '', city: '', description: '',
    };

    expect(fixture.componentInstance['experienceTitle'](unbuilt)).toBe('Consultant at Initech');
  });

  it('should add an experience when the add control is clicked', () => {
    const addBtn = fixture.nativeElement.querySelector(
      'button[aria-label="Add experience"]',
    ) as HTMLButtonElement;
    addBtn.click();
    expect(store.addExperience).toHaveBeenCalledTimes(1);
  });

  it('should delete the experience when its delete control is clicked', () => {
    const delBtn = fixture.nativeElement.querySelector(
      'button[aria-label^="Delete"]',
    ) as HTMLButtonElement;
    delBtn.click();
    expect(store.deleteExperience).toHaveBeenCalledWith(seed[0]);
  });

  it('should reorder experiences when the list emits itemDrop', () => {
    const list = fixture.debugElement.query(By.directive(OrderableList))
      .componentInstance as OrderableList<ExperienceModel>;
    const event = { previousIndex: 1, currentIndex: 0 } as CdkDragDrop<Orderly[]>;
    list.itemDrop.emit(event);
    expect(store.reorderExperiences).toHaveBeenCalledWith(event);
  });

  it('does not persist any entry when the section is opened with existing descriptions', async () => {
    // Entry bodies render eagerly, so opening the section initialises every
    // entry's editor at once — the broken code emitted one save per entry.
    const described: ExperienceModel[] = seed.map(e => ({
      ...e,
      description: `<p>Did work at ${e.employer}</p>`,
    }));

    TestBed.resetTestingModule();
    vi.useFakeTimers();
    try {
      const isolated = { ...mockResumesStore(), experiences: signal(described) };
      await TestBed.configureTestingModule({
        imports: [Experience],
        providers: [{ provide: ResumesStore, useValue: isolated }],
      }).compileComponents();

      const f = TestBed.createComponent(Experience);
      f.detectChanges();
      await f.whenStable();

      vi.advanceTimersByTime(3000);

      expect(isolated.updateExperience).not.toHaveBeenCalled();
    } finally {
      vi.useRealTimers();
    }
  }, 20_000);

  it('should have no AXE violations', async () => {
    await expectNoAxeViolations(fixture);
  }, 20000);
});
