import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { FbField } from './fb-field';

@Component({
  template: `<fb-field [formControl]="ctrl" [label]="label" [type]="type"
                       [placeholder]="placeholder" [hint]="hint" />`,
  imports: [FbField, ReactiveFormsModule],
})
class TestHost {
  ctrl = new FormControl('');
  label = 'Job title';
  type: 'text' | 'email' | 'tel' | 'url' | 'number' = 'text';
  placeholder = 'e.g. Frontend Developer';
  hint = '';
}

describe('FbField', () => {
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
    expect(hostFixture.componentInstance).toBeTruthy();
  });

  it('should render the label', () => {
    hostFixture.detectChanges();
    const label = hostFixture.nativeElement.querySelector('label');
    expect(label?.textContent?.trim()).toBe('Job title');
  });

  it('should not render label when label input is empty', async () => {
    host.label = '';
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const label = hostFixture.nativeElement.querySelector('label');
    expect(label).toBeNull();
  });

  it('should bind label "for" to the input id', () => {
    hostFixture.detectChanges();
    const label = hostFixture.nativeElement.querySelector('label') as HTMLLabelElement;
    const input = hostFixture.nativeElement.querySelector('input') as HTMLInputElement;
    expect(input.id).toBeTruthy();
    expect(label.htmlFor).toBe(input.id);
  });

  it('should render placeholder on native input', () => {
    hostFixture.detectChanges();
    const input = hostFixture.nativeElement.querySelector('input') as HTMLInputElement;
    expect(input.placeholder).toBe('e.g. Frontend Developer');
  });

  it('should bind the type input to the native input type', async () => {
    host.type = 'email';
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const input = hostFixture.nativeElement.querySelector('input') as HTMLInputElement;
    expect(input.type).toBe('email');
  });

  // --- CVA: value binding ---

  it('should display value set via writeValue', async () => {
    host.ctrl.setValue('Frontend Developer');
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const input = hostFixture.nativeElement.querySelector('input') as HTMLInputElement;
    expect(input.value).toBe('Frontend Developer');
  });

  it('should propagate typed value to the form control', () => {
    hostFixture.detectChanges();
    const input = hostFixture.nativeElement.querySelector('input') as HTMLInputElement;
    input.value = 'typed';
    input.dispatchEvent(new Event('input'));
    expect(host.ctrl.value).toBe('typed');
  });

  it('should mark control touched on blur', () => {
    hostFixture.detectChanges();
    const input = hostFixture.nativeElement.querySelector('input') as HTMLInputElement;
    input.dispatchEvent(new Event('focus'));
    input.dispatchEvent(new Event('blur'));
    hostFixture.detectChanges();
    expect(host.ctrl.touched).toBe(true);
  });

  it('should disable the native input when the control is disabled', async () => {
    host.ctrl.disable();
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const input = hostFixture.nativeElement.querySelector('input') as HTMLInputElement;
    expect(input.disabled).toBe(true);
  });

  // --- Focus state ---

  it('should highlight the label on focus and revert on blur', async () => {
    hostFixture.detectChanges();
    const input = hostFixture.nativeElement.querySelector('input') as HTMLInputElement;
    const label = hostFixture.nativeElement.querySelector('label') as HTMLLabelElement;

    input.dispatchEvent(new Event('focus'));
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    expect(label.classList.contains('text-foreground')).toBe(true);

    input.dispatchEvent(new Event('blur'));
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    expect(label.classList.contains('text-muted-foreground')).toBe(true);
  });

  // --- Hint ---

  it('should not render hint or aria-describedby when hint is empty', () => {
    hostFixture.detectChanges();
    const input = hostFixture.nativeElement.querySelector('input') as HTMLInputElement;
    expect(hostFixture.nativeElement.querySelector('span')).toBeNull();
    expect(input.getAttribute('aria-describedby')).toBeNull();
  });

  it('should render hint and wire it via aria-describedby', async () => {
    host.hint = "Where you're based";
    hostFixture.detectChanges();
    await hostFixture.whenStable();
    const input = hostFixture.nativeElement.querySelector('input') as HTMLInputElement;
    const hintEl = hostFixture.nativeElement.querySelector('span') as HTMLSpanElement;
    expect(hintEl.textContent?.trim()).toBe("Where you're based");
    expect(hintEl.id).toBeTruthy();
    expect(input.getAttribute('aria-describedby')).toBe(hintEl.id);
  });
});
