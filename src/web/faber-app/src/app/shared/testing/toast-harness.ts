import { ComponentFixture } from '@angular/core/testing';

export const toastHarness = (fixture: ComponentFixture<unknown>) => {
  const root = (): HTMLElement => fixture.nativeElement as HTMLElement;
  return {
    query: () => root().querySelector('[role="alert"]'),
    text: () => root().querySelector('[role="alert"]')?.textContent ?? '',
    dismiss: () => {
      const btn = root().querySelector<HTMLButtonElement>('button[aria-label="Dismiss"]');
      if (!btn) {
        throw new Error('toastHarness.dismiss: dismiss button not in DOM');
      }
      btn.click();
    },
  };
};
