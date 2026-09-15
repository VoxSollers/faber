import { ComponentFixture } from '@angular/core/testing';
import axe from 'axe-core';
import { expect } from 'vitest';

/**
 * Runs axe-core against a component fixture and asserts there are no WCAG A/AA
 * violations. The fixture root is temporarily attached to `document.body` so axe
 * can evaluate it in document context, then detached again.
 *
 * Note: jsdom cannot compute layout or colour, so the `color-contrast` rule is
 * disabled here — contrast is verified manually against the AA design tokens.
 */
export async function expectNoAxeViolations(
  fixture: ComponentFixture<unknown>,
): Promise<void> {
  const root = fixture.nativeElement as HTMLElement;
  document.body.appendChild(root);
  try {
    const results = await axe.run(root, {
      runOnly: { type: 'tag', values: ['wcag2a', 'wcag2aa'] },
      rules: { 'color-contrast': { enabled: false } },
    });
    const summary = results.violations
      .map(v => `${v.id}: ${v.help} (${v.nodes.length} node(s))`)
      .join('\n');
    expect(results.violations, `AXE violations:\n${summary}`).toEqual([]);
  } finally {
    root.remove();
  }
}
