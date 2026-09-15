import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { FbSelect } from './fb-select';
import { expectNoAxeViolations } from '../../testing/axe';

@Component({
  template: `<fb-select [formControl]="ctrl" [label]="label"
                        [placeholder]="placeholder" [options]="options" [hint]="hint" />`,
  imports: [FbSelect, ReactiveFormsModule],
})
class TestHost {
  ctrl = new FormControl('');
  label = 'Level';
  placeholder = 'Select level';
  options: readonly string[] = ['Beginner', 'Intermediate', 'Advanced', 'Expert'];
  hint = '';
}

describe('FbSelect', () => {
  let hostFixture: ComponentFixture<TestHost>;
  let host: TestHost;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestHost],
    }).compileComponents();

    hostFixture = TestBed.createComponent(TestHost);
    host = hostFixture.componentInstance;
  });

  // --- Rendering ---

  it('should create', () => {
    expect(host).toBeTruthy();
  });

  it('should render the label', () => {
    hostFixture.detectChanges();
    const label = hostFixture.nativeElement.querySelector('label');
    expect(label?.textContent?.trim()).toBe('Level');
  });

  it('should not render label when label input is empty', async () => {
    host.label = '';
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    expect(hostFixture.nativeElement.querySelector('label')).toBeNull();
  });

  it('should bind label "for" to the select id', () => {
    hostFixture.detectChanges();
    const label = hostFixture.nativeElement.querySelector('label') as HTMLLabelElement;
    const select = hostFixture.nativeElement.querySelector('select') as HTMLSelectElement;
    expect(select.id).toBeTruthy();
    expect(label.htmlFor).toBe(select.id);
  });

  it('should render the placeholder option plus one option per value', () => {
    hostFixture.detectChanges();
    const options = hostFixture.nativeElement.querySelectorAll('option');
    expect(options.length).toBe(host.options.length + 1);
    expect((options[0] as HTMLOptionElement).value).toBe('');
    expect((options[0] as HTMLOptionElement).textContent?.trim()).toBe('Select level');
    expect((options[1] as HTMLOptionElement).value).toBe('Beginner');
  });

  // --- CVA: value binding ---

  it('should display value set via writeValue', async () => {
    host.ctrl.setValue('Advanced');
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const select = hostFixture.nativeElement.querySelector('select') as HTMLSelectElement;
    expect(select.value).toBe('Advanced');
  });

  it('should propagate the selected value to the form control', () => {
    hostFixture.detectChanges();
    const select = hostFixture.nativeElement.querySelector('select') as HTMLSelectElement;
    select.value = 'Expert';
    select.dispatchEvent(new Event('change'));
    expect(host.ctrl.value).toBe('Expert');
  });

  it('should mark control touched on blur', () => {
    hostFixture.detectChanges();
    const select = hostFixture.nativeElement.querySelector('select') as HTMLSelectElement;
    select.dispatchEvent(new Event('focus'));
    select.dispatchEvent(new Event('blur'));
    hostFixture.detectChanges();
    expect(host.ctrl.touched).toBe(true);
  });

  it('should disable the native select when the control is disabled', async () => {
    host.ctrl.disable();
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const select = hostFixture.nativeElement.querySelector('select') as HTMLSelectElement;
    expect(select.disabled).toBe(true);
  });

  // --- Focus state ---

  it('should highlight the label on focus and revert on blur', async () => {
    hostFixture.detectChanges();
    const select = hostFixture.nativeElement.querySelector('select') as HTMLSelectElement;
    const label = hostFixture.nativeElement.querySelector('label') as HTMLLabelElement;

    select.dispatchEvent(new Event('focus'));
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    expect(label.classList.contains('text-foreground')).toBe(true);

    select.dispatchEvent(new Event('blur'));
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    expect(label.classList.contains('text-muted-foreground')).toBe(true);
  });

  // --- Accessibility ---

  it('should have no AXE violations', async () => {
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    await expectNoAxeViolations(hostFixture);
  }, 20000);
});
