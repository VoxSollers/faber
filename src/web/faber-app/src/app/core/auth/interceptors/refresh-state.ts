import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class RefreshState {
  isRefreshing = false;
  readonly refreshSubject = new BehaviorSubject<string | null>(null);

  reset(): void {
    this.isRefreshing = false;
    this.refreshSubject.next(null);
  }
}
