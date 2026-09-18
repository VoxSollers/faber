import { ChangeDetectionStrategy, Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ResumesStore } from '../../resumes-store';
import { Project as ProjectModel } from './project-response';
import { UpdateProjectRequest } from './update-project-request';
import { OrderableList } from '../orderable-list/orderable-list';
import { FbField } from '../../../../shared/components/fb-field/fb-field';
import { FbDatePicker } from '../../../../shared/components/fb-date-picker/fb-date-picker';
import { FbRichText } from '../../../../shared/components/fb-rich-text/fb-rich-text';
import { dateRangeValidator } from '../../../../shared/validators/date-range.validator';
import { EntryTitles } from '../entry-titles';

@Component({
  selector: 'app-project',
  imports: [ReactiveFormsModule, OrderableList, FbField, FbDatePicker, FbRichText],
  templateUrl: './project.html',
  styleUrl: './project.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Project {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly store = inject(ResumesStore);

  private readonly formMap = new Map<string, ReturnType<typeof this.buildForm>>();
  protected readonly titles = new EntryTitles(this.destroyRef);

  protected readonly projectTitle = (item: ProjectModel): string =>
    this.titles.resolve(item.id, [item.name, item.role]);

  protected getForm(item: ProjectModel) {
    if (!this.formMap.has(item.id)) {
      const form = this.buildForm(item);
      form.valueChanges
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(value => this.store.updateProject({ id: item.id, ...(value as UpdateProjectRequest) }));
      this.titles.register(item.id, form, ['name', 'role']);
      this.formMap.set(item.id, form);
    }
    return this.formMap.get(item.id)!;
  }

  protected onTitleFocusIn(item: ProjectModel): void {
    this.titles.onFocusIn(item.id);
  }

  protected onTitleFocusOut(item: ProjectModel, event: FocusEvent): void {
    this.titles.onFocusOut(item.id, event);
  }

  private buildForm(item: ProjectModel) {
    return this.fb.group(
      {
        name: [item.name ?? ''],
        role: [item.role ?? ''],
        url: [item.url ?? ''],
        startDate: [item.startDate ?? ''],
        endDate: [item.endDate ?? ''],
        description: [item.description ?? ''],
      },
      { validators: dateRangeValidator() },
    );
  }
}
