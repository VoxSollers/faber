import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  signal,
} from '@angular/core';
import { LucideAngularModule } from 'lucide-angular';
import { ResumesStore } from '../../resumes-store';
import { Localization } from '../localization/localization';

/** Which way the action island stacks its controls. */
export type ActionsOrientation = 'vertical' | 'horizontal';

/**
 * Save status, localization and Download PDF — the three controls a user needs
 * while editing, regardless of whether a PDF preview is on screen.
 *
 * These lived in the old preview-only toolbar component and were rendered
 * only alongside the desktop preview column, so all three vanished on a
 * phone (#425). The component is now orientation-aware: a vertical pill
 * beside the desktop preview, a horizontal group inside the mobile bottom bar.
 */
@Component({
  selector: 'app-builder-actions',
  imports: [LucideAngularModule, Localization],
  templateUrl: './builder-actions.html',
  styleUrl: './builder-actions.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class]': 'hostClasses()',
    role: 'group',
    'aria-label': 'Resume actions',
  },
})
export class BuilderActions {
  protected readonly store = inject(ResumesStore);

  readonly orientation = input<ActionsOrientation>('vertical');

  /** The dropdown must open away from the island: leftwards beside the desktop
   *  preview, upwards out of the mobile bottom bar. */
  protected readonly menuPlacement = computed(() =>
    this.orientation() === 'vertical' ? ('left' as const) : ('top' as const),
  );

  protected readonly hostClasses = computed(() =>
    this.orientation() === 'vertical'
      ? 'self-center flex flex-col items-center gap-1.5 shrink-0 bg-card border-[0.5px] border-line rounded-full p-1.5'
      : 'flex flex-row items-center gap-1 shrink-0',
  );

  // Gates the save-indicator's enter animation until after first render. The
  // component normally mounts after the resume has loaded, with `saveStatus`
  // already 'saved'. Without this gate the initial server-backed state would
  // play the same success animation as a freshly completed edit, even though
  // no transition happened on screen. This mirrors fb-theme-toggle's guard.
  protected readonly animationsReady = signal(false);

  /** The single, persistent announcement for the save indicator. Empty for
   *  'idle' and 'saving' so only the terminal state — 'saved' or 'error' — is
   *  ever read out, and exactly once per transition into it. */
  protected readonly saveAnnouncement = computed(() => {
    switch (this.store.saveStatus()) {
      case 'saved':
        return 'Saved';
      case 'error':
        return this.store.syncErrorKind() === 'preview' ? 'Sync failed' : 'Save failed';
      default:
        return '';
    }
  });

  protected readonly errorLabel = computed(() =>
    this.store.syncErrorKind() === 'preview' ? 'Sync failed' : 'Save failed',
  );

  constructor() {
    afterNextRender(() => this.animationsReady.set(true));
  }
}
