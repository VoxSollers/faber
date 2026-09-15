import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'fb-form-field',
  imports: [],
  templateUrl: './fb-form-field.html',
  styleUrl: './fb-form-field.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FbFormField {
  label = input('');
  hint = input('');
  inputId = input('');
}
