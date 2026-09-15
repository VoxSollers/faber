import { ChangeDetectionStrategy, Component, DestroyRef, inject } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ResumesStore } from '../../resumes-store';
import { Language as LanguageModel } from './language-response';
import { UpdateLanguageRequest } from './update-language-request';
import { OrderableList } from '../orderable-list/orderable-list';
import { FbField } from '../../../../shared/components/fb-field/fb-field';
import { FbSelect } from '../../../../shared/components/fb-select/fb-select';
import { EntryTitles } from '../entry-titles';

const LANGUAGE_LEVELS = ['Native speaker', 'Highly proficient', 'Very good command', 'Good working knowledge', 'Working knowledge', 'C2', 'C1', 'B2', 'B1', 'A2', 'A1'];

@Component({
  selector: 'app-language',
  imports: [ReactiveFormsModule, OrderableList, FbField, FbSelect],
  templateUrl: './language.html',
  styleUrl: './language.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Language {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly store = inject(ResumesStore);
  protected readonly languageLevels = LANGUAGE_LEVELS;

  private readonly formMap = new Map<string, ReturnType<typeof this.buildForm>>();

  protected readonly titles = new EntryTitles(this.destroyRef);

  protected readonly languageTitle = (item: LanguageModel): string =>
    this.titles.resolve(item.id, [item.name, null]);

  protected getForm(item: LanguageModel) {
    if (!this.formMap.has(item.id)) {
      const form = this.buildForm(item);
      form.valueChanges
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(v => this.store.updateLanguage({ id: item.id, ...(v as UpdateLanguageRequest) }));
      this.titles.register(item.id, form, ['name', '']);
      this.formMap.set(item.id, form);
    }
    return this.formMap.get(item.id)!;
  }

  protected onTitleFocusIn(item: LanguageModel): void {
    this.titles.onFocusIn(item.id);
  }

  protected onTitleFocusOut(item: LanguageModel, event: FocusEvent): void {
    this.titles.onFocusOut(item.id, event);
  }

  private buildForm(item: LanguageModel) {
    return this.fb.group({
      name: [item.name ?? ''],
      level: [item.level ?? ''],
    });
  }
}
