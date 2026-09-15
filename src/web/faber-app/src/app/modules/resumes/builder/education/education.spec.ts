import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Education } from './education';
import { Education as EducationModel } from './education-response';
import { ResumesStore } from '../../resumes-store';
import { Orderly } from '../../orderly';
import { OrderableList } from '../orderable-list/orderable-list';
import { mockResumesStore } from '../../../../shared/testing/mock-resumes-store';
import { expectNoAxeViolations } from '../../../../shared/testing/axe';

describe('Education', () => {
  let fixture: ComponentFixture<Education>;
  let store: Partial<ResumesStore>;

  const seed: EducationModel[] = [
    {
      id: 'd1', order: 0, school: 'University of Oxford', degree: 'BSc Computer Science',
      startDate: '2014-09-01', endDate: '2017-06-01', city: 'Oxford', description: '',
    },
    {
      id: 'd2', order: 1, school: 'MIT', degree: 'MSc AI',
      startDate: '2018-09-01', endDate: '', city: 'Cambridge', description: '',
    },
  ];

  beforeEach(async () => {
    vi.clearAllMocks();
    store = { ...mockResumesStore(), educations: signal(seed) };

    await TestBed.configureTestingModule({
      imports: [Education],
      providers: [{ provide: ResumesStore, useValue: store }],
    }).compileComponents();

    fixture = TestBed.createComponent(Education);
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
    // a narrower width than School/Degree do — dates need far less room.
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

  it('should render a form per education with fb-field, fb-date-picker and fb-rich-text', () => {
    expect(fixture.nativeElement.querySelectorAll('form').length).toBe(seed.length);
    expect(fixture.nativeElement.querySelector('fb-field')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('fb-date-picker')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('fb-rich-text')).toBeTruthy();
  });

  it('should compose the collapsed title as "degree at school"', () => {
    expect(fixture.componentInstance['educationTitle'](seed[0])).toBe(
      'BSc Computer Science at University of Oxford',
    );
    expect(fixture.componentInstance['educationTitle'](seed[1])).toBe('MSc AI at MIT');
  });

  it('updates the title synchronously and forwards the value to the debouncing store', () => {
    const form = fixture.componentInstance['getForm'](seed[0]);
    form.patchValue({ degree: 'MSc Computer Science' });

    expect(fixture.componentInstance['educationTitle'](seed[0])).toBe(
      'MSc Computer Science at University of Oxford',
    );
    expect(store.updateEducation).toHaveBeenCalledTimes(1);
  });

  it('leaves debounce timing to the store', () => {
    vi.useFakeTimers();
    try {
      const form = fixture.componentInstance['getForm'](seed[0]);
      form.patchValue({ degree: 'MSc Computer Science' });
      vi.advanceTimersByTime(1999);

      expect(store.updateEducation).toHaveBeenCalledTimes(1);
    } finally {
      vi.useRealTimers();
    }
  });

  it('joins degree and school, dropping whichever part is empty', () => {
    const titleFn = fixture.componentInstance['educationTitle'];
    const base = seed[0];

    expect(titleFn({ ...base, degree: 'BSc', school: 'Oxford' })).toBe('BSc at Oxford');
    expect(titleFn({ ...base, degree: 'BSc', school: '' })).toBe('BSc');
    expect(titleFn({ ...base, degree: '', school: 'Oxford' })).toBe('Oxford');
    expect(titleFn({ ...base, degree: '', school: '' })).toBe('Untitled');
  });

  it('withholds "Untitled" while focused and applies it once focusout fires for real', () => {
    const item = seed[0];
    const form = fixture.componentInstance['getForm'](item);
    form.patchValue({ degree: 'BSc Computer Science', school: 'University of Oxford' });

    fixture.componentInstance['onTitleFocusIn'](item);
    form.patchValue({ degree: '', school: '' });

    expect(fixture.componentInstance['educationTitle'](item)).toBe(
      'BSc Computer Science at University of Oxford',
    );

    const wrapper = document.createElement('div');
    const event = { currentTarget: wrapper, relatedTarget: null } as unknown as FocusEvent;
    fixture.componentInstance['onTitleFocusOut'](item, event);

    expect(fixture.componentInstance['educationTitle'](item)).toBe('Untitled');
  });

  it('falls back to the store model for an entry whose form has not been built yet', () => {
    const unbuilt: EducationModel = {
      id: 'd3', order: 2, school: 'Stanford', degree: 'PhD',
      startDate: '', endDate: '', city: '', description: '',
    };

    expect(fixture.componentInstance['educationTitle'](unbuilt)).toBe('PhD at Stanford');
  });

  it('should add an education when the add control is clicked', () => {
    const addBtn = fixture.nativeElement.querySelector(
      'button[aria-label="Add education"]',
    ) as HTMLButtonElement;
    addBtn.click();
    expect(store.addEducation).toHaveBeenCalledTimes(1);
  });

  it('should delete the education when its delete control is clicked', () => {
    const delBtn = fixture.nativeElement.querySelector(
      'button[aria-label^="Delete"]',
    ) as HTMLButtonElement;
    delBtn.click();
    expect(store.deleteEducation).toHaveBeenCalledWith(seed[0]);
  });

  it('should reorder educations when the list emits itemDrop', () => {
    const list = fixture.debugElement.query(By.directive(OrderableList))
      .componentInstance as OrderableList<EducationModel>;
    const event = { previousIndex: 1, currentIndex: 0 } as CdkDragDrop<Orderly[]>;
    list.itemDrop.emit(event);
    expect(store.reorderEducations).toHaveBeenCalledWith(event);
  });

  it('does not persist any entry when the section is opened with existing descriptions', async () => {
    const described: EducationModel[] = seed.map(e => ({
      ...e,
      description: `<p>Studied at ${e.school}</p>`,
    }));

    TestBed.resetTestingModule();
    vi.useFakeTimers();
    try {
      const isolated = { ...mockResumesStore(), educations: signal(described) };
      await TestBed.configureTestingModule({
        imports: [Education],
        providers: [{ provide: ResumesStore, useValue: isolated }],
      }).compileComponents();

      const f = TestBed.createComponent(Education);
      f.detectChanges();
      await f.whenStable();

      vi.advanceTimersByTime(3000);

      expect(isolated.updateEducation).not.toHaveBeenCalled();
    } finally {
      vi.useRealTimers();
    }
  }, 20_000);

  it('should have no AXE violations', async () => {
    await expectNoAxeViolations(fixture);
  }, 20000);
});
