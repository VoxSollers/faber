import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ResumesStore } from '../../resumes-store';
import { FbField } from '../../../../shared/components/fb-field/fb-field';
import { FbDatePicker } from '../../../../shared/components/fb-date-picker/fb-date-picker';

@Component({
  selector: 'app-person',
  imports: [ReactiveFormsModule, FbField, FbDatePicker],
  templateUrl: './person.html',
  styleUrl: './person.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Person {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly store = inject(ResumesStore);

  protected readonly form = this.fb.group({
    jobTitle: [''],
    firstname: [''],
    lastname: [''],
    email: [''],
    phone: [''],
    country: [''],
    city: [''],
    street: [''],
    postCode: [''],
    nationality: [''],
    dateOfBirth: [''],
    drivingLicense: [''],
  });

  constructor() {
    effect(() => {
      const person = this.store.person();
      if (person) {
        this.form.patchValue(person, { emitEvent: false });
      }
    });

    this.form.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.store.updatePerson(this.form.getRawValue()));
  }
}
