import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Component } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { FbDatePicker } from './fb-date-picker';

@Component({
  template: `<fb-date-picker
    [formControl]="ctrl"
    [label]="label"
    [allowPresent]="allowPresent"
    [precision]="precision"
    [locale]="locale" />`,
  imports: [FbDatePicker, ReactiveFormsModule],
})
class TestHost {
  ctrl = new FormControl<string | null>(null);
  label = 'Start date';
  allowPresent = false;
  precision: 'day' | 'month' | 'year' = 'month';
  locale = 'en-us';
}

describe('FbDatePicker', () => {
  let hostFixture: ComponentFixture<TestHost>;
  let host: TestHost;
  let nativeEl: HTMLElement;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHost],
      providers: [provideNoopAnimations()],
    }).compileComponents();

    hostFixture = TestBed.createComponent(TestHost);
    host = hostFixture.componentInstance;
    nativeEl = hostFixture.nativeElement;
    // detectChanges() is NOT called here — each test calls it with the right initial state
  });

  // --- Rendering ---

  it('should create', () => {
    hostFixture.detectChanges();
    expect(host).toBeTruthy();
  });

  it('should render the label', () => {
    hostFixture.detectChanges();
    const label = nativeEl.querySelector('label[for]') as HTMLLabelElement;
    expect(label?.textContent?.trim()).toBe('Start date');
  });

  it('should not render label when label input is empty', async () => {
    host.label = '';
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    expect(nativeEl.querySelector('label[for]')).toBeNull();
  });

  it('should bind label for to the date input id', () => {
    hostFixture.detectChanges();
    const label = nativeEl.querySelector('label[for]') as HTMLLabelElement;
    const input = nativeEl.querySelector('input:not([type="checkbox"])') as HTMLInputElement;
    expect(label.htmlFor).toBeTruthy();
    expect(label.htmlFor).toBe(input.id);
  });

  // --- CVA: value binding ---

  it('should display ISO value as month+year in input', async () => {
    host.ctrl.setValue('2023-06-01');
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const input = nativeEl.querySelector('input:not([type="checkbox"])') as HTMLInputElement;
    expect(input.value).toContain('2023');
    expect(input.value.toLowerCase()).toContain('jun');
  });

  it('should propagate month selection as ISO string with day 01', async () => {
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    const pickerComp = hostFixture.debugElement.query(By.directive(FbDatePicker))
      .componentInstance as FbDatePicker;
    const mockPicker = { close: () => {} } as never;
    pickerComp.onMonthSelected(new Date(2024, 2, 1), mockPicker);
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    expect(host.ctrl.value).toMatch(/^2024-03-01$/);
  });

  // --- precision ---

  it('should display the full day for precision=day', async () => {
    host.precision = 'day';
    host.ctrl.setValue('1990-05-15');
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const input = nativeEl.querySelector('input:not([type="checkbox"])') as HTMLInputElement;
    expect(input.value).toContain('15');
    expect(input.value.toLowerCase()).toContain('may');
    expect(input.value).toContain('1990');
  });

  it('should ignore month selection for precision=day', async () => {
    host.precision = 'day';
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    const pickerComp = hostFixture.debugElement.query(By.directive(FbDatePicker))
      .componentInstance as FbDatePicker;
    const mockPicker = { close: () => {} } as never;
    pickerComp.onMonthSelected(new Date(2024, 2, 1), mockPicker);
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    expect(host.ctrl.value).toBeNull();
  });

  it('should propagate year selection as ISO string pinned to 01-01 for precision=year', async () => {
    host.precision = 'year';
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    const pickerComp = hostFixture.debugElement.query(By.directive(FbDatePicker))
      .componentInstance as FbDatePicker;
    const mockPicker = { close: () => {} } as never;
    pickerComp.onYearSelected(new Date(2030, 7, 20), mockPicker);
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    expect(host.ctrl.value).toMatch(/^2030-01-01$/);
  });

  // --- allowPresent ---

  it('should start in Present state (placeholder + readonly, not disabled) when allowPresent and value is null', async () => {
    // Present must NOT disable the input — that would also disable the calendar toggle,
    // leaving no way to reopen the panel and switch back off.
    host.allowPresent = true;
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const input = nativeEl.querySelector('input') as HTMLInputElement;
    expect(input.placeholder).toBe('Present');
    expect(input.readOnly).toBe(true);
    expect(input.disabled).toBe(false);
  });

  it('should not enter Present state when allowPresent is false and value is null', async () => {
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const input = nativeEl.querySelector('input') as HTMLInputElement;
    expect(input.placeholder).not.toBe('Present');
    expect(input.readOnly).toBe(false);
  });

  it('should clear the date and emit null when Present is toggled on', async () => {
    host.allowPresent = true;
    host.ctrl.setValue('2023-06-01'); // Start with a date so isPresent is false
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    const pickerComp = hostFixture.debugElement.query(By.directive(FbDatePicker))
      .componentInstance as FbDatePicker;
    pickerComp.togglePresent();
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    expect(host.ctrl.value).toBeNull();
    const input = nativeEl.querySelector('input') as HTMLInputElement;
    expect(input.placeholder).toBe('Present');
    expect(input.disabled).toBe(false);
  });

  it('should leave Present and emit an ISO date when a month is selected', async () => {
    host.allowPresent = true; // starts present (null)
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    const pickerComp = hostFixture.debugElement.query(By.directive(FbDatePicker))
      .componentInstance as FbDatePicker;
    const mockPicker = { close: () => {} } as never;
    pickerComp.onMonthSelected(new Date(2024, 2, 1), mockPicker);
    hostFixture.detectChanges();
    await hostFixture.whenStable();

    expect(host.ctrl.value).toMatch(/^2024-03-01$/);
    const input = nativeEl.querySelector('input') as HTMLInputElement;
    expect(input.readOnly).toBe(false);
  });

  // --- setDisabledState ---

  it('should disable the date input when the form control is disabled', async () => {
    host.ctrl.disable();
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const dateInput = nativeEl.querySelector('input:not([type="checkbox"])') as HTMLInputElement;
    expect(dateInput.disabled).toBe(true);
  });

  it('should re-enable date input when control is re-enabled', async () => {
    host.ctrl.disable();
    hostFixture.detectChanges();
    host.ctrl.enable();
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const dateInput = nativeEl.querySelector('input:not([type="checkbox"])') as HTMLInputElement;
    expect(dateInput.disabled).toBe(false);
  });

  // --- Error message ---

  it('should render error message when control has endBeforeStart error and is touched', async () => {
    hostFixture.detectChanges();
    host.ctrl.setErrors({ endBeforeStart: true });
    host.ctrl.markAsTouched();
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const error = nativeEl.querySelector('[role="alert"]');
    expect(error?.textContent?.trim()).toContain('End date must be after start date');
  });

  it('should not render error when control is untouched', () => {
    hostFixture.detectChanges();
    host.ctrl.setErrors({ endBeforeStart: true });
    hostFixture.detectChanges();
    expect(nativeEl.querySelector('[role="alert"]')).toBeNull();
  });

  it('should set aria-invalid when control has error and is touched', async () => {
    hostFixture.detectChanges();
    host.ctrl.setErrors({ endBeforeStart: true });
    host.ctrl.markAsTouched();
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const input = nativeEl.querySelector('input:not([type="checkbox"])') as HTMLInputElement;
    expect(input.getAttribute('aria-invalid')).toBe('true');
  });

  it('should wire aria-describedby to the error span id', async () => {
    hostFixture.detectChanges();
    host.ctrl.setErrors({ endBeforeStart: true });
    host.ctrl.markAsTouched();
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const input = nativeEl.querySelector('input:not([type="checkbox"])') as HTMLInputElement;
    const errorSpan = nativeEl.querySelector('[role="alert"]') as HTMLElement;
    expect(input.getAttribute('aria-describedby')).toBe(errorSpan.id);
  });
});
