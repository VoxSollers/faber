import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';
import { FbAccordion } from './fb-accordion';
import { FbAccordionItem } from '../fb-accordion-item/fb-accordion-item';

@Component({
  template: `
    <fb-accordion>
      <fb-accordion-item label="Item A" />
      <fb-accordion-item label="Item B" />
    </fb-accordion>
  `,
  imports: [FbAccordion, FbAccordionItem],
})
class HostSingleOpen {}

@Component({
  template: `
    <fb-accordion [allowMultiple]="true">
      <fb-accordion-item label="Item A" />
      <fb-accordion-item label="Item B" />
    </fb-accordion>
  `,
  imports: [FbAccordion, FbAccordionItem],
})
class HostMultiOpen {}

describe('FbAccordion', () => {
  beforeEach(async () => {
    vi.clearAllMocks();
    await TestBed.configureTestingModule({
      imports: [HostSingleOpen, HostMultiOpen],
    }).compileComponents();
  });

  function items(f: ComponentFixture<unknown>): FbAccordionItem[] {
    return f.debugElement
      .queryAll(By.directive(FbAccordionItem))
      .map(de => de.componentInstance as FbAccordionItem);
  }

  function trigger(f: ComponentFixture<unknown>, index: number): HTMLButtonElement {
    return f.debugElement
      .queryAll(By.directive(FbAccordionItem))[index]
      .query(By.css('button[type="button"]')).nativeElement;
  }

  describe('allowMultiple = false (default)', () => {
    let fixture: ComponentFixture<HostSingleOpen>;

    beforeEach(async () => {
      fixture = TestBed.createComponent(HostSingleOpen);
      fixture.detectChanges();
      await fixture.whenStable();
    });

    it('should create', () => {
      expect(fixture.debugElement.query(By.directive(FbAccordion))).toBeTruthy();
    });

    it('should render two accordion items', () => {
      expect(items(fixture).length).toBe(2);
    });

    it('should close item A when item B is opened', () => {
      const [itemA, itemB] = items(fixture);

      trigger(fixture, 0).click();
      fixture.detectChanges();
      expect(itemA.open()).toBe(true);

      trigger(fixture, 1).click();
      fixture.detectChanges();
      expect(itemB.open()).toBe(true);
      expect(itemA.open()).toBe(false);
    });

    it('should allow closing the open item without opening another', () => {
      const [itemA] = items(fixture);
      trigger(fixture, 0).click();
      fixture.detectChanges();
      trigger(fixture, 0).click();
      fixture.detectChanges();
      expect(itemA.open()).toBe(false);
    });

    it('should close item A when item B is opened externally (not via click)', () => {
      const [itemA, itemB] = items(fixture);

      itemA.open.set(true);
      fixture.detectChanges();
      expect(itemA.open()).toBe(true);

      itemB.open.set(true);
      fixture.detectChanges();
      expect(itemB.open()).toBe(true);
      expect(itemA.open()).toBe(false);
    });
  });

  describe('allowMultiple = true', () => {
    let fixture: ComponentFixture<HostMultiOpen>;

    beforeEach(async () => {
      fixture = TestBed.createComponent(HostMultiOpen);
      fixture.detectChanges();
      await fixture.whenStable();
    });

    it('should keep item A open when item B is opened', () => {
      const [itemA, itemB] = items(fixture);

      trigger(fixture, 0).click();
      fixture.detectChanges();
      trigger(fixture, 1).click();
      fixture.detectChanges();

      expect(itemA.open()).toBe(true);
      expect(itemB.open()).toBe(true);
    });
  });

  describe('item registration', () => {
    let fixture: ComponentFixture<HostSingleOpen>;

    beforeEach(async () => {
      fixture = TestBed.createComponent(HostSingleOpen);
      fixture.detectChanges();
      await fixture.whenStable();
    });

    it('should unregister item on destroy so accordion does not close a destroyed item', () => {
      trigger(fixture, 0).click();
      fixture.detectChanges();

      const [itemA] = items(fixture);
      itemA.ngOnDestroy();

      trigger(fixture, 1).click();
      fixture.detectChanges();

      expect(items(fixture)[1].open()).toBe(true);
    });
  });
});
