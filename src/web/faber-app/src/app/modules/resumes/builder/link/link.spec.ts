import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Link } from './link';
import { Link as LinkModel } from './link-response';
import { ResumesStore } from '../../resumes-store';
import { Orderly } from '../../orderly';
import { OrderableList } from '../orderable-list/orderable-list';
import { mockResumesStore } from '../../../../shared/testing/mock-resumes-store';
import { expectNoAxeViolations } from '../../../../shared/testing/axe';

describe('Link', () => {
  let fixture: ComponentFixture<Link>;
  let store: Partial<ResumesStore>;

  const seed: LinkModel[] = [
    { id: 'k1', order: 0, label: 'LinkedIn', uri: 'https://linkedin.com/in/me' },
    { id: 'k2', order: 1, label: 'GitHub', uri: 'https://github.com/me' },
  ];

  beforeEach(async () => {
    vi.clearAllMocks();
    store = { ...mockResumesStore(), links: signal(seed) };

    await TestBed.configureTestingModule({
      imports: [Link],
      providers: [{ provide: ResumesStore, useValue: store }],
    }).compileComponents();

    fixture = TestBed.createComponent(Link);
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

  it('should render two fb-field inputs per link', () => {
    expect(fixture.nativeElement.querySelectorAll('form').length).toBe(seed.length);
    const firstForm = fixture.nativeElement.querySelector('form') as HTMLFormElement;
    expect(firstForm.querySelectorAll('fb-field').length).toBe(2);
    expect(firstForm.querySelector('fb-form-field')).toBeNull();
  });

  it('uses label before URI, falling back to either available value', () => {
    const titleFn = fixture.componentInstance['linkTitle'];
    const base = seed[0];

    expect(titleFn(base)).toBe('LinkedIn');
    expect(titleFn({ ...base, label: '', uri: 'https://example.com' })).toBe('https://example.com');
    expect(titleFn({ ...base, label: 'Portfolio', uri: '' })).toBe('Portfolio');
    expect(titleFn({ ...base, label: '', uri: '' })).toBe('Untitled');
  });

  it('updates the title synchronously from the live form value', () => {
    const form = fixture.componentInstance['getForm'](seed[0]);
    form.patchValue({ label: 'Portfolio' });

    expect(fixture.componentInstance['linkTitle'](seed[0])).toBe('Portfolio');

    form.patchValue({ label: '', uri: 'https://portfolio.example' });

    expect(fixture.componentInstance['linkTitle'](seed[0])).toBe('https://portfolio.example');
  });

  it('withholds "Untitled" while either title field is focused', () => {
    const item = seed[0];
    const form = fixture.componentInstance['getForm'](item);
    form.patchValue({ label: 'LinkedIn', uri: 'https://linkedin.com/in/me' });

    fixture.componentInstance['onTitleFocusIn'](item);
    form.patchValue({ label: '', uri: '' });

    expect(fixture.componentInstance['linkTitle'](item)).toBe('LinkedIn');

    const wrapper = document.createElement('div');
    const event = { currentTarget: wrapper, relatedTarget: null } as unknown as FocusEvent;
    fixture.componentInstance['onTitleFocusOut'](item, event);

    expect(fixture.componentInstance['linkTitle'](item)).toBe('Untitled');
  });

  it('should add a link when the add control is clicked', () => {
    const addBtn = fixture.nativeElement.querySelector(
      'button[aria-label="Add link"]',
    ) as HTMLButtonElement;
    addBtn.click();
    expect(store.addLink).toHaveBeenCalledTimes(1);
  });

  it('should delete the link when its delete control is clicked', () => {
    const delBtn = fixture.nativeElement.querySelector(
      'button[aria-label^="Delete"]',
    ) as HTMLButtonElement;
    delBtn.click();
    expect(store.deleteLink).toHaveBeenCalledWith(seed[0]);
  });

  it('should reorder links when the list emits itemDrop', () => {
    const list = fixture.debugElement.query(By.directive(OrderableList))
      .componentInstance as OrderableList<LinkModel>;
    const event = { previousIndex: 1, currentIndex: 0 } as CdkDragDrop<Orderly[]>;
    list.itemDrop.emit(event);
    expect(store.reorderLinks).toHaveBeenCalledWith(event);
  });

  it('should have no AXE violations', async () => {
    await expectNoAxeViolations(fixture);
  }, 20000);
});
