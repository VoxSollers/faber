import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function passwordMatchValidator(
  passwordField = 'password',
  confirmField = 'confirmPassword',
): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const password = control.get(passwordField);
    const confirm = control.get(confirmField);
    if (!password || !confirm) return null;

    if (password.value === confirm.value) {
      if (confirm.hasError('mismatch')) {
        const rest = { ...confirm.errors };
        delete rest['mismatch'];
        confirm.setErrors(Object.keys(rest).length ? rest : null);
      }
      return null;
    }

    confirm.setErrors({ ...(confirm.errors ?? {}), mismatch: true });
    return null;
  };
}
