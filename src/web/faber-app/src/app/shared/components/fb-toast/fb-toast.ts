import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  output,
} from '@angular/core';
import {
  LucideAngularModule,
  LucideIconProvider,
  LUCIDE_ICONS,
  CircleAlert,
  CircleCheck,
  TriangleAlert,
  Info,
  X,
} from 'lucide-angular';
import { cva } from 'class-variance-authority';

export type ToastVariant = 'error' | 'success' | 'warning' | 'info';

const AUTO_DISMISS_MS = 7000;

const toastVariants = cva(
  'glass-surface fixed bottom-6 left-1/2 -translate-x-1/2 z-[200] flex items-center gap-3 rounded-full whitespace-nowrap max-w-[90vw] py-2.5 pl-5 pr-2',
  {
    variants: {
      variant: {
        error:   'bg-destructive/10 text-destructive-text',
        success: 'bg-success/10 text-success-text',
        warning: 'bg-warning/10 text-warning-text',
        info:    'bg-info/10 text-info-text',
      },
    },
    defaultVariants: { variant: 'error' },
  }
);

@Component({
  selector: 'fb-toast',
  imports: [LucideAngularModule],
  providers: [
    {
      provide: LUCIDE_ICONS,
      multi: true,
      useValue: new LucideIconProvider({ CircleAlert, CircleCheck, TriangleAlert, Info, X }),
    },
  ],
  templateUrl: './fb-toast.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FbToast {
  message = input<string | null>(null);
  variant = input<ToastVariant>('error');

  dismissed = output<void>();

  private readonly destroyRef = inject(DestroyRef);
  private timer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    effect(() => {
      const msg = this.message();
      this.clearTimer();
      if (!msg) return;
      this.timer = setTimeout(() => this.dismiss(), AUTO_DISMISS_MS);
    });

    this.destroyRef.onDestroy(() => this.clearTimer());
  }

  protected readonly iconName = computed<string>(() => {
    switch (this.variant()) {
      case 'success': return 'circle-check';
      case 'warning': return 'triangle-alert';
      case 'info':    return 'info';
      default:        return 'circle-alert';
    }
  });

  protected readonly containerClass = computed(() => toastVariants({ variant: this.variant() }));

  protected readonly ariaRole = computed<'status' | 'alert'>(() =>
    this.variant() === 'success' || this.variant() === 'info' ? 'status' : 'alert',
  );

  protected readonly ariaLive = computed<'polite' | 'assertive'>(() =>
    this.variant() === 'success' || this.variant() === 'info' ? 'polite' : 'assertive',
  );

  protected dismiss(): void {
    this.clearTimer();
    this.dismissed.emit();
  }

  private clearTimer(): void {
    if (this.timer) {
      clearTimeout(this.timer);
      this.timer = null;
    }
  }
}
