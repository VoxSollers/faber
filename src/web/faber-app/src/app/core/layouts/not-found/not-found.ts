import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Location } from '@angular/common';
import { ArrowLeft, LUCIDE_ICONS, LucideAngularModule, LucideIconProvider } from 'lucide-angular';
import { FbButton } from '../../../shared/components/fb-button/fb-button';

@Component({
  selector: 'app-not-found',
  imports: [LucideAngularModule, FbButton],
  providers: [
    {
      provide: LUCIDE_ICONS,
      multi: true,
      useValue: new LucideIconProvider({ ArrowLeft }),
    },
  ],
  templateUrl: './not-found.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'flex min-h-[calc(100vh-var(--navbar-height))] w-full items-center justify-center px-6 dotted-background',
  },
})
export class NotFound {
  private readonly location = inject(Location);

  goBack(): void {
    this.location.back();
  }
}
