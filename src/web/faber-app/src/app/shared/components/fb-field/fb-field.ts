import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';

@Component({
  selector: 'fb-field',
  templateUrl: './fb-field.html',
  styleUrl: './fb-field.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FbField implements ControlValueAccessor {
  readonly label = input('');
  readonly type = input<'text' | 'email' | 'tel' | 'url' | 'number'>('text');
  readonly placeholder = input('');
  readonly autocomplete = input('off');
  readonly hint = input('');

  protected readonly value = signal('');
  protected readonly focused = signal(false);
  protected readonly disabled = signal(false);

  protected readonly labelClasses = computed(() =>
    this.focused() ? 'text-foreground' : 'text-muted-foreground',
  );

  readonly inputId = `fb-field-${Math.random().toString(36).slice(2, 9)}`;
  readonly hintId = `${this.inputId}-hint`;

  private readonly ngControl = inject(NgControl, { self: true, optional: true });

  private onChange: (v: string) => void = () => {};
  private onTouched: () => void = () => {};

  constructor() {
    if (this.ngControl) {
      this.ngControl.valueAccessor = this;
    }
  }

  writeValue(val: string): void {
    this.value.set(val ?? '');
  }

  registerOnChange(fn: (v: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  onInput(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.value.set(val);
    this.onChange(val);
  }

  onFocus(): void {
    this.focused.set(true);
  }

  onBlur(): void {
    this.focused.set(false);
    this.onTouched();
  }
}
