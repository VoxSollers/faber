import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { cn } from '../../../utils/cn';

@Component({
  selector: 'fb-card-header',
  templateUrl: './fb-card-header.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class]': 'classes()',
  },
})
export class FbCardHeader {
  readonly class = input<string>('');

  protected readonly classes = computed(() =>
    cn('flex flex-col gap-1.5 p-6', this.class())
  );
}
