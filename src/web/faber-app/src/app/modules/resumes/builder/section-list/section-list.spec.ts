import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { signal, WritableSignal } from '@angular/core';
import {
  LUCIDE_ICONS, LucideIconProvider,
  User, FileText, Briefcase, GraduationCap, Zap, Globe, Link2, BookOpen, Heart,
} from 'lucide-angular';
import { vi } from 'vitest';
import { SectionList } from './section-list';
import { ResumesStore, SectionBulkRequest } from '../../resumes-store';
import { FbAccordionItem } from '../../../../shared/components/fb-accordion-item/fb-accordion-item';
import { mockResumesStore } from '../../../../shared/testing/mock-resumes-store';

describe('SectionList', () => {
  let component: SectionList;
  let fixture: ComponentFixture<SectionList>;

  function getItems() {
    return fixture.debugElement.queryAll(By.directive(FbAccordionItem));
  }

  function getItem(index: number) {
    return getItems()[index];
  }

  function getItemComponent(index: number): FbAccordionItem {
    return getItem(index).componentInstance as FbAccordionItem;
  }

  function getTrigger(index: number): HTMLButtonElement {
    return getItem(index).query(By.css('button[type="button"]')).nativeElement;
  }

  let requestedAllSections: WritableSignal<SectionBulkRequest | null>;

  beforeEach(async () => {
    vi.clearAllMocks();
    requestedAllSections = signal<SectionBulkRequest | null>(null);
    await TestBed.configureTestingModule({
      imports: [SectionList],
      providers: [
        { provide: ResumesStore, useValue: { ...mockResumesStore(), requestedAllSections } },
        {
          provide: LUCIDE_ICONS,
          multi: true,
          useValue: new LucideIconProvider({
            User, FileText, Briefcase, GraduationCap, Zap, Globe, Link2, BookOpen, Heart,
          }),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SectionList);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render 10 section rows', () => {
    expect(getItems().length).toBe(10);
  });

  it('should have role="list" on the accordion container', () => {
    const container = fixture.debugElement.query(By.css('[role="list"]'));
    expect(container).toBeTruthy();
  });

  it('should start with person section expanded', () => {
    expect(getItemComponent(0).open()).toBe(true);
    expect(getItem(0).nativeElement.classList.contains('open')).toBe(true);
  });

  it('should have aria-expanded="true" on the open section trigger', () => {
    expect(getTrigger(0).getAttribute('aria-expanded')).toBe('true');
  });

  it('should have aria-expanded="false" on a closed section trigger', () => {
    expect(getTrigger(1).getAttribute('aria-expanded')).toBe('false');
  });

  it('should expand a collapsed section on header click', () => {
    getTrigger(1).click();
    fixture.detectChanges();
    expect(getItemComponent(1).open()).toBe(true);
  });

  it('should collapse an open section on header click', () => {
    getTrigger(0).click();
    fixture.detectChanges();
    expect(getItemComponent(0).open()).toBe(false);
  });

  it('should have aria-controls pointing to the matching body element', () => {
    const trigger = getTrigger(2);
    const bodyId = trigger.getAttribute('aria-controls');
    expect(bodyId).toBeTruthy();
    expect(fixture.nativeElement.querySelector(`#${bodyId}`)).toBeTruthy();
  });

  it('should have role="region" with aria-labelledby on each section body', () => {
    const body = getItem(0).query(By.css('[role="region"]')).nativeElement;
    const triggerId = getTrigger(0).id;
    expect(body.getAttribute('role')).toBe('region');
    expect(body.getAttribute('aria-labelledby')).toBe(triggerId);
  });

  it('should allow multiple sections to be open simultaneously', () => {
    getTrigger(1).click();
    fixture.detectChanges();
    // Both person (index 0) and summary (index 1) should now be open
    expect(getItemComponent(0).open()).toBe(true);
    expect(getItemComponent(1).open()).toBe(true);
  });

  async function settle(): Promise<void> {
    await fixture.whenStable();
    fixture.detectChanges();
  }

  describe('bulk expand / collapse', () => {
    it('opens every section on an expand request', async () => {
      requestedAllSections.set({ action: 'expand' });
      await settle();
      expect(getItems().every(item => (item.componentInstance as FbAccordionItem).open())).toBe(true);
    });

    it('closes every section on a collapse request', async () => {
      requestedAllSections.set({ action: 'collapse' });
      await settle();
      expect(getItems().some(item => (item.componentInstance as FbAccordionItem).open())).toBe(false);
    });

    it('re-applies a repeated expand request after a section was closed by hand', async () => {
      requestedAllSections.set({ action: 'expand' });
      await settle();

      getTrigger(1).click();
      await settle();
      expect(getItemComponent(1).open()).toBe(false);

      requestedAllSections.set({ action: 'expand' });
      await settle();
      expect(getItemComponent(1).open()).toBe(true);
    });
  });
});
