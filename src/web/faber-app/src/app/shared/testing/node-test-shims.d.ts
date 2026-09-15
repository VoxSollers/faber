/**
 * Minimal ambient declarations for the Node APIs used by test-only utilities
 * (e.g. reading `styles.css` from disk in the contrast spec). The project has no
 * `@types/node` dependency and the unit-test runner executes in Node, so this
 * shim covers just the surface we touch — no full `@types/node` install needed.
 */
declare module 'node:fs' {
  export function readFileSync(path: string, encoding: 'utf8'): string;
}

declare const process: { cwd(): string };
