import { FormControl, FormGroup } from '@angular/forms';
import { passwordMatchValidator } from './password-match.validator';

describe('passwordMatchValidator', () => {
  it('should leave confirm clean when passwords match', () => {
    const group = new FormGroup(
      {
        password: new FormControl('Password1'),
        confirmPassword: new FormControl('Password1'),
      },
      { validators: passwordMatchValidator() },
    );

    expect(group.errors).toBeNull();
    expect(group.controls.confirmPassword.hasError('mismatch')).toBe(false);
  });

  it('should set mismatch on confirm control when passwords differ', () => {
    const group = new FormGroup(
      {
        password: new FormControl('Password1'),
        confirmPassword: new FormControl('Different1'),
      },
      { validators: passwordMatchValidator() },
    );

    expect(group.errors).toBeNull();
    expect(group.controls.password.hasError('mismatch')).toBe(false);
    expect(group.controls.confirmPassword.hasError('mismatch')).toBe(true);
  });

  it('should work with custom field names', () => {
    const group = new FormGroup(
      {
        newPassword: new FormControl('Password1'),
        confirmNewPassword: new FormControl('Password1'),
      },
      { validators: passwordMatchValidator('newPassword', 'confirmNewPassword') },
    );

    expect(group.errors).toBeNull();
    expect(group.controls.confirmNewPassword.hasError('mismatch')).toBe(false);
  });

  it('should detect mismatch with custom field names', () => {
    const group = new FormGroup(
      {
        newPassword: new FormControl('Password1'),
        confirmNewPassword: new FormControl('Different1'),
      },
      { validators: passwordMatchValidator('newPassword', 'confirmNewPassword') },
    );

    expect(group.errors).toBeNull();
    expect(group.controls.confirmNewPassword.hasError('mismatch')).toBe(true);
  });

  it('should return null if password field is missing', () => {
    const group = new FormGroup(
      {
        confirmPassword: new FormControl('Password1'),
      },
      { validators: passwordMatchValidator() },
    );

    expect(group.errors).toBeNull();
  });

  it('should clear mismatch when values become equal again', () => {
    const group = new FormGroup(
      {
        password: new FormControl('Password1'),
        confirmPassword: new FormControl('Different1'),
      },
      { validators: passwordMatchValidator() },
    );

    expect(group.controls.confirmPassword.hasError('mismatch')).toBe(true);

    group.controls.confirmPassword.setValue('Password1');

    expect(group.controls.confirmPassword.hasError('mismatch')).toBe(false);
    expect(group.controls.confirmPassword.errors).toBeNull();
  });

  it('should preserve other errors on confirm control when stripping mismatch', () => {
    const required = (c: FormControl): { required: true } | null => (c.value ? null : { required: true });
    const group = new FormGroup(
      {
        password: new FormControl('Password1'),
        confirmPassword: new FormControl('Different1', { validators: [(c) => required(c as FormControl)] }),
      },
      { validators: passwordMatchValidator() },
    );

    expect(group.controls.confirmPassword.hasError('mismatch')).toBe(true);

    group.controls.confirmPassword.setValue('Password1');

    expect(group.controls.confirmPassword.hasError('mismatch')).toBe(false);
  });
});
