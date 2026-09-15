import { ChangeDetectionStrategy, Component, DestroyRef, inject } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ResumesStore } from '../../resumes-store';
import { Link as LinkModel } from './link-response';
import { UpdateLinkRequest } from './update-link-request';
import { OrderableList } from '../orderable-list/orderable-list';
import { FbField } from '../../../../shared/components/fb-field/fb-field';
import { EntryTitles, firstNonEmptyTitlePart } from '../entry-titles';

@Component({
  selector: 'app-link',
  imports: [ReactiveFormsModule, OrderableList, FbField],
  templateUrl: './link.html',
  styleUrl: './link.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Link {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly store = inject(ResumesStore);

  private readonly formMap = new Map<string, ReturnType<typeof this.buildForm>>();

  protected readonly titles = new EntryTitles(this.destroyRef);

  protected readonly linkTitle = (item: LinkModel): string =>
    this.titles.resolve(item.id, [item.label, item.uri], firstNonEmptyTitlePart);

  protected getForm(item: LinkModel) {
    if (!this.formMap.has(item.id)) {
      const form = this.buildForm(item);
      form.valueChanges
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(v => this.store.updateLink({ id: item.id, ...(v as UpdateLinkRequest) }));
      this.titles.register(item.id, form, ['label', 'uri'], firstNonEmptyTitlePart);
      this.formMap.set(item.id, form);
    }
    return this.formMap.get(item.id)!;
  }

  protected onTitleFocusIn(item: LinkModel): void {
    this.titles.onFocusIn(item.id);
  }

  protected onTitleFocusOut(item: LinkModel, event: FocusEvent): void {
    this.titles.onFocusOut(item.id, event);
  }

  private buildForm(item: LinkModel) {
    return this.fb.group({
      label: [item.label ?? ''],
      uri: [item.uri ?? ''],
    });
  }
}
