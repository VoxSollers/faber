import { DestroyRef, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormGroup } from '@angular/forms';

/**
 * The two fields a collapsed accordion title is composed from, e.g.
 * `[jobTitle, employer]` for Experience. Order is significant — it is the
 * order the parts are joined in.
 */
export type TitleParts = readonly [string | null | undefined, string | null | undefined];

/** Formats the two live form values into an accordion title. */
export type TitleFormatter = (parts: TitleParts) => string;

/** Uses the first populated value, e.g. a Link label before its URI. */
export const firstNonEmptyTitlePart: TitleFormatter = parts =>
  parts.map(part => part?.trim() ?? '').find(part => part.length > 0) ?? '';

/**
 * Derives collapsed accordion titles from the *live* value of a nested-list
 * entry's form, so the title updates on every keystroke while the actual
 * save to `ResumesStore` stays behind its 2s `debounceTime`. `ResumesStore`
 * only ever reflects what the server confirmed — this class is local display
 * state layered on top of it, never written back into the store.
 *
 * Reused verbatim by Experience, Education and Course: each registers its
 * own two title-field control names and calls `resolve` from the `titleFn`
 * passed to `app-orderable-list`.
 */
export class EntryTitles {
  private readonly liveParts = signal<ReadonlyMap<string, TitleParts>>(new Map());
  private readonly focusedIds = signal<ReadonlySet<string>>(new Set());
  private readonly formatters = new Map<string, TitleFormatter>();

  // Plain (non-signal) map, and that is the whole point: `resolve` runs from a
  // template expression during change detection ([label]="titleFn()(item)")
  // and writes here, which a signal could not survive — writing a signal
  // mid-CD trips Angular's "signal write during render" guard. Reading
  // `liveParts`/`focusedIds` in `resolve` is fine and necessary — that's how
  // OrderableList's OnPush view picks up the dependency.
  //
  // The resolver's write is not redundant with `setLiveParts`: a title can
  // also come from the store-model fallback (an id with no live form value),
  // and that title still has to be remembered for the focus latch.
  private readonly lastNonEmpty = new Map<string, string>();

  constructor(private readonly destroyRef: DestroyRef) {}

  /**
   * Registers `form` as the live source of truth for `id`'s title, projecting
   * `fieldNames` on every (undebounced) value change. `getForm` (and so this
   * method) runs from inside the template ([formGroup]="getForm(item)"), so
   * this only ever seeds the plain `lastNonEmpty` map synchronously — writing
   * `liveParts` (a signal) here would hit Angular's "signal write during
   * render" guard. Until the first real value change, `resolve` reads the
   * store-model fallback instead, which is exactly what the form was built
   * from, so there's nothing to seed for `liveParts` itself.
   */
  register(
    id: string,
    form: FormGroup,
    fieldNames: readonly [string, string],
    formatter: TitleFormatter = joinTitleParts,
  ): void {
    const project = (): TitleParts => [form.get(fieldNames[0])?.value, form.get(fieldNames[1])?.value];
    this.formatters.set(id, formatter);

    const initialTitle = formatter(project());
    if (initialTitle) {
      this.lastNonEmpty.set(id, initialTitle);
    }

    form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.setLiveParts(id, project());
    });
  }

  private setLiveParts(id: string, parts: TitleParts): void {
    const title = (this.formatters.get(id) ?? joinTitleParts)(parts);
    if (title) {
      this.lastNonEmpty.set(id, title);
    }
    this.liveParts.update(map => {
      const next = new Map(map);
      next.set(id, parts);
      return next;
    });
  }

  /**
   * Resolves the displayed title for `id`. Uses the live form value if `id`
   * is registered, else falls back to `fallback` (the store model) — items
   * whose form hasn't been built yet (accordion not opened) have no live
   * value. An empty result only becomes "Untitled" once `id` isn't focused;
   * while focused it holds the last non-empty title to avoid a mid-edit
   * flicker to "Untitled" and back.
   */
  resolve(
    id: string,
    fallback: TitleParts,
    formatter: TitleFormatter = this.formatters.get(id) ?? joinTitleParts,
  ): string {
    const parts = this.liveParts().get(id) ?? fallback;
    const title = formatter(parts);
    if (title) {
      this.lastNonEmpty.set(id, title);
      return title;
    }
    if (this.focusedIds().has(id)) {
      return this.lastNonEmpty.get(id) ?? 'Untitled';
    }
    return 'Untitled';
  }

  onFocusIn(id: string): void {
    this.focusedIds.update(ids => {
      const next = new Set(ids);
      next.add(id);
      return next;
    });
  }

  /**
   * Tabbing between the two title fields (e.g. Job title → Employer) fires
   * focusout on the first then focusin on the second, with a change-detection
   * pass possibly landing in between — clearing focus here would flash
   * "Untitled" for that pass. `relatedTarget` is the element gaining focus;
   * if it's still inside the same wrapper this wasn't a real blur, so ignore it.
   */
  onFocusOut(id: string, event: FocusEvent): void {
    const wrapper = event.currentTarget as HTMLElement | null;
    const related = event.relatedTarget as Node | null;
    if (wrapper && related && wrapper.contains(related)) {
      return;
    }
    this.focusedIds.update(ids => {
      const next = new Set(ids);
      next.delete(id);
      return next;
    });

    // The latch only ever means "the title to hold for the edit in progress",
    // so it dies with the edit. Keeping it would resurrect a deleted title:
    // clear both fields, click away (settles to "Untitled"), then click back
    // in to type something new — `resolve`'s focused branch would hand back
    // the old text over two empty fields. If the entry still has a title,
    // the next `resolve` re-records it immediately; if it doesn't, staying
    // absent is exactly what makes the re-focus read "Untitled".
    this.lastNonEmpty.delete(id);
  }
}

const joinTitleParts: TitleFormatter = parts =>
  parts
    .map(part => part?.trim() ?? '')
    .filter(part => part.length > 0)
    .join(' at ');
