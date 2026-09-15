# Tailwind Design System: Advanced Patterns (Angular)

Advanced Tailwind CSS v4 patterns including animations, dark mode theming, custom utilities, theme modifiers, namespace overrides, and the v3-to-v4 migration checklist. **Angular adaptation.**

## Pattern 5: Native CSS Animations (v4) + Angular CDK Dialog

```css
/* In styles.css — native @starting-style for entry animations */
@theme {
  --animate-dialog-in: dialog-fade-in 0.2s ease-out;
  --animate-dialog-out: dialog-fade-out 0.15s ease-in;
}

@keyframes dialog-fade-in {
  from { opacity: 0; transform: scale(0.95) translateY(-0.5rem); }
  to { opacity: 1; transform: scale(1) translateY(0); }
}

@keyframes dialog-fade-out {
  from { opacity: 1; transform: scale(1) translateY(0); }
  to { opacity: 0; transform: scale(0.95) translateY(-0.5rem); }
}

/* Native popover animations using @starting-style */
[popover] {
  transition: opacity 0.2s, transform 0.2s, display 0.2s allow-discrete;
  opacity: 0;
  transform: scale(0.95);
}

[popover]:popover-open {
  opacity: 1;
  transform: scale(1);
}

@starting-style {
  [popover]:popover-open {
    opacity: 0;
    transform: scale(0.95);
  }
}
```

Angular CDK Dialog instead of Radix UI:

```typescript
// shared/dialog/dialog.component.ts
import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';

export interface DialogData {
  title: string;
  description?: string;
}

@Component({
  selector: 'fb-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="fixed inset-0 z-50 bg-black/80 animate-fade-in" (click)="close()"></div>
    <div class="fixed left-1/2 top-1/2 z-50 grid w-full max-w-lg -translate-x-1/2 -translate-y-1/2 gap-4 border border-border bg-background p-6 shadow-lg sm:rounded-lg animate-dialog-in">
      <div class="flex flex-col space-y-1.5">
        <h2 class="text-lg font-semibold leading-none tracking-tight">{{ data.title }}</h2>
        @if (data.description) {
          <p class="text-sm text-muted-foreground">{{ data.description }}</p>
        }
      </div>
      <ng-content />
    </div>
  `,
})
export class DialogComponent {
  readonly data = inject<DialogData>(DIALOG_DATA);
  private readonly dialogRef = inject(DialogRef);

  close() {
    this.dialogRef.close();
  }
}

// Opening a dialog from a component:
// import { Dialog } from '@angular/cdk/dialog';
//
// @Component(...)
// export class MyComponent {
//   private readonly dialog = inject(Dialog);
//
//   openDialog() {
//     this.dialog.open(DialogComponent, {
//       data: { title: 'Confirm', description: 'Are you sure?' } satisfies DialogData,
//     });
//   }
// }
```

## Pattern 6: Dark Mode with Angular Signals

Angular `ThemeService` replacing React's `ThemeProvider` context:

```typescript
// core/theme.service.ts
import { Injectable, signal, computed, effect } from '@angular/core';

type Theme = 'dark' | 'light' | 'system';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly storageKey = 'theme';
  private readonly _theme = signal<Theme>('system');

  readonly theme = this._theme.asReadonly();
  readonly resolvedTheme = computed<'dark' | 'light'>(() => {
    const theme = this._theme();
    if (theme === 'system') {
      return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }
    return theme;
  });

  constructor() {
    const stored = localStorage.getItem(this.storageKey) as Theme | null;
    if (stored) this._theme.set(stored);

    effect(() => {
      const resolved = this.resolvedTheme();
      document.documentElement.classList.remove('light', 'dark');
      document.documentElement.classList.add(resolved);

      const metaThemeColor = document.querySelector('meta[name="theme-color"]');
      metaThemeColor?.setAttribute('content', resolved === 'dark' ? '#09090b' : '#ffffff');
    });
  }

  setTheme(theme: Theme) {
    localStorage.setItem(this.storageKey, theme);
    this._theme.set(theme);
  }

  toggle() {
    this.setTheme(this.resolvedTheme() === 'dark' ? 'light' : 'dark');
  }
}
```

```typescript
// shared/theme-toggle/theme-toggle.component.ts
import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { ThemeService } from '@/core/theme.service';

