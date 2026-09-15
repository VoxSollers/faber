import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Language } from './language';
import { Language as LanguageModel } from './language-response';
import { ResumesStore } from '../../resumes-store';
import { Orderly } from '../../orderly';
import { OrderableList } from '../orderable-list/orderable-list';
import { mockResumesStore } from '../../../../shared/testing/mock-resumes-store';
import { expectNoAxeViolations } from '../../../../shared/testing/axe';

describe('Language', () => {
  let fixture: ComponentFixture<Language>;
  let store: Partial<ResumesStore>;

  const seed: LanguageModel[] = [
    { id: 'l1', order: 0, name: 'English', level: 'C2' },
    { id: 'l2', order: 1, name: 'Polish', level: 'Native speaker' },
  ];

  beforeEach(async () => {
    vi.clearAllMocks();
    store = { ...mockResumesStore(), languages: signal(seed) };

    await TestBed.configureTestingModule({
      imports: [Language],
      providers: [{ provide: ResumesStore, useValue: store }],
    }).compileComponents();

    fixture = TestBed.createComponent(Language);
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('sizes entry fields so a second column only appears once there is room for it', () => {
    const forms = fixture.nativeElement.querySelectorAll('form.grid') as NodeListOf<HTMLElement>;
    expect(forms.length).toBe(seed.length);

    for (const form of Array.from(forms)) {
      expect(form.classList.contains('grid-cols-[repeat(auto-fit,minmax(11rem,1fr))]')).toBe(true);
      // The unprefixed two-column class ignored available width entirely and
      // squeezed fields on any narrow pane; it must be gone.
      expect(form.classList.contains('grid-cols-2')).toBe(false);
    }
  });

  it('should render a row with fb-field and fb-select per language', () => {
    expect(fixture.nativeElement.querySelectorAll('form').length).toBe(seed.length);
    expect(fixture.nativeElement.querySelector('fb-field')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('fb-select')).toBeTruthy();
  });

  it('updates the title synchronously from the live form value', () => {
    const form = fixture.componentInstance['getForm'](seed[0]);
    form.patchValue({ name: 'German' });

    expect(fixture.componentInstance['languageTitle'](seed[0])).toBe('German');
  });

  it('withholds "Untitled" while the title field is focused', () => {
    const item = seed[0];
    const form = fixture.componentInstance['getForm'](item);
    form.patchValue({ name: 'English' });

    fixture.componentInstance['onTitleFocusIn'](item);
    form.patchValue({ name: '' });

    expect(fixture.componentInstance['languageTitle'](item)).toBe('English');

    const wrapper = document.createElement('div');
    const event = { currentTarget: wrapper, relatedTarget: null } as unknown as FocusEvent;
    fixture.componentInstance['onTitleFocusOut'](item, event);

    expect(fixture.componentInstance['languageTitle'](item)).toBe('Untitled');
  });

  it('should add a language when the add control is clicked', () => {
    const addBtn = fixture.nativeElement.querySelector(
      'button[aria-label="Add language"]',
    ) as HTMLButtonElement;
    addBtn.click();
    expect(store.addLanguage).toHaveBeenCalledTimes(1);
  });

  it('should delete the language when its delete control is clicked', () => {
    const delBtn = fixture.nativeElement.querySelector(
      'button[aria-label^="Delete"]',
    ) as HTMLButtonElement;
    delBtn.click();
    expect(store.deleteLanguage).toHaveBeenCalledWith(seed[0]);
  });

  it('should reorder languages when the list emits itemDrop', () => {
    const list = fixture.debugElement.query(By.directive(OrderableList))
      .componentInstance as OrderableList<LanguageModel>;
    const event = { previousIndex: 1, currentIndex: 0 } as CdkDragDrop<Orderly[]>;
    list.itemDrop.emit(event);
    expect(store.reorderLanguages).toHaveBeenCalledWith(event);
  });

  it('should have no AXE violations', async () => {
    await expectNoAxeViolations(fixture);
  }, 20000);
});
