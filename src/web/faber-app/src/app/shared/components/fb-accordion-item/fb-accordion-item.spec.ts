import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { LUCIDE_ICONS, LucideIconProvider, User } from 'lucide-angular';
import { vi } from 'vitest';
import { FbAccordionItem } from './fb-accordion-item';

// Host for content projection tests
@Component({
  template: `
    <fb-accordion-item label="Projected Section">
      <span fbAccordionPrefix class="prefix-slot">drag</span>
      <button fbAccordionSuffix type="button" class="suffix-slot">delete</button>
      <p class="body-slot">Body content</p>
    </fb-accordion-item>
  `,
  imports: [FbAccordionItem],
})
class ProjectionHost {}

describe('FbAccordionItem', () => {
  let fixture: ComponentFixture<FbAccordionItem>;
  let component: FbAccordionItem;

  const lucideProviders = [
    { provide: LUCIDE_ICONS, multi: true, useValue: new LucideIconProvider({ User }) },
  ];

  beforeEach(async () => {
    vi.clearAllMocks();
    await TestBed.configureTestingModule({
      imports: [FbAccordionItem],
      providers: lucideProviders,
    }).compileComponents();

    fixture = TestBed.createComponent(FbAccordionItem);
    fixture.componentRef.setInput('label', 'Test Section');
    fixture.detectChanges();
    await fixture.whenStable();
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start closed by default', () => {
    expect(component.open()).toBe(false);
    expect(fixture.nativeElement.classList.contains('open')).toBe(false);
  });

  it('should toggle open when trigger button is clicked', () => {
    fixture.debugElement.query(By.css('button[type="button"]')).nativeElement.click();
    fixture.detectChanges();
    expect(component.open()).toBe(true);
    expect(fixture.nativeElement.classList.contains('open')).toBe(true);
  });

  it('should toggle closed again on second click', () => {
    const btn = fixture.debugElement.query(By.css('button[type="button"]')).nativeElement;
    btn.click();
    fixture.detectChanges();
    btn.click();
    fixture.detectChanges();
    expect(component.open()).toBe(false);
  });

  it('should set aria-expanded="false" when closed', () => {
    const btn = fixture.debugElement.query(By.css('button[type="button"]')).nativeElement;
    expect(btn.getAttribute('aria-expanded')).toBe('false');
  });

  it('should set aria-expanded="true" when open', () => {
    component.open.set(true);
    fixture.detectChanges();
    const btn = fixture.debugElement.query(By.css('button[type="button"]')).nativeElement;
    expect(btn.getAttribute('aria-expanded')).toBe('true');
  });

  it('should have aria-controls pointing to the body element', () => {
    const btn = fixture.debugElement.query(By.css('button[type="button"]')).nativeElement;
    const bodyId = btn.getAttribute('aria-controls');
    expect(bodyId).toBeTruthy();
    expect(fixture.nativeElement.querySelector(`#${bodyId}`)).toBeTruthy();
  });

  it('should have role="region" on the body', () => {
    expect(fixture.debugElement.query(By.css('[role="region"]'))).toBeTruthy();
  });

  it('should mark the body inert when closed so it stays out of tab order', () => {
    const body = fixture.debugElement.query(By.css('[role="region"]')).nativeElement as HTMLElement;
    expect(body.inert).toBe(true);
  });

  it('should not be inert when open', () => {
    component.open.set(true);
    fixture.detectChanges();
    const body = fixture.debugElement.query(By.css('[role="region"]')).nativeElement as HTMLElement;
    expect(body.inert).toBe(false);
  });

  it('should have aria-labelledby on body matching trigger id', () => {
    const btn = fixture.debugElement.query(By.css('button[type="button"]')).nativeElement;
    const body = fixture.debugElement.query(By.css('[role="region"]')).nativeElement;
    expect(body.getAttribute('aria-labelledby')).toBe(btn.id);
  });

  it('should render lucide-icon when icon input is provided', () => {
    fixture.componentRef.setInput('icon', 'user');
    fixture.detectChanges();
    expect(fixture.debugElement.query(By.css('lucide-icon'))).toBeTruthy();
  });

  it('should not render lucide-icon when icon is absent', () => {
    expect(fixture.debugElement.query(By.css('lucide-icon'))).toBeNull();
  });

  it('should render count badge when count > 0', () => {
    fixture.componentRef.setInput('count', 3);
    fixture.detectChanges();
    const badge = fixture.debugElement.query(By.css('.rounded-full'));
    expect(badge).toBeTruthy();
    expect(badge.nativeElement.textContent.trim()).toBe('3');
  });

  it('should not render count badge when count is 0', () => {
    fixture.componentRef.setInput('count', 0);
    fixture.detectChanges();
    expect(fixture.debugElement.query(By.css('.rounded-full'))).toBeNull();
  });

  it('should not render count badge when count is absent', () => {
    expect(fixture.debugElement.query(By.css('.rounded-full'))).toBeNull();
  });

  it('should open when open model is set externally', () => {
    component.open.set(true);
    fixture.detectChanges();
    expect(fixture.nativeElement.classList.contains('open')).toBe(true);
  });

  describe('mobile padding', () => {
    // A list entry sits inside two accordion bodies, so px-5 was paid twice —
    // 80px of pure indentation on a 375px phone (#425). Below md each level
    // drops to px-3, giving the entry form 303px instead of 211px.
    it('halves the header padding below md', () => {
      fixture.detectChanges();
      const trigger = fixture.nativeElement.querySelector('button[aria-expanded]') as HTMLElement;
      expect(trigger.classList.contains('px-5')).toBe(true);
      expect(trigger.classList.contains('max-md:px-3')).toBe(true);
    });

    it('halves the body padding below md', () => {
      fixture.detectChanges();
      const body = fixture.nativeElement.querySelector('[role="region"] > div > div') as HTMLElement;
      expect(body.classList.contains('px-5')).toBe(true);
      expect(body.classList.contains('max-md:px-3')).toBe(true);
    });

    it('grows the chevron to a 44px touch target below md', () => {
      fixture.detectChanges();
      const chevron = fixture.nativeElement.querySelector('[data-testid="accordion-chevron"]') as HTMLElement;
      expect(chevron).toBeTruthy();
      expect(chevron.classList.contains('max-md:size-11')).toBe(true);
    });
  });
});

