---
name: tailwind-design-system
description: Use when creating component libraries, implementing design systems, standardizing UI patterns, or configuring Tailwind CSS v4 tokens and theming in Angular projects.
---

# Tailwind Design System (v4 + Angular)

Build production-ready design systems with Tailwind CSS v4, including CSS-first configuration, design tokens, component variants, responsive patterns, and accessibility. **Angular adaptation** — all CSS patterns are universal; component patterns use Angular standalone components with signals.

> **Note**: This skill targets Tailwind CSS v4 (2024+). For v3 projects, refer to the [upgrade guide](https://tailwindcss.com/docs/upgrade-guide).

## When to Use This Skill

- Creating a component library with Tailwind v4 in Angular
- Implementing design tokens and theming with CSS-first configuration
- Building responsive and accessible Angular components
- Standardizing UI patterns across a codebase
- Migrating from Tailwind v3 to v4
- Setting up dark mode with native CSS features

## Key v4 Changes

| v3 Pattern                            | v4 Pattern                                                            |
| ------------------------------------- | --------------------------------------------------------------------- |
| `tailwind.config.ts`                  | `@theme` in CSS                                                       |
| `@tailwind base/components/utilities` | `@import "tailwindcss"`                                               |
| `darkMode: "class"`                   | `@custom-variant dark (&:where(.dark, .dark *))`                      |
| `theme.extend.colors`                 | `@theme { --color-*: value }`                                         |
| `require("tailwindcss-animate")`      | CSS `@keyframes` in `@theme` + `@starting-style` for entry animations |

## Quick Start

```css
/* styles.css - Tailwind v4 CSS-first configuration */
@import "tailwindcss";

/* Define your theme with @theme */
@theme {
  /* Semantic color tokens using OKLCH for better color perception */
  --color-background: oklch(100% 0 0);
  --color-foreground: oklch(14.5% 0.025 264);

  --color-primary: oklch(14.5% 0.025 264);
  --color-primary-foreground: oklch(98% 0.01 264);

  --color-secondary: oklch(96% 0.01 264);
  --color-secondary-foreground: oklch(14.5% 0.025 264);

  --color-muted: oklch(96% 0.01 264);
  --color-muted-foreground: oklch(46% 0.02 264);

  --color-accent: oklch(96% 0.01 264);
  --color-accent-foreground: oklch(14.5% 0.025 264);

  --color-destructive: oklch(53% 0.22 27);
  --color-destructive-foreground: oklch(98% 0.01 264);

  --color-border: oklch(91% 0.01 264);
  --color-ring: oklch(14.5% 0.025 264);

  --color-card: oklch(100% 0 0);
  --color-card-foreground: oklch(14.5% 0.025 264);

  --color-ring-offset: oklch(100% 0 0);

  /* Radius tokens */
  --radius-sm: 0.25rem;
  --radius-md: 0.375rem;
  --radius-lg: 0.5rem;
  --radius-xl: 0.75rem;

  /* Animation tokens */
  --animate-fade-in: fade-in 0.2s ease-out;
  --animate-fade-out: fade-out 0.2s ease-in;
  --animate-slide-in: slide-in 0.3s ease-out;
  --animate-slide-out: slide-out 0.3s ease-in;

  @keyframes fade-in {
    from { opacity: 0; }
    to { opacity: 1; }
  }

  @keyframes fade-out {
    from { opacity: 1; }
    to { opacity: 0; }
  }

  @keyframes slide-in {
    from { transform: translateY(-0.5rem); opacity: 0; }
    to { transform: translateY(0); opacity: 1; }
  }

  @keyframes slide-out {
    from { transform: translateY(0); opacity: 1; }
    to { transform: translateY(-0.5rem); opacity: 0; }
  }
}

/* Dark mode variant */
@custom-variant dark (&:where(.dark, .dark *));

/* Dark mode theme overrides */
.dark {
  --color-background: oklch(14.5% 0.025 264);
  --color-foreground: oklch(98% 0.01 264);

  --color-primary: oklch(98% 0.01 264);
  --color-primary-foreground: oklch(14.5% 0.025 264);

  --color-secondary: oklch(22% 0.02 264);
  --color-secondary-foreground: oklch(98% 0.01 264);

  --color-muted: oklch(22% 0.02 264);
  --color-muted-foreground: oklch(65% 0.02 264);

  --color-accent: oklch(22% 0.02 264);
  --color-accent-foreground: oklch(98% 0.01 264);

  --color-destructive: oklch(42% 0.15 27);
  --color-destructive-foreground: oklch(98% 0.01 264);

  --color-border: oklch(22% 0.02 264);
  --color-ring: oklch(83% 0.02 264);

  --color-card: oklch(14.5% 0.025 264);
  --color-card-foreground: oklch(98% 0.01 264);

  --color-ring-offset: oklch(14.5% 0.025 264);
}

/* Base styles */
@layer base {
  * {
    @apply border-border;
  }

  body {
    @apply bg-background text-foreground antialiased;
  }
}
```

## Core Concepts

### 1. Design Token Hierarchy

```
Brand Tokens (abstract)
    └── Semantic Tokens (purpose)
        └── Component Tokens (specific)

Example:
    oklch(45% 0.2 260) → --color-primary → bg-primary
```

### 2. Component Architecture

```
Base styles → Variants → Sizes → States → Overrides
```

## Patterns

### Pattern 1: CVA Variants in Angular

`class-variance-authority` is framework-agnostic — use it with Angular `input()` signals and `host` binding.

```typescript
// shared/button/button.component.ts
import { Component, ChangeDetectionStrategy, computed, input } from '@angular/core';
import { cva } from 'class-variance-authority';
import { cn } from '@/lib/utils';

const buttonVariants = cva(
  'inline-flex items-center justify-center whitespace-nowrap rounded-md text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50',
  {
    variants: {
      variant: {
        default: 'bg-primary text-primary-foreground hover:bg-primary/90',
        destructive: 'bg-destructive text-destructive-foreground hover:bg-destructive/90',
        outline: 'border border-border bg-background hover:bg-accent hover:text-accent-foreground',
        secondary: 'bg-secondary text-secondary-foreground hover:bg-secondary/80',
        ghost: 'hover:bg-accent hover:text-accent-foreground',
        link: 'text-primary underline-offset-4 hover:underline',
      },
      size: {
        default: 'h-10 px-4 py-2',
        sm: 'h-9 rounded-md px-3',
        lg: 'h-11 rounded-md px-8',
        icon: 'size-10',
      },
    },
    defaultVariants: { variant: 'default', size: 'default' },
  }
);

@Component({
  selector: 'fb-button',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[class]': 'classes()' },
  template: `<ng-content />`,
})
export class ButtonComponent {
  variant = input<'default' | 'destructive' | 'outline' | 'secondary' | 'ghost' | 'link'>('default');
  size = input<'default' | 'sm' | 'lg' | 'icon'>('default');
  class = input<string>('');

  classes = computed(() =>
    cn(buttonVariants({ variant: this.variant(), size: this.size() }), this.class())
  );
}

// Usage
// <fb-button variant="destructive" size="lg">Delete</fb-button>
// <fb-button variant="outline">Cancel</fb-button>
```

### Pattern 2: Compound Components via Content Projection

Angular uses `<ng-content>` instead of children props.

```typescript
// shared/card/card.component.ts
@Component({
  selector: 'fb-card',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'rounded-lg border border-border bg-card text-card-foreground shadow-sm block' },
  template: `<ng-content />`,
})
export class CardComponent {}

@Component({
  selector: 'fb-card-header',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'flex flex-col space-y-1.5 p-6 block' },
  template: `<ng-content />`,
})
export class CardHeaderComponent {}

@Component({
  selector: 'fb-card-title',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'text-2xl font-semibold leading-none tracking-tight block' },
  template: `<ng-content />`,
})
export class CardTitleComponent {}

@Component({
  selector: 'fb-card-description',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'text-sm text-muted-foreground block' },
  template: `<ng-content />`,
})
export class CardDescriptionComponent {}

@Component({
  selector: 'fb-card-content',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'p-6 pt-0 block' },
  template: `<ng-content />`,
})
export class CardContentComponent {}

@Component({
  selector: 'fb-card-footer',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'flex items-center p-6 pt-0 block' },
  template: `<ng-content />`,
})
export class CardFooterComponent {}

// Usage
// <fb-card>
//   <fb-card-header>
//     <fb-card-title>Account</fb-card-title>
//     <fb-card-description>Manage your account settings</fb-card-description>
//   </fb-card-header>
//   <fb-card-content>
//     <form>...</form>
//   </fb-card-content>
//   <fb-card-footer>
//     <fb-button>Save</fb-button>
//   </fb-card-footer>
// </fb-card>
```

### Pattern 3: Form Input Component with Reactive Forms

```typescript
// shared/input/input.component.ts
import { Component, ChangeDetectionStrategy, computed, input } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { cn } from '@/lib/utils';

@Component({
  selector: 'fb-input',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule],
  template: `
    <div class="relative">
      <input
        [type]="type()"
        [class]="inputClasses()"
        [formControl]="control()"
        [id]="id()"
        [attr.aria-invalid]="!!error() || null"
        [attr.aria-describedby]="error() ? id() + '-error' : null"
      />
      @if (error()) {
        <p [id]="id() + '-error'" class="mt-1 text-sm text-destructive" role="alert">
          {{ error() }}
        </p>
      }
    </div>
  `,
})
export class InputComponent {
  type = input<string>('text');
  id = input<string>('');
  control = input.required<FormControl>();
  error = input<string | null>(null);

  inputClasses = computed(() => cn(
    'flex h-10 w-full rounded-md border border-border bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50',
    this.error() && 'border-destructive focus-visible:ring-destructive'
  ));
}

// Usage with Reactive Forms
// In component:
// form = new FormGroup({
//   email: new FormControl('', [Validators.required, Validators.email]),
//   password: new FormControl('', [Validators.required, Validators.minLength(8)]),
// });
// emailError = computed(() => this.form.controls.email.touched && this.form.controls.email.errors?.['email'] ? 'Invalid email address' : null);

// In template:
// <form [formGroup]="form" (ngSubmit)="onSubmit()">
//   <div class="space-y-2">
//     <label for="email" class="text-sm font-medium leading-none">Email</label>
//     <fb-input id="email" type="email" [control]="form.controls.email" [error]="emailError()" />
//   </div>
//   <fb-button type="submit" class="w-full">Sign In</fb-button>
// </form>
```

### Pattern 4: Responsive Grid System

```typescript
// shared/grid/grid.component.ts
import { Component, ChangeDetectionStrategy, computed, input } from '@angular/core';
import { cva } from 'class-variance-authority';
import { cn } from '@/lib/utils';

const gridVariants = cva('grid', {
  variants: {
    cols: {
      1: 'grid-cols-1',
      2: 'grid-cols-1 sm:grid-cols-2',
      3: 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-3',
      4: 'grid-cols-1 sm:grid-cols-2 lg:grid-cols-4',
      5: 'grid-cols-2 sm:grid-cols-3 lg:grid-cols-5',
      6: 'grid-cols-2 sm:grid-cols-3 lg:grid-cols-6',
    },
    gap: {
      none: 'gap-0',
      sm: 'gap-2',
      md: 'gap-4',
      lg: 'gap-6',
      xl: 'gap-8',
    },
  },
  defaultVariants: { cols: 3, gap: 'md' },
});

@Component({
  selector: 'fb-grid',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[class]': 'classes()' },
  template: `<ng-content />`,
})
export class GridComponent {
  cols = input<1 | 2 | 3 | 4 | 5 | 6>(3);
  gap = input<'none' | 'sm' | 'md' | 'lg' | 'xl'>('md');

  classes = computed(() => cn(gridVariants({ cols: this.cols(), gap: this.gap() })));
}

@Component({
  selector: 'fb-container',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[class]': 'classes()' },
  template: `<ng-content />`,
})
export class ContainerComponent {
  size = input<'sm' | 'md' | 'lg' | 'xl' | '2xl' | 'full'>('xl');

  classes = computed(() => cn(
    'mx-auto w-full px-4 sm:px-6 lg:px-8',
    {
      sm: 'max-w-screen-sm',
      md: 'max-w-screen-md',
      lg: 'max-w-screen-lg',
      xl: 'max-w-screen-xl',
      '2xl': 'max-w-screen-2xl',
      full: 'max-w-full',
    }[this.size()]
  ));
}

// Usage
// <fb-container>
//   <fb-grid [cols]="4" gap="lg">
//     @for (item of items(); track item.id) {
//       <app-item-card [item]="item" />
//     }
//   </fb-grid>
// </fb-container>
```

For advanced animation and dark mode patterns, see [references/advanced-patterns.md](references/advanced-patterns.md):

- **Pattern 5: Native CSS Animations** — dialog `@keyframes`, `@starting-style`, `allow-discrete` transitions with Angular CDK Dialog
- **Pattern 6: Dark Mode** — Angular `ThemeService` with `signal()` + `localStorage` persistence and `prefers-color-scheme` detection

## Utility Functions

```typescript
// lib/utils.ts — framework-agnostic, works as-is in Angular
import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export const focusRing = cn(
  'focus-visible:outline-none focus-visible:ring-2',
  'focus-visible:ring-ring focus-visible:ring-offset-2',
);

export const disabled = 'disabled:pointer-events-none disabled:opacity-50';
```

Install dependencies: `npm install clsx tailwind-merge class-variance-authority`
