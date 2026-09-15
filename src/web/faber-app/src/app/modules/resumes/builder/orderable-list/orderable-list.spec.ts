import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { vi } from 'vitest';
import { CdkDrag, CdkDragDrop, CdkDropList } from '@angular/cdk/drag-drop';
import { FbAccordion } from '../../../../shared/components/fb-accordion/fb-accordion';
import { FbAccordionItem } from '../../../../shared/components/fb-accordion-item/fb-accordion-item';
import { OrderableList } from './orderable-list';
import { Orderly } from '../../orderly';

const ITEMS: Orderly[] = [
  { id: 'a', order: 0 },
  { id: 'b', order: 1 },
];

/**
 * Host component that satisfies the required `contentChild(TemplateRef)` of OrderableList.
 */
@Component({
  template: `
    <app-orderable-list
      [items]="items"
      [title]="title"
      [hideTitle]="hideTitle"
      [titleFn]="titleFn"
      [addLabel]="addLabel">
      <ng-template let-item>
        <span class="item-content">{{ item.id }}</span>
      </ng-template>
    </app-orderable-list>
  `,
  imports: [OrderableList],
})
class HostComponent {
  items: Orderly[] = ITEMS;
  title = 'Skills';
  hideTitle = false;
  titleFn = (item: Orderly) => `Item ${item.order}`;
  addLabel: string | undefined = undefined;
}

