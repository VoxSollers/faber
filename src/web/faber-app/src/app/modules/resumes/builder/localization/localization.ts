import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  ElementRef,
  inject,
  input,
  OnInit,
  signal,
} from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { LucideAngularModule } from 'lucide-angular';
import { ResumesStore } from '../../resumes-store';
import { LANGUAGES, LANGUAGE_MAP } from '../../../../shared/utils/language-map';

@Component({
  selector: 'app-localization',
  imports: [LucideAngularModule],
  templateUrl: './localization.html',
  styleUrl: './localization.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'relative flex' },
})
export class Localization implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly elementRef = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly document = inject(DOCUMENT);
  protected readonly store = inject(ResumesStore);

  /** Which edge the dropdown flies out from: `'left'` beside the desktop
   *  action pill, `'top'` out of the mobile bottom bar. */
  readonly placement = input<'left' | 'top'>('left');

  protected readonly languages = LANGUAGES;
  protected readonly open = signal(false);
  protected readonly selected = computed(() => LANGUAGE_MAP.get(this.store.localization()) ?? null);

  ngOnInit(): void {
    this.registerOutsideClickListener();
  }

  protected toggle(): void {
    this.open.update(value => !value);
  }

  protected select(locale: string): void {
    this.store.updateLocalization(locale);
    this.open.set(false);
  }

  // Capture phase runs document → target before any bubble-phase
  // stopPropagation, mirroring the user-menu dismissal in the builder shell.
  private registerOutsideClickListener(): void {
    const handler = (event: MouseEvent): void => this.handleOutsideClick(event);
    this.document.addEventListener('click', handler, { capture: true });
    this.destroyRef.onDestroy(() =>
      this.document.removeEventListener('click', handler, { capture: true }),
    );
  }

  private handleOutsideClick(event: MouseEvent): void {
    if (!this.open()) {
      return;
    }
    if (this.elementRef.nativeElement.contains(event.target as Node)) {
      return;
    }
    this.open.set(false);
  }
}
