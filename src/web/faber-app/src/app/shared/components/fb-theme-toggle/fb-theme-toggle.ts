import { afterNextRender, ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { LucideAngularModule, LucideIconProvider, LUCIDE_ICONS, Moon, Sun } from 'lucide-angular';
import { FbIconButton } from '../fb-icon-button/fb-icon-button';
import { ThemeService } from '../../../core/services/theme';

@Component({
  selector: 'fb-theme-toggle',
  imports: [LucideAngularModule, FbIconButton],
  providers: [
    {
      provide: LUCIDE_ICONS,
      multi: true,
      useValue: new LucideIconProvider({ Moon, Sun }),
    },
  ],
  templateUrl: './fb-theme-toggle.html',
  styleUrl: './fb-theme-toggle.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FbThemeToggle {
  protected readonly themeService = inject(ThemeService);
  protected readonly animationsReady = signal(false);

  constructor() {
    afterNextRender(() => this.animationsReady.set(true));
  }
}
