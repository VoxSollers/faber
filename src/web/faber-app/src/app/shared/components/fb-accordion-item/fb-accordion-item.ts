import {
  booleanAttribute,
  ChangeDetectionStrategy,
  Component,
  effect,
  ElementRef,
  inject,
  input,
  model,
  OnDestroy,
  OnInit,
  viewChild,
} from '@angular/core';
import { LucideAngularModule } from 'lucide-angular';
import { FbAccordion } from '../fb-accordion/fb-accordion';

@Component({
  selector: 'fb-accordion-item',
  imports: [LucideAngularModule],
  templateUrl: './fb-accordion-item.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block shrink-0',
    '[class.open]': 'open()',
  },
})
export class FbAccordionItem implements OnInit, OnDestroy {
  readonly label = input.required<string>();
  readonly icon = input<string>();
  readonly count = input<number>();
  readonly open = model(false);
  /** Render the chevron's hover background as a circle instead of a rounded square. */
  readonly roundChevron = input(false, { transform: booleanAttribute });

  private readonly accordion = inject(FbAccordion, { optional: true });
  private readonly body = viewChild.required<ElementRef<HTMLElement>>('body');

  protected readonly triggerId = `fb-ai-t-${Math.random().toString(36).slice(2, 9)}`;
  protected readonly bodyId = `fb-ai-b-${Math.random().toString(36).slice(2, 9)}`;

  constructor() {
    // Single source of truth for single-open enforcement: whenever this item
    // becomes open — via click, [(open)], or an external open.set(true) — ask
    // the accordion to close its siblings. Writes only OTHER items' open signal,
    // so it never re-triggers itself; in allowMultiple mode itemOpened no-ops.
    effect(() => {
      if (this.open()) {
        this.accordion?.itemOpened(this);
      }
    });
  }

  ngOnInit(): void {
    this.accordion?.register(this);
  }

  ngOnDestroy(): void {
    this.accordion?.unregister(this);
  }

  protected toggle(): void {
    // Re-enable the open/close animation that collapseForDrag may have suppressed.
    this.body().nativeElement.style.removeProperty('transition');
    this.open.set(!this.open());
    // Single-open enforcement happens reactively in the constructor effect().
  }

  /**
   * Collapses the item synchronously as a drag is grabbed (handle pointerdown),
   * writing the closed state straight to the DOM. CDK measures the drag boundary
   * on the following pointermove, and zoneless change detection may not have
   * flushed by then — so the imperative write guarantees the element is already
   * at its closed height, keeping the vertical constraint correct for open items.
   */
  collapseForDrag(): void {
    const el = this.body().nativeElement;
    el.style.transition = 'none';
    el.style.gridTemplateRows = '0fr';
    this.open.set(false);
  }
}
