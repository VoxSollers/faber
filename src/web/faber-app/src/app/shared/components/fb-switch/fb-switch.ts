import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { ControlValueAccessor, NgControl } from '@angular/forms';

/**
 * Sliding on/off switch. Works two ways:
 * - as a reactive-forms control via `formControlName` / `[formControl]` (ControlValueAccessor), or
 * - as a controlled element via `[checked]` + `(checkedChange)` when no form control is attached.
 */
@Component({
  selector: 'fb-switch',
  templateUrl: './fb-switch.html',
  styleUrl: './fb-switch.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FbSwitch implements ControlValueAccessor {
  /** Controlled checked state — used only when the switch is NOT bound to a form control. */
  readonly checked = input(false);
  readonly disabled = input(false);
  readonly label = input('');
  readonly ariaLabel = input('');

  readonly checkedChange = output<boolean>();

  protected readonly modelChecked = signal(false);
  protected readonly modelDisabled = signal(false);

  private readonly ngControl = inject(NgControl, { self: true, optional: true });

  /** Form-bound instances follow the control's value; controlled instances follow the input. */
  protected readonly displayChecked = computed(() =>
    this.ngControl ? this.modelChecked() : this.checked(),
  );
  protected readonly displayDisabled = computed(() =>
    this.ngControl ? this.modelDisabled() : this.disabled(),
  );

  readonly inputId = `fb-switch-${Math.random().toString(36).slice(2, 9)}`;

  private onChange: (v: boolean) => void = () => {};
  private onTouched: () => void = () => {};

  constructor() {
    if (this.ngControl) {
      this.ngControl.valueAccessor = this;
    }
  }

  writeValue(val: boolean): void {
    this.modelChecked.set(!!val);
  }

  registerOnChange(fn: (v: boolean) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.modelDisabled.set(isDisabled);
  }

  protected onToggle(event: Event): void {
    const next = (event.target as HTMLInputElement).checked;
    if (this.ngControl) {
      this.modelChecked.set(next);
      this.onChange(next);
    }
    this.onTouched();
    this.checkedChange.emit(next);
  }

  protected onBlur(): void {
    this.onTouched();
  }
}
