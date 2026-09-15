import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class Offset {
  private readonly offsetHeight = signal(0);
  readonly height = this.offsetHeight.asReadonly();

  setHeight(value: number): void {
    this.offsetHeight.set(value);
  }
}
