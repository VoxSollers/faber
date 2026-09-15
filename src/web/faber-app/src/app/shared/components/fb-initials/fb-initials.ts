import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'fb-initials',
  templateUrl: './fb-initials.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FbInitials {
  firstName = input.required<string>();
  lastName = input.required<string>();
  firstChar = computed(() => this.firstName().charAt(0).toUpperCase());
  lastChar = computed(() => this.lastName().charAt(0).toUpperCase());
}