describe('OrderableList', () => {
  let hostFixture: ComponentFixture<HostComponent>;

  function getOrderableList(): OrderableList {
    return hostFixture.debugElement.query(By.directive(OrderableList)).componentInstance;
  }

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [HostComponent],
    }).compileComponents();

    hostFixture = TestBed.createComponent(HostComponent);
    hostFixture.detectChanges();
    await hostFixture.whenStable();
  });

  it('should create', () => {
    expect(getOrderableList()).toBeTruthy();
  });

  it('should render the add button with a descriptive aria-label', () => {
    const btn = hostFixture.debugElement.query(By.css('[aria-label="Add Skills"]'));
    expect(btn).toBeTruthy();
  });

  it('should render one accordion item per item', () => {
    const panels = hostFixture.debugElement.queryAll(By.directive(FbAccordionItem));
    expect(panels.length).toBe(2);
  });

  it('should not render the accordion when there are no items', async () => {
    const localFixture = TestBed.createComponent(HostComponent);
    localFixture.componentInstance.items = [];
    localFixture.detectChanges();
    await localFixture.whenStable();

    expect(localFixture.debugElement.query(By.directive(FbAccordion))).toBeNull();
    // The add button stays visible so an empty list can still be populated.
    expect(localFixture.debugElement.query(By.css('[aria-label="Add Skills"]'))).toBeTruthy();
  });

  it('should show the title heading when hideTitle is false', () => {
    const heading = hostFixture.debugElement.query(By.css('h2'));
    expect(heading?.nativeElement.textContent.trim()).toBe('Skills');
  });

  it('should hide the title heading when hideTitle is true', async () => {
    const localFixture = TestBed.createComponent(HostComponent);
    localFixture.componentInstance.hideTitle = true;
    localFixture.detectChanges();
    await localFixture.whenStable();
    expect(localFixture.debugElement.query(By.css('h2'))).toBeNull();
  });

  it('should emit add when the add button is clicked', () => {
    const addSpy = vi.fn();
    getOrderableList().add.subscribe(addSpy);
    hostFixture.debugElement.query(By.css('[aria-label="Add Skills"]')).nativeElement.click();
    expect(addSpy).toHaveBeenCalledTimes(1);
  });

  it('should emit delete with the matching item when delete button is clicked', () => {
    const deleteSpy = vi.fn();
    getOrderableList().delete.subscribe(deleteSpy);
    hostFixture.detectChanges();
    hostFixture.debugElement.query(By.css('[aria-label^="Delete"]')).nativeElement.click();
    expect(deleteSpy).toHaveBeenCalledWith(ITEMS[0]);
  });

  describe('delete button colour (#406)', () => {
    function deleteButton(): HTMLElement {
      return hostFixture.debugElement.query(By.css('[aria-label^="Delete"]')).nativeElement;
    }

    it('uses the destructive icon-button intent, not caller colour overrides', () => {
      const classes = deleteButton().className;
      expect(classes).toContain('text-destructive-text');
      expect(classes).not.toContain('text-muted-foreground');
    });

    it('does not use an alpha destructive hover fill (#370)', () => {
      // Alpha fills composite to plain gray over dark cards — the opaque
      // destructive-soft token is what keeps the hover state red in dark theme.
      expect(deleteButton().className).not.toMatch(/bg-destructive\/\d/);
    });
  });

  describe('add button hover colour (#407)', () => {
    function addButton(): HTMLElement {
      return hostFixture.debugElement.query(By.css('[aria-label^="Add"]')).nativeElement;
    }

    it('fills with the solid accent in dark mode', () => {
      // The issue is dark-specific: the wash measures 1.21 there, the solid
      // accent 6.55. Anchored so a `dark:hover:bg-primary/20` style wash or a
      // `-soft` derivative cannot satisfy it.
      expect(addButton().className).toMatch(/(?:^|\s)dark:hover:bg-primary(?:\s|$)/);
    });

    it('keeps a wash in light mode, raised to /38', () => {
      // Light stays translucent rather than taking the solid fill, but /10 was
      // too faint to see — /38 measures 1.29, matching the --color-surface
      // benchmark for a fill sitting on --color-card.
      expect(addButton().className).toContain('hover:bg-primary/38');
    });

    it('overrides the label only in dark mode', () => {
      // --color-muted-foreground holds 6.39 on the light wash, so light needs
      // no override at all; it collapses to 1.33 on the solid yellow, so dark
      // must have one. --color-primary-foreground (9.86) is that token —
      // --color-foreground would be wrong, it flips near-white in dark and
      // measures 1.90 on this fill.
      const classes = addButton().className;
      expect(classes).toContain('dark:hover:text-primary-foreground');
      expect(classes).toContain('text-muted-foreground');
      expect(classes).not.toMatch(/(?:^|\s)hover:text-/);
    });
  });

  it('should emit itemDrop when the CDK drop handler fires', () => {
    const component = getOrderableList();
    const dropSpy = vi.fn();
    component.itemDrop.subscribe(dropSpy);

    const mockEvent = {
      previousIndex: 0,
      currentIndex: 1,
      item: {} as unknown as CdkDrag,
      container: { data: ITEMS } as unknown as CdkDropList<Orderly[]>,
      previousContainer: { data: ITEMS } as unknown as CdkDropList<Orderly[]>,
      isPointerOverContainer: true,
      distance: { x: 0, y: 0 },
      dropPoint: { x: 0, y: 0 },
      event: new MouseEvent('drop'),
    } as CdkDragDrop<Orderly[]>;

    (component as unknown as { onDrop(e: CdkDragDrop<Orderly[]>): void }).onDrop(mockEvent);
    expect(dropSpy).toHaveBeenCalledWith(mockEvent);
  });

  describe('add button label (#454)', () => {
    it('defaults the visible label and aria-label to "Add {title}"', () => {
      const btn = hostFixture.debugElement.query(By.css('[aria-label="Add Skills"]')).nativeElement;
      expect(btn.textContent.trim()).toBe('Add Skills');
    });

    it('uses the custom addLabel for both the visible text and aria-label when provided', async () => {
      const localFixture = TestBed.createComponent(HostComponent);
      localFixture.componentInstance.addLabel = 'Add skill';
      localFixture.detectChanges();
      await localFixture.whenStable();

      const btn = localFixture.debugElement.query(By.css('[aria-label="Add skill"]'));
      expect(btn).toBeTruthy();
      expect(btn.nativeElement.textContent.trim()).toBe('Add skill');
      expect(localFixture.debugElement.query(By.css('[aria-label="Add Skills"]'))).toBeNull();
    });
  });

  describe('mobile ergonomics (#425)', () => {
    it('gives the delete button a 44px touch target below md', () => {
      const del = hostFixture.debugElement.query(By.css('[fb-icon-button][aria-label^="Delete"]'))
        .nativeElement as HTMLElement;
      expect(del).toBeTruthy();
      expect(del.classList.contains('max-md:h-11')).toBe(true);
      expect(del.classList.contains('max-md:w-11')).toBe(true);
    });

    it('trims the drag-handle gutters below md', () => {
      const handle = hostFixture.debugElement.query(By.css('[cdkDragHandle]')).nativeElement as HTMLElement;
      expect(handle.classList.contains('max-md:ml-2')).toBe(true);
    });
  });
});
