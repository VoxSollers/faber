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
  selector: 'fb-select',
  templateUrl: './fb-select.html',
  styleUrl: './fb-select.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FbSelect implements ControlValueAccessor {
  readonly label = input('');
  readonly placeholder = input('Select…');
  readonly options = input<readonly string[]>([]);
  readonly hint = input('');

  protected readonly value = signal('');
  protected readonly focused = signal(false);
  protected readonly disabled = signal(false);

  protected readonly labelClasses = computed(() =>
    this.focused() ? 'text-foreground' : 'text-muted-foreground',
  );

  readonly selectId = `fb-select-${Math.random().toString(36).slice(2, 9)}`;
  readonly hintId = `${this.selectId}-hint`;

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

  onSelect(event: Event): void {
    const val = (event.target as HTMLSelectElement).value;
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
