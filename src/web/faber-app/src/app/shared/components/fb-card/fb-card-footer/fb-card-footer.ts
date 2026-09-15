import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { cn } from '../../../utils/cn';

@Component({
  selector: 'fb-card-footer',
  templateUrl: './fb-card-footer.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class]': 'classes()',
  },
})
export class FbCardFooter {
  readonly class = input<string>('');

  protected readonly classes = computed(() =>
    cn('flex items-center p-6 pt-0', this.class())
  );
}
