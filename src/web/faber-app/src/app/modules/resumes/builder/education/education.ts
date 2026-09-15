import { ChangeDetectionStrategy, Component, DestroyRef, inject } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ResumesStore } from '../../resumes-store';
import { Education as EducationModel } from './education-response';
import { UpdateEducationRequest } from './update-education-request';
import { OrderableList } from '../orderable-list/orderable-list';
import { FbField } from '../../../../shared/components/fb-field/fb-field';
import { FbDatePicker } from '../../../../shared/components/fb-date-picker/fb-date-picker';
import { FbRichText } from '../../../../shared/components/fb-rich-text/fb-rich-text';
import { dateRangeValidator } from '../../../../shared/validators/date-range.validator';
import { EntryTitles } from '../entry-titles';

@Component({
  selector: 'app-education',
  imports: [ReactiveFormsModule, OrderableList, FbField, FbDatePicker, FbRichText],
  templateUrl: './education.html',
  styleUrl: './education.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Education {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly store = inject(ResumesStore);

  private readonly formMap = new Map<string, ReturnType<typeof this.buildForm>>();

  // Local display state: derived from the live form value so the accordion
  // title updates immediately while typing, while `ResumesStore` keeps
  // meaning "what the server confirmed" and the save below stays debounced.
  protected readonly titles = new EntryTitles(this.destroyRef);

  protected readonly educationTitle = (item: EducationModel): string =>
    this.titles.resolve(item.id, [item.degree, item.school]);

  protected getForm(item: EducationModel) {
    if (!this.formMap.has(item.id)) {
      const form = this.buildForm(item);
      form.valueChanges
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(v => this.store.updateEducation({ id: item.id, ...(v as UpdateEducationRequest) }));
      this.titles.register(item.id, form, ['degree', 'school']);
      this.formMap.set(item.id, form);
    }
    return this.formMap.get(item.id)!;
  }

  protected onTitleFocusIn(item: EducationModel): void {
    this.titles.onFocusIn(item.id);
  }

  protected onTitleFocusOut(item: EducationModel, event: FocusEvent): void {
    this.titles.onFocusOut(item.id, event);
  }

  private buildForm(item: EducationModel) {
    return this.fb.group(
      {
        school: [item.school ?? ''],
        degree: [item.degree ?? ''],
        startDate: [item.startDate ?? ''],
        endDate: [item.endDate ?? ''],
        city: [item.city ?? ''],
        description: [item.description ?? ''],
      },
      { validators: dateRangeValidator() },
    );
  }
}