describe('FbAccordionItem — content projection', () => {
  let hostFixture: ComponentFixture<ProjectionHost>;

  beforeEach(async () => {
    vi.clearAllMocks();
    await TestBed.configureTestingModule({
      imports: [ProjectionHost],
    }).compileComponents();

    hostFixture = TestBed.createComponent(ProjectionHost);
    hostFixture.detectChanges();
    await hostFixture.whenStable();
  });

  it('should project body content into the region', () => {
    const body = hostFixture.debugElement.query(By.css('[role="region"]'));
    expect(body.nativeElement.querySelector('.body-slot')).toBeTruthy();
  });

  it('should project fbAccordionPrefix before the trigger button', () => {
    const row = hostFixture.debugElement.query(By.css('fb-accordion-item > div'));
    const children = Array.from(row.nativeElement.children) as HTMLElement[];
    const prefixIndex = children.findIndex(el => el.classList.contains('prefix-slot'));
    const buttonIndex = children.findIndex(el => el.tagName === 'BUTTON');
    expect(prefixIndex).toBeLessThan(buttonIndex);
  });

  it('should project fbAccordionSuffix after the trigger button', () => {
    const row = hostFixture.debugElement.query(By.css('fb-accordion-item > div'));
    const children = Array.from(row.nativeElement.children) as HTMLElement[];
    const suffixIndex = children.findIndex(el => el.classList.contains('suffix-slot'));
    const buttonIndex = children.findIndex(el => el.tagName === 'BUTTON');
    expect(suffixIndex).toBeGreaterThan(buttonIndex);
  });
});
