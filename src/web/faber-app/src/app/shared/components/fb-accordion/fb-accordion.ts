import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FbAccordionItem } from '../fb-accordion-item/fb-accordion-item';

@Component({
  selector: 'fb-accordion',
  templateUrl: './fb-accordion.html',
  styleUrl: './fb-accordion.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'flex flex-col' },
})
export class FbAccordion {
  readonly allowMultiple = input(false);

  private readonly _items: FbAccordionItem[] = [];

  register(item: FbAccordionItem): void {
    this._items.push(item);
  }

  unregister(item: FbAccordionItem): void {
    const idx = this._items.indexOf(item);
    if (idx !== -1) this._items.splice(idx, 1);
  }

  itemOpened(item: FbAccordionItem): void {
    if (!this.allowMultiple()) {
      this._items.filter(i => i !== item).forEach(i => i.open.set(false));
    }
  }
}