@Component({
  selector: 'fb-theme-toggle',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button
      class="inline-flex size-10 items-center justify-center rounded-md hover:bg-accent hover:text-accent-foreground"
      (click)="theme.toggle()"
      [attr.aria-label]="theme.resolvedTheme() === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'"
    >
      @if (theme.resolvedTheme() === 'dark') {
        <!-- Sun icon -->
        <svg xmlns="http://www.w3.org/2000/svg" class="size-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="4"/><path d="M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M2 12h2M20 12h2M6.34 17.66l-1.41 1.41M19.07 4.93l-1.41 1.41"/>
        </svg>
      } @else {
        <!-- Moon icon -->
        <svg xmlns="http://www.w3.org/2000/svg" class="size-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/>
        </svg>
      }
    </button>
  `,
})
export class ThemeToggleComponent {
  readonly theme = inject(ThemeService);
}
```

## Advanced v4 Patterns

### Custom Utilities with `@utility`

```css
@utility line-t {
  @apply relative before:absolute before:top-0 before:-left-[100vw] before:h-px before:w-[200vw] before:bg-gray-950/5 dark:before:bg-white/10;
}

@utility text-gradient {
  @apply bg-gradient-to-r from-primary to-accent bg-clip-text text-transparent;
}
```

### Theme Modifiers

```css
/* Reference other CSS variables */
@theme inline {
  --font-sans: var(--font-inter), system-ui;
}

/* Always generate CSS variables (even when unused) */
@theme static {
  --color-brand: oklch(65% 0.15 240);
}
```

### Namespace Overrides

```css
@theme {
  /* Clear all default colors and define your own */
  --color-*: initial;
  --color-white: #fff;
  --color-black: #000;
  --color-primary: oklch(45% 0.2 260);
  --color-secondary: oklch(65% 0.15 200);
}
```

### Semi-transparent Color Variants

```css
@theme {
  --color-primary-50: color-mix(in oklab, var(--color-primary) 5%, transparent);
  --color-primary-100: color-mix(in oklab, var(--color-primary) 10%, transparent);
  --color-primary-200: color-mix(in oklab, var(--color-primary) 20%, transparent);
}
```

### Container Queries

```css
@theme {
  --container-xs: 20rem;
  --container-sm: 24rem;
  --container-md: 28rem;
  --container-lg: 32rem;
}
```

## v3 to v4 Migration Checklist

- [ ] Replace `tailwind.config.ts` with CSS `@theme` block
- [ ] Change `@tailwind base/components/utilities` to `@import "tailwindcss"`
- [ ] Move color definitions to `@theme { --color-*: value }`
- [ ] Replace `darkMode: "class"` with `@custom-variant dark`
- [ ] Move `@keyframes` inside `@theme` blocks
- [ ] Replace `require("tailwindcss-animate")` with native CSS animations
- [ ] Update `h-10 w-10` to `size-10`
- [ ] Consider OKLCH colors for better color perception
- [ ] Replace custom plugins with `@utility` directives

## Best Practices

### Do's

- **Use `@theme` blocks** — CSS-first configuration is v4's core pattern
- **Use OKLCH colors** — better perceptual uniformity than HSL
- **Use CVA** — type-safe variants with `class-variance-authority`
- **Use semantic tokens** — `bg-primary` not `bg-blue-500`
- **Use `size-*`** — shorthand for `w-* h-*`
- **Add accessibility** — ARIA attributes, focus states
- **Use `host` binding** — apply variant classes on the host element, not a wrapper div
- **Use `input()` signals** — Angular v20+ reactive inputs for variants

### Don'ts

- **Don't use `tailwind.config.ts`** — use CSS `@theme` instead
- **Don't use `@tailwind` directives** — use `@import "tailwindcss"`
- **Don't use arbitrary values** — extend `@theme` instead
- **Don't hardcode colors** — use semantic tokens
- **Don't forget dark mode** — test both themes
- **Don't use `[ngClass]` for variants** — use CVA + `computed()` with `host: { '[class]': 'classes()' }`
