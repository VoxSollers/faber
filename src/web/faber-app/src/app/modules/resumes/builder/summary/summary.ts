import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs';
import { ResumesStore } from '../../resumes-store';
import { FbRichText } from '../../../../shared/components/fb-rich-text/fb-rich-text';

@Component({
  selector: 'app-summary',
  imports: [ReactiveFormsModule, FbRichText],
  templateUrl: './summary.html',
  styleUrl: './summary.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Summary {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly store = inject(ResumesStore);

  protected readonly control = this.fb.control('');

  constructor() {
    effect(() => {
      const value = this.store.summary();
      if (value !== this.control.value) {
        this.control.setValue(value, { emitEvent: false });
      }
    });

    this.control.valueChanges
      .pipe(
        filter(() => this.control.valid),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(html => this.store.updateSummary(html));
  }
}
