import { ChangeDetectionStrategy, Component, DestroyRef, inject } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ResumesStore } from '../../resumes-store';
import { Skill as SkillModel } from './skill-response';
import { UpdateSkillRequest } from './update-skill-request';
import { OrderableList } from '../orderable-list/orderable-list';
import { FbField } from '../../../../shared/components/fb-field/fb-field';
import { FbSelect } from '../../../../shared/components/fb-select/fb-select';
import { EntryTitles } from '../entry-titles';

const SKILL_LEVELS = ['Beginner', 'Intermediate', 'Advanced', 'Expert'];

@Component({
  selector: 'app-skill',
  imports: [ReactiveFormsModule, OrderableList, FbField, FbSelect],
  templateUrl: './skill.html',
  styleUrl: './skill.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Skill {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly store = inject(ResumesStore);
  protected readonly skillLevels = SKILL_LEVELS;

  private readonly formMap = new Map<string, ReturnType<typeof this.buildForm>>();

  protected readonly titles = new EntryTitles(this.destroyRef);

  protected readonly skillTitle = (item: SkillModel): string =>
    this.titles.resolve(item.id, [item.name, null]);

  protected getForm(item: SkillModel) {
    if (!this.formMap.has(item.id)) {
      const form = this.buildForm(item);
      form.valueChanges
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(v => this.store.updateSkill({ id: item.id, ...(v as UpdateSkillRequest) }));
      this.titles.register(item.id, form, ['name', '']);
      this.formMap.set(item.id, form);
    }
    return this.formMap.get(item.id)!;
  }

  protected onTitleFocusIn(item: SkillModel): void {
    this.titles.onFocusIn(item.id);
  }

  protected onTitleFocusOut(item: SkillModel, event: FocusEvent): void {
    this.titles.onFocusOut(item.id, event);
  }

  private buildForm(item: SkillModel) {
    return this.fb.group({
      name: [item.name ?? ''],
      level: [item.level ?? ''],
    });
  }
}
