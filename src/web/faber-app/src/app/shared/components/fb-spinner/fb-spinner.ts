import { ChangeDetectionStrategy, Component, input } from '@angular/core';

type Size = 'small' | 'medium' | 'large';

@Component({
  selector: 'fb-spinner',
  templateUrl: './fb-spinner.html',
  styleUrl: './fb-spinner.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FbSpinner {
  size = input<Size>('medium');
}
