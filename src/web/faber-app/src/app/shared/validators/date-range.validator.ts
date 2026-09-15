import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { isBefore, isValid, parseISO } from 'date-fns';

// Remove a stale endBeforeStart error while preserving any other errors.
function clearEndBeforeStartError(control: AbstractControl): void {
  if (!control.hasError('endBeforeStart')) {
    return;
  }
  const rest = { ...control.errors };
  delete rest['endBeforeStart'];
  control.setErrors(Object.keys(rest).length ? rest : null);
}

export function dateRangeValidator(
  startField = 'startDate',
  endField = 'endDate',
): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const start = control.get(startField);
    const end = control.get(endField);
    if (!start || !end) return null;

    const startVal = start.value as string | null;
    const endVal = end.value as string | null;

    // null/empty end means Present (ongoing) → always valid
    if (!startVal || !endVal) {
      clearEndBeforeStartError(end);
      return null;
    }

    const startDate = parseISO(startVal);
    const endDate = parseISO(endVal);

    if (!isValid(startDate) || !isValid(endDate)) {
      return null;
    }

    if (isBefore(endDate, startDate)) {
      end.setErrors({ ...(end.errors ?? {}), endBeforeStart: true });
      return null;
    }

    clearEndBeforeStartError(end);
    return null;
  };
}
