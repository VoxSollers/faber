import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  ElementRef,
  input,
  output,
  signal,
  viewChild,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { LucideAngularModule, LucideIconProvider, LUCIDE_ICONS, SquarePen, Trash2, Pencil } from 'lucide-angular';
import { Resume } from '../../resume-response';
import { FbButton } from '../../../../shared/components/fb-button/fb-button';

@Component({
  selector: 'app-resume-card',
  imports: [DatePipe, LucideAngularModule, FbButton],
  providers: [{ provide: LUCIDE_ICONS, multi: true, useValue: new LucideIconProvider({ SquarePen, Trash2, Pencil }) }],
  templateUrl: './resume-card.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResumeCard {
  resume = input.required<Resume>();
  open = output<string>();
  delete = output<string>();
  titleChange = output<{ id: string; title: string }>();

  protected readonly isEditingTitle = signal(false);

  /**
   * Live mirror of what is typed in the editor. An invisible sizer span renders
   * this alongside the input in the same grid cell, so the editing box is always
   * exactly as wide as its own text — which is what keeps the card's geometry
   * from jumping and keeps the accent underline flush with the title.
   */
  protected readonly draftTitle = signal('');
  private readonly titleInput = viewChild<ElementRef<HTMLInputElement>>('titleInput');

  protected readonly cardTitle = computed(() => this.resume().title || 'Untitled Resume');

  constructor() {
    effect(() => {
      if (this.isEditingTitle()) {
        this.titleInput()?.nativeElement.focus();
      }
    });
  }

  protected startEditingTitle(): void {
    this.draftTitle.set(this.resume().title ?? '');
    this.isEditingTitle.set(true);
  }

  protected commitTitle(value: string): void {
    const trimmed = value.trim();
    if (trimmed !== (this.resume().title ?? '')) {
      this.titleChange.emit({ id: this.resume().id, title: trimmed });
    }
    this.isEditingTitle.set(false);
  }

  protected cancelEditingTitle(): void {
    this.isEditingTitle.set(false);
  }
}
