import {
  ChangeDetectionStrategy,
  Component,
  computed,
  contentChild,
  input,
  output,
  TemplateRef,
} from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import {
  GripVertical,
  LUCIDE_ICONS,
  LucideAngularModule,
  LucideIconProvider,
  Plus,
  Trash2,
} from 'lucide-angular';
import { CdkDrag, CdkDragDrop, CdkDragHandle, CdkDropList } from '@angular/cdk/drag-drop';
import { FbIconButton } from '../../../../shared/components/fb-icon-button/fb-icon-button';
import { FbAccordion } from '../../../../shared/components/fb-accordion/fb-accordion';
import { FbAccordionItem } from '../../../../shared/components/fb-accordion-item/fb-accordion-item';
import { Orderly } from '../../orderly';

@Component({
  selector: 'app-orderable-list',
  imports: [
    NgTemplateOutlet,
    FbIconButton,
    LucideAngularModule,
    CdkDropList,
    CdkDrag,
    CdkDragHandle,
    FbAccordion,
    FbAccordionItem,
  ],
  providers: [
    { provide: LUCIDE_ICONS, multi: true, useValue: new LucideIconProvider({ Plus, GripVertical, Trash2 }) },
  ],
  templateUrl: './orderable-list.html',
  styleUrl: './orderable-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrderableList<T extends Orderly = Orderly> {
  items = input.required<T[]>();
  title = input.required<string>();
  hideTitle = input(false);
  titleFn = input.required<(item: T) => string>();
  addLabel = input<string>();

  itemTemplate = contentChild.required(TemplateRef);

  protected readonly resolvedAddLabel = computed(() => this.addLabel() ?? `Add ${this.title()}`);

  add = output<void>();
  delete = output<Orderly>();
  itemDrop = output<CdkDragDrop<Orderly[]>>();

  protected onAdd(): void {
    this.add.emit();
  }

  protected onDelete(item: T, event: Event): void {
    event.stopPropagation();
    this.delete.emit(item);
  }

  protected onDrop(event: CdkDragDrop<T[]>): void {
    this.itemDrop.emit(event as unknown as CdkDragDrop<Orderly[]>);
  }
}
