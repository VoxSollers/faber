import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { cn } from '../../../utils/cn';

@Component({
  selector: 'fb-card-title',
  templateUrl: './fb-card-title.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class]': 'classes()',
  },
})
export class FbCardTitle {
  readonly class = input<string>('');

  protected readonly classes = computed(() =>
    cn('text-xl font-semibold leading-none tracking-tight', this.class())
  );
}
