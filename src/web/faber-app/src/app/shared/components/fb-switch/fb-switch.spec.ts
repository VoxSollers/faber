import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { FbSwitch } from './fb-switch';
import { expectNoAxeViolations } from '../../testing/axe';

@Component({
  template: `<fb-switch
    [checked]="checked"
    [disabled]="disabled"
    label="Present"
    (checkedChange)="onChange($event)" />`,
  imports: [FbSwitch],
})
class ControlledHost {
  checked = false;
  disabled = false;
  lastEmitted: boolean | null = null;
  onChange(value: boolean): void {
    this.lastEmitted = value;
  }
}

@Component({
  template: `<fb-switch [formControl]="ctrl" ariaLabel="Toggle" />`,
  imports: [FbSwitch, ReactiveFormsModule],
})
class FormHost {
  ctrl = new FormControl(false);
}

const checkboxOf = (el: HTMLElement) => el.querySelector('input[type="checkbox"]') as HTMLInputElement;

describe('FbSwitch', () => {
  describe('controlled mode', () => {
    let fixture: ComponentFixture<ControlledHost>;
    let host: ControlledHost;

    beforeEach(async () => {
      await TestBed.configureTestingModule({ imports: [ControlledHost] }).compileComponents();
      fixture = TestBed.createComponent(ControlledHost);
      host = fixture.componentInstance;
      // detectChanges() is NOT called here — each test sets state first, then renders.
    });

    it('should create', () => {
      fixture.detectChanges();
      expect(host).toBeTruthy();
    });

    it('should render a switch role and the label text', () => {
      fixture.detectChanges();
      const checkbox = checkboxOf(fixture.nativeElement);
      expect(checkbox.getAttribute('role')).toBe('switch');
      expect(fixture.nativeElement.textContent).toContain('Present');
    });

    it('should reflect the checked input', async () => {
      host.checked = true;
      fixture.detectChanges();
      await fixture.whenStable();
      expect(checkboxOf(fixture.nativeElement).checked).toBe(true);
    });

    it('should emit checkedChange with the new value when toggled', () => {
      fixture.detectChanges();
      const checkbox = checkboxOf(fixture.nativeElement);
      checkbox.checked = true;
      checkbox.dispatchEvent(new Event('change'));
      expect(host.lastEmitted).toBe(true);
    });

    it('should disable the checkbox when the disabled input is set', async () => {
      host.disabled = true;
      fixture.detectChanges();
      await fixture.whenStable();
      expect(checkboxOf(fixture.nativeElement).disabled).toBe(true);
    });
  });

  describe('reactive-forms mode', () => {
    let fixture: ComponentFixture<FormHost>;
    let host: FormHost;

    beforeEach(async () => {
      await TestBed.configureTestingModule({ imports: [FormHost] }).compileComponents();
      fixture = TestBed.createComponent(FormHost);
      host = fixture.componentInstance;
      fixture.detectChanges();
      await fixture.whenStable();
    });

    it('should reflect the control value (writeValue)', async () => {
      host.ctrl.setValue(true);
      fixture.detectChanges();
      await fixture.whenStable();
      expect(checkboxOf(fixture.nativeElement).checked).toBe(true);
    });

    it('should write the toggled value back to the control', () => {
      const checkbox = checkboxOf(fixture.nativeElement);
      checkbox.checked = true;
      checkbox.dispatchEvent(new Event('change'));
      fixture.detectChanges();
      expect(host.ctrl.value).toBe(true);
    });

    it('should disable the checkbox when the control is disabled', async () => {
      host.ctrl.disable();
      fixture.detectChanges();
      await fixture.whenStable();
      expect(checkboxOf(fixture.nativeElement).disabled).toBe(true);
    });

    it('should have no AXE violations', async () => {
      await expectNoAxeViolations(fixture);
    }, 20000);
  });
});
