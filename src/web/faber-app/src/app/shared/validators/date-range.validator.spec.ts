import { FormControl, FormGroup } from '@angular/forms';
import { dateRangeValidator } from './date-range.validator';

function makeGroup(startDate: string | null, endDate: string | null) {
  return new FormGroup(
    {
      startDate: new FormControl(startDate),
      endDate: new FormControl(endDate),
    },
    { validators: [dateRangeValidator()] },
  );
}

describe('dateRangeValidator', () => {
  it('should return null (group has no error) when end is after start', () => {
    const group = makeGroup('2022-01-01', '2024-06-01');
    expect(group.errors).toBeNull();
  });

  it('should not set endBeforeStart when range is valid', () => {
    const group = makeGroup('2022-01-01', '2024-06-01');
    expect(group.get('endDate')?.hasError('endBeforeStart')).toBeFalsy();
  });

  it('should set endBeforeStart on end control when end is before start', () => {
    const group = makeGroup('2024-06-01', '2023-01-01');
    expect(group.get('endDate')?.hasError('endBeforeStart')).toBe(true);
  });

  it('should set error on end control only, not on the group', () => {
    const group = makeGroup('2024-06-01', '2023-01-01');
    expect(group.errors).toBeNull();
  });

  it('should pass when end is null (Present)', () => {
    const group = makeGroup('2024-06-01', null);
    expect(group.get('endDate')?.hasError('endBeforeStart')).toBeFalsy();
  });

  it('should pass when end is empty string (Present equivalent)', () => {
    const group = makeGroup('2024-06-01', '');
    expect(group.get('endDate')?.hasError('endBeforeStart')).toBeFalsy();
  });

  it('should pass when start is null', () => {
    const group = makeGroup(null, '2024-06-01');
    expect(group.get('endDate')?.hasError('endBeforeStart')).toBeFalsy();
  });

  it('should pass when both are null', () => {
    const group = makeGroup(null, null);
    expect(group.get('endDate')?.hasError('endBeforeStart')).toBeFalsy();
  });

  it('should clear endBeforeStart when the range becomes valid', () => {
    const group = makeGroup('2024-06-01', '2023-01-01');
    expect(group.get('endDate')?.hasError('endBeforeStart')).toBe(true);

    group.get('startDate')?.setValue('2022-01-01');
    expect(group.get('endDate')?.hasError('endBeforeStart')).toBeFalsy();
  });

  it('should clear endBeforeStart when end becomes null (Present)', () => {
    const group = makeGroup('2024-06-01', '2023-01-01');
    expect(group.get('endDate')?.hasError('endBeforeStart')).toBe(true);

    group.get('endDate')?.setValue(null);
    expect(group.get('endDate')?.hasError('endBeforeStart')).toBeFalsy();
  });

  it('should preserve other errors on end control when adding endBeforeStart', () => {
    const group = makeGroup('2024-06-01', '2023-01-01');
    const endCtrl = group.get('endDate')!;

    // Add another error to the end control
    endCtrl.setErrors({ ...(endCtrl.errors ?? {}), required: true });
    // Re-run group validators
    group.updateValueAndValidity();

    expect(endCtrl.hasError('endBeforeStart')).toBe(true);
    expect(endCtrl.hasError('required')).toBe(true);
  });

  it('should clear endBeforeStart but preserve other errors when range becomes valid', () => {
    const group = makeGroup('2024-06-01', '2023-01-01');
    const endCtrl = group.get('endDate')!;

    endCtrl.setErrors({ ...(endCtrl.errors ?? {}), required: true });
    group.updateValueAndValidity();
    expect(endCtrl.hasError('endBeforeStart')).toBe(true);

    group.get('startDate')?.setValue('2022-01-01');
    expect(endCtrl.hasError('endBeforeStart')).toBeFalsy();
    expect(endCtrl.hasError('required')).toBe(true);
  });

  it('should use custom field names when provided', () => {
    const group = new FormGroup(
      {
        from: new FormControl('2024-06-01'),
        to: new FormControl('2023-01-01'),
      },
      { validators: [dateRangeValidator('from', 'to')] },
    );
    expect(group.get('to')?.hasError('endBeforeStart')).toBe(true);
  });
});
