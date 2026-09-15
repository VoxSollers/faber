import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { Skill } from './skill';
import { Skill as SkillModel } from './skill-response';
import { ResumesStore } from '../../resumes-store';
import { Orderly } from '../../orderly';
import { OrderableList } from '../orderable-list/orderable-list';
import { mockResumesStore } from '../../../../shared/testing/mock-resumes-store';
import { expectNoAxeViolations } from '../../../../shared/testing/axe';

describe('Skill', () => {
  let fixture: ComponentFixture<Skill>;
  let store: Partial<ResumesStore>;

  const seed: SkillModel[] = [
    { id: 's1', order: 0, name: 'Figma', level: 'Expert' },
    { id: 's2', order: 1, name: 'TypeScript', level: 'Advanced' },
  ];

  beforeEach(async () => {
    vi.clearAllMocks();
    store = { ...mockResumesStore(), skills: signal(seed) };

    await TestBed.configureTestingModule({
      imports: [Skill],
      providers: [{ provide: ResumesStore, useValue: store }],
    }).compileComponents();

    fixture = TestBed.createComponent(Skill);
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

  it('should render a row with fb-field and fb-select per skill', () => {
    expect(fixture.nativeElement.querySelectorAll('form').length).toBe(seed.length);
    expect(fixture.nativeElement.querySelector('fb-field')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('fb-select')).toBeTruthy();
  });

  it('updates the title synchronously from the live form value', () => {
    const form = fixture.componentInstance['getForm'](seed[0]);
    form.patchValue({ name: 'Product Design' });

    expect(fixture.componentInstance['skillTitle'](seed[0])).toBe('Product Design');
  });

  it('withholds "Untitled" while the title field is focused', () => {
    const item = seed[0];
    const form = fixture.componentInstance['getForm'](item);
    form.patchValue({ name: 'Figma' });

    fixture.componentInstance['onTitleFocusIn'](item);
    form.patchValue({ name: '' });

    expect(fixture.componentInstance['skillTitle'](item)).toBe('Figma');

    const wrapper = document.createElement('div');
    const event = { currentTarget: wrapper, relatedTarget: null } as unknown as FocusEvent;
    fixture.componentInstance['onTitleFocusOut'](item, event);

    expect(fixture.componentInstance['skillTitle'](item)).toBe('Untitled');
  });

  it('should add a skill when the add control is clicked', () => {
    const addBtn = fixture.nativeElement.querySelector(
      'button[aria-label="Add skill"]',
    ) as HTMLButtonElement;
    addBtn.click();
    expect(store.addSkill).toHaveBeenCalledTimes(1);
  });

  it('should delete the skill when its delete control is clicked', () => {
    const delBtn = fixture.nativeElement.querySelector(
      'button[aria-label^="Delete"]',
    ) as HTMLButtonElement;
    delBtn.click();
    expect(store.deleteSkill).toHaveBeenCalledWith(seed[0]);
  });

  it('should reorder skills when the list emits itemDrop', () => {
    const list = fixture.debugElement.query(By.directive(OrderableList))
      .componentInstance as OrderableList<SkillModel>;
    const event = { previousIndex: 1, currentIndex: 0 } as CdkDragDrop<Orderly[]>;
    list.itemDrop.emit(event);
    expect(store.reorderSkills).toHaveBeenCalledWith(event);
  });

  it('should have no AXE violations', async () => {
    await expectNoAxeViolations(fixture);
  }, 20000);
});
