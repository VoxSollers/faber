import { Component, DestroyRef, inject } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { FormBuilder } from '@angular/forms';
import { EntryTitles, firstNonEmptyTitlePart } from './entry-titles';

// EntryTitles needs a DestroyRef to unsubscribe its valueChanges subscriptions
// via takeUntilDestroyed. A plain class can't `inject()` on its own, so tests
// build it the same way the real components do: inside a component's
// injection context, exposed for assertions.
@Component({ template: '', standalone: true })
class HostComponent {
  readonly destroyRef = inject(DestroyRef);
  readonly fb = inject(FormBuilder);
  readonly titles = new EntryTitles(this.destroyRef);
}

/**
 * A focusout that really leaves the title-field pair: `relatedTarget` is the
 * element gaining focus, and null means it landed outside the wrapper.
 */
function blurEvent(): FocusEvent {
  return {
    currentTarget: document.createElement('div'),
    relatedTarget: null,
  } as unknown as FocusEvent;
}

describe('EntryTitles', () => {
  let host: HostComponent;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [HostComponent] }).compileComponents();
    host = TestBed.createComponent(HostComponent).componentInstance;
  });

  describe('join rule (via the store-model fallback)', () => {
    it('joins both non-empty parts with " at "', () => {
      expect(host.titles.resolve('x', ['Developer', 'Acme'])).toBe('Developer at Acme');
    });

    it('drops an empty second part', () => {
      expect(host.titles.resolve('x', ['Developer', ''])).toBe('Developer');
    });

    it('drops an empty first part', () => {
      expect(host.titles.resolve('x', ['', 'Acme'])).toBe('Acme');
    });

    it('falls back to "Untitled" when both parts are empty', () => {
      expect(host.titles.resolve('x', ['', ''])).toBe('Untitled');
    });

    it('treats whitespace-only parts as empty', () => {
      expect(host.titles.resolve('x', ['  ', 'Acme'])).toBe('Acme');
    });
  });

  it('falls back to the store model for an id that was never registered', () => {
    expect(host.titles.resolve('unregistered', ['Developer', 'Acme'])).toBe('Developer at Acme');
  });

  it('uses the live form value over the store-model fallback once registered', () => {
    const form = host.fb.group({ jobTitle: 'Developer', employer: 'Acme' });
    host.titles.register('e1', form, ['jobTitle', 'employer']);

    form.patchValue({ employer: 'Globex' });

    expect(host.titles.resolve('e1', ['Developer', 'Acme'])).toBe('Developer at Globex');
  });

  it('supports a first-non-empty formatter for live and fallback values', () => {
    const form = host.fb.group({ label: 'LinkedIn', uri: 'https://linkedin.com/in/me' });
    host.titles.register('l1', form, ['label', 'uri'], firstNonEmptyTitlePart);

    expect(host.titles.resolve('l1', ['LinkedIn', 'https://linkedin.com/in/me'], firstNonEmptyTitlePart))
      .toBe('LinkedIn');

    form.patchValue({ label: '' });

    expect(host.titles.resolve('l1', ['LinkedIn', 'https://linkedin.com/in/me'], firstNonEmptyTitlePart))
      .toBe('https://linkedin.com/in/me');
    expect(host.titles.resolve('unregistered-link', ['', 'https://example.com'], firstNonEmptyTitlePart))
      .toBe('https://example.com');
  });

  describe('focus latch', () => {
    it('holds the last non-empty title while focused instead of falling to "Untitled"', () => {
      const form = host.fb.group({ jobTitle: 'Developer', employer: 'Acme' });
      host.titles.register('e1', form, ['jobTitle', 'employer']);

      host.titles.onFocusIn('e1');
      form.patchValue({ jobTitle: '', employer: '' });

      expect(host.titles.resolve('e1', ['', ''])).toBe('Developer at Acme');
    });

    it('yields "Untitled" once focus leaves for real', () => {
      const form = host.fb.group({ jobTitle: 'Developer', employer: 'Acme' });
      host.titles.register('e1', form, ['jobTitle', 'employer']);

      host.titles.onFocusIn('e1');
      form.patchValue({ jobTitle: '', employer: '' });

      host.titles.onFocusOut('e1', blurEvent());

      expect(host.titles.resolve('e1', ['', ''])).toBe('Untitled');
    });

    it('an id that never had a non-empty title is "Untitled" even while focused', () => {
      host.titles.onFocusIn('never-typed');
      expect(host.titles.resolve('never-typed', ['', ''])).toBe('Untitled');
    });

    it('does not resurrect a deleted title when the emptied entry is focused again', () => {
      const form = host.fb.group({ jobTitle: 'Developer', employer: 'Acme' });
      host.titles.register('e1', form, ['jobTitle', 'employer']);

      // Clear both fields, then leave — the entry settles to "Untitled".
      host.titles.onFocusIn('e1');
      form.patchValue({ jobTitle: '', employer: '' });
      host.titles.onFocusOut('e1', blurEvent());
      expect(host.titles.resolve('e1', ['', ''])).toBe('Untitled');

      // Clicking back in to type something new must not bring "Developer at
      // Acme" back over two empty fields — the latch belongs to the finished
      // edit, not to this one.
      host.titles.onFocusIn('e1');
      expect(host.titles.resolve('e1', ['', ''])).toBe('Untitled');
    });

    it('ignores focusout when relatedTarget stays inside the same wrapper (tabbing between title fields)', () => {
      const form = host.fb.group({ jobTitle: 'Developer', employer: 'Acme' });
      host.titles.register('e1', form, ['jobTitle', 'employer']);

      host.titles.onFocusIn('e1');
      form.patchValue({ jobTitle: '', employer: '' });

      const wrapper = document.createElement('div');
      const relatedField = document.createElement('input');
      wrapper.appendChild(relatedField);
      const event = { currentTarget: wrapper, relatedTarget: relatedField } as unknown as FocusEvent;
      host.titles.onFocusOut('e1', event);

      // Still latched — the wrapper-internal focus move must not have cleared it.
      expect(host.titles.resolve('e1', ['', ''])).toBe('Developer at Acme');
    });
  });
});
