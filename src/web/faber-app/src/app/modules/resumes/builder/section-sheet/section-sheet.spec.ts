import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DialogRef } from '@angular/cdk/dialog';
import { SectionSheet, SECTION_SHEET_ITEMS } from './section-sheet';
import { ResumesStore } from '../../resumes-store';
import { mockResumesStore } from '../../../../shared/testing/mock-resumes-store';
import { expectNoAxeViolations } from '../../../../shared/testing/axe';

describe('SectionSheet', () => {
  let fixture: ComponentFixture<SectionSheet>;

  const requestSection = vi.fn();
  const requestAllSections = vi.fn();
  const close = vi.fn();

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [SectionSheet],
      providers: [
        {
          provide: ResumesStore,
          useValue: { ...mockResumesStore(), requestSection, requestAllSections },
        },
        { provide: DialogRef, useValue: { close } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SectionSheet);
    await fixture.whenStable();
    fixture.detectChanges();
  });

  function sectionButtons(): HTMLButtonElement[] {
    return Array.from(
      fixture.nativeElement.querySelectorAll('nav[aria-label="Resume sections"] button'),
    ) as HTMLButtonElement[];
  }

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('lists every builder section', () => {
    expect(sectionButtons().length).toBe(SECTION_SHEET_ITEMS.length);
    expect(sectionButtons().length).toBe(9);
    expect(sectionButtons()[0].textContent?.trim()).toBe('Personal Details');
  });

  it('preserves list semantics on the section list', () => {
    const list = fixture.nativeElement.querySelector('nav[aria-label="Resume sections"] ul') as HTMLElement;
    expect(list.getAttribute('role')).toBe('list');

    const items = Array.from(
      fixture.nativeElement.querySelectorAll('nav[aria-label="Resume sections"] ul > li'),
    ) as HTMLElement[];
    expect(items.length).toBe(SECTION_SHEET_ITEMS.length);
    expect(items.every(item => item.getAttribute('role') === 'listitem')).toBe(true);
  });

  it('requests the chosen section and closes', () => {
    sectionButtons()[2].click();
    expect(requestSection).toHaveBeenCalledWith('experience');
    expect(close).toHaveBeenCalled();
  });

  it('expands every section and closes', () => {
    (fixture.nativeElement.querySelector('[aria-label="Expand all sections"]') as HTMLButtonElement).click();
    expect(requestAllSections).toHaveBeenCalledWith('expand');
    expect(close).toHaveBeenCalled();
  });

  it('collapses every section and closes', () => {
    (fixture.nativeElement.querySelector('[aria-label="Collapse all sections"]') as HTMLButtonElement).click();
    expect(requestAllSections).toHaveBeenCalledWith('collapse');
    expect(close).toHaveBeenCalled();
  });

  it('is a labelled modal dialog', () => {
    const dialog = fixture.nativeElement.querySelector('[role="dialog"]') as HTMLElement;
    expect(dialog.getAttribute('aria-modal')).toBe('true');
    expect(fixture.nativeElement.querySelector('#section-sheet-title')?.textContent).toContain('Jump to section');
  });

  it('has no AXE violations', async () => {
    await expectNoAxeViolations(fixture);
  }, 20000);
});
