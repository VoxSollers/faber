import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  input,
  signal,
  OnInit,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ControlValueAccessor, FormControl, NgControl, ReactiveFormsModule } from '@angular/forms';
import { DateAdapter, MAT_DATE_FORMATS } from '@angular/material/core';
import {
  MatDatepicker,
  MatDatepickerActions,
  MatDatepickerInput,
  MatDatepickerToggle,
} from '@angular/material/datepicker';
import { provideDateFnsAdapter } from '@angular/material-date-fns-adapter';
import { enUS, pl } from 'date-fns/locale';
import type { Locale } from 'date-fns';
import { isValid, parseISO } from 'date-fns';
import {
  createDatePickerFormats,
  displayFormatFor,
  isoFor,
  setMonthAndYear,
  setYearOnly,
  type DatePrecision,
} from '../../utils/date-formats';
import { FbSwitch } from '../fb-switch/fb-switch';

const LOCALE_MAP: Record<string, Locale> = {
  'en-us': enUS,
  'en': enUS,
  'pl-pl': pl,
  'pl': pl,
};

@Component({
  selector: 'fb-date-picker',
  templateUrl: './fb-date-picker.html',
  styleUrl: './fb-date-picker.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    provideDateFnsAdapter(),
    { provide: MAT_DATE_FORMATS, useFactory: createDatePickerFormats },
  ],
  imports: [
    ReactiveFormsModule,
    MatDatepicker,
    MatDatepickerActions,
    MatDatepickerInput,
    MatDatepickerToggle,
    FbSwitch,
  ],
})
export class FbDatePicker implements ControlValueAccessor, OnInit {
  readonly label = input('');
  readonly placeholder = input('Month & year');
  readonly allowPresent = input(false);
  readonly locale = input('en-us');
  readonly precision = input<DatePrecision>('day');

  protected readonly isPresent = signal(false);
  protected readonly focused = signal(false);
  protected readonly internalCtrl = new FormControl<Date | null>(null);

  private readonly controlTick = signal(0);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dateAdapter = inject(DateAdapter<Date>);
  private readonly dateFormats = inject(MAT_DATE_FORMATS);
  private readonly ngControl = inject(NgControl, { self: true, optional: true });

  protected readonly startView = computed<'month' | 'multi-year'>(() =>
    this.precision() === 'day' ? 'month' : 'multi-year',
  );

  readonly inputId = `fb-date-picker-${Math.random().toString(36).slice(2, 9)}`;
  readonly errorId = `${this.inputId}-error`;

  protected readonly hasError = computed(() => {
    this.controlTick();
    const ctrl = this.ngControl?.control;
    return !!(ctrl?.invalid && ctrl.touched);
  });

  protected readonly errorMessage = computed(() => {
    this.controlTick();
    const ctrl = this.ngControl?.control;
    if (!ctrl?.touched) return null;
    if (ctrl.hasError('endBeforeStart')) return 'End date must be after start date.';
    return null;
  });

  protected readonly labelClasses = computed(() =>
    this.focused() ? 'text-foreground' : 'text-muted-foreground',
  );

  protected readonly inputClasses = computed(() => {
    const base =
      'w-full rounded-[10px] border-none bg-input pl-3.5 pr-10 py-2.5 ' +
      'font-[inherit] text-[15px] leading-[1.4] text-foreground ' +
      'placeholder:text-muted-foreground/60 outline-none transition-[box-shadow,background-color] ' +
      'disabled:cursor-not-allowed disabled:opacity-60';
    if (this.hasError()) {
      return `${base} shadow-[inset_0_0_0_1px_var(--color-destructive-text)]`;
    }
    return (
      `${base} focus:bg-card ` +
      'focus:shadow-[inset_0_0_0_1px_color-mix(in_oklab,var(--color-foreground)_22%,transparent),' +
      '0_1px_2px_rgba(0,0,0,0.05),0_5px_16px_rgba(0,0,0,0.11)]'
    );
  });

  private onChange: (v: string | null) => void = () => {};
  private onTouched: () => void = () => {};

  constructor() {
    if (this.ngControl) {
      this.ngControl.valueAccessor = this;
    }

    effect(() => {
      const code = this.locale().toLowerCase();
      this.dateAdapter.setLocale(LOCALE_MAP[code] ?? enUS);
    });

    effect(() => {
      const pattern = displayFormatFor(this.precision());
      this.dateFormats.display.dateInput = pattern;
      this.dateFormats.parse.dateInput = pattern;
      // Force MatDatepickerInput to re-render the visible text with the new pattern.
      const value = this.internalCtrl.value;
      if (value) {
        this.internalCtrl.setValue(new Date(value), { emitEvent: false });
      }
    });
  }

  ngOnInit(): void {
    this.trackControlEvents();
    this.internalCtrl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(date => {
        if (!this.isPresent()) {
          this.onChange(date ? isoFor(date, this.precision()) : null);
        }
      });
  }

  private trackControlEvents(): void {
    const ctrl = this.ngControl?.control;
    if (!ctrl) return;
    ctrl.events
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.controlTick.update(n => n + 1));
  }

  onMonthSelected(normalized: Date, picker: MatDatepicker<Date>): void {
    if (this.precision() !== 'month') {
      return;
    }
    this.isPresent.set(false);
    setMonthAndYear(this.internalCtrl, normalized, picker);
    this.onTouched();
  }

  onYearSelected(normalized: Date, picker: MatDatepicker<Date>): void {
    if (this.precision() !== 'year') {
      return;
    }
    this.isPresent.set(false);
    setYearOnly(this.internalCtrl, normalized, picker);
    this.onTouched();
  }

  togglePresent(): void {
    const next = !this.isPresent();
    this.isPresent.set(next);
    if (next) {
      // Present clears the chosen date. The input stays enabled (just readonly), so
      // the calendar toggle — and the in-panel Present action — remain reachable to switch back.
      this.internalCtrl.setValue(null, { emitEvent: false });
      this.onChange(null);
    } else {
      const date = this.internalCtrl.value;
      this.onChange(date ? isoFor(date, this.precision()) : null);
    }
    this.onTouched();
  }

  onFocus(): void {
    this.focused.set(true);
  }

  onBlur(): void {
    this.focused.set(false);
    this.onTouched();
  }

  writeValue(val: string | null): void {
    const isEmpty = !val;
    if (isEmpty) {
      // An empty value with allowPresent reads as "Present"; otherwise it's just unset.
      this.isPresent.set(this.allowPresent());
      this.internalCtrl.setValue(null, { emitEvent: false });
      return;
    }
    this.isPresent.set(false);
    const parsed = parseISO(val);
    this.internalCtrl.setValue(isValid(parsed) ? parsed : null, { emitEvent: false });
  }

  registerOnChange(fn: (v: string | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    if (isDisabled) {
      this.internalCtrl.disable({ emitEvent: false });
    } else {
      this.internalCtrl.enable({ emitEvent: false });
    }
  }
}
