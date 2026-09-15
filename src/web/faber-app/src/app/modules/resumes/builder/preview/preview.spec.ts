import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { LUCIDE_ICONS, LucideIconProvider, ChevronLeft, ChevronRight } from 'lucide-angular';
import { Preview } from './preview';
import { PDFJS_LOADER } from './pdfjs-loader';
import { PreviewStatus, ResumesStore } from '../../resumes-store';
import { mockResumesStore } from '../../../../shared/testing/mock-resumes-store';
import { expectNoAxeViolations } from '../../../../shared/testing/axe';

class RenderingCancelledException extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'RenderingCancelledException';
  }
}

// Models the real pdfjs RenderTask contract closely enough to exercise
// cancellation. The injected module exposes this same exception class, so
// production's `instanceof` check remains meaningful without a module mock.
function createRenderTask(inner: Promise<void>) {
  let settled = false;
  let rejectFromCancel: ((reason: unknown) => void) | undefined;
  const promise = new Promise<void>((resolve, reject) => {
    rejectFromCancel = reason => {
      if (settled) return;
      settled = true;
      reject(reason);
    };
    inner.then(
      value => {
        if (!settled) {
          settled = true;
          resolve(value);
        }
      },
      reason => {
        if (!settled) {
          settled = true;
          reject(reason);
        }
      },
    );
  });
  return {
    promise,
    cancel: vi.fn(() => {
      rejectFromCancel?.(new RenderingCancelledException('Rendering cancelled'));
    }),
  };
}

function createMockDocument() {
  let destroyed = false;
  return {
    numPages: 3,
    getPage: vi.fn(() =>
      Promise.resolve({
        getViewport: vi.fn(() => ({ width: 595, height: 842 })),
        render: vi.fn(() => {
          if (destroyed) {
            throw new Error('Cannot render: the PDF document has been destroyed.');
          }
          return createRenderTask(Promise.resolve());
        }),
      }),
    ),
    loadingTask: {
      destroy: vi.fn(() => {
        destroyed = true;
        return Promise.resolve();
      }),
    },
  };
}

type MockDocument = ReturnType<typeof createMockDocument>;

const getDocumentMock = vi.fn<() => { promise: Promise<MockDocument> }>(
  () => ({ promise: Promise.resolve(createMockDocument()) }),
);
const pdfjsMock = {
  GlobalWorkerOptions: { workerSrc: '' },
  RenderingCancelledException,
  getDocument: getDocumentMock,
};

/** The mocked document from the most recent `getDocument()` call. */
async function lastMockedDoc() {
  const calls = getDocumentMock.mock.results;
  return calls[calls.length - 1].value.promise;
}

class ResizeObserverStub {
  observe(): void {}
  unobserve(): void {}
  disconnect(): void {}
}

describe('Preview', () => {
  let component: Preview;
  let fixture: ComponentFixture<Preview>;

  const preview = signal<Blob | null>(null);
  const previewRevision = signal<number | null>(0);
  const previewGeneration = signal<number | null>(0);
  const previewStatus = signal<PreviewStatus>('unavailable');
  const hasRenderedPreview = signal(true);
  const confirmPreviewRendered = vi.fn();
  const reportPreviewRenderFailure = vi.fn();

  beforeEach(async () => {
    vi.clearAllMocks();
    getDocumentMock.mockReset();
    getDocumentMock.mockImplementation(() => ({ promise: Promise.resolve(createMockDocument()) }));
    vi.stubGlobal('ResizeObserver', ResizeObserverStub);
    preview.set(null);
    previewRevision.set(0);
    previewGeneration.set(0);
    previewStatus.set('unavailable');
    hasRenderedPreview.set(true);

    await TestBed.configureTestingModule({
      imports: [Preview],
      providers: [
        {
          provide: ResumesStore,
          useValue: {
            ...mockResumesStore(),
            preview,
            previewRevision,
            previewGeneration,
            previewStatus,
            hasRenderedPreview,
            confirmPreviewRendered,
            reportPreviewRenderFailure,
          },
        },
        { provide: PDFJS_LOADER, useValue: () => Promise.resolve(pdfjsMock) },
        {
          provide: LUCIDE_ICONS,
          multi: true,
          useValue: new LucideIconProvider({ ChevronLeft, ChevronRight }),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Preview);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('renders page navigation on the shared glass surface', () => {
    preview.set(new Blob(['pdf-bytes'], { type: 'application/pdf' }));
    fixture.detectChanges();

    const nav = fixture.nativeElement.querySelector('[aria-label="Page navigation"]');
    expect(nav.classList.contains('glass-surface')).toBe(true);
  });

  it('uses a visible light-theme hover fill and foreground arrows in dark mode', () => {
    preview.set(new Blob(['pdf-bytes'], { type: 'application/pdf' }));
    fixture.detectChanges();

    const buttons = (fixture.nativeElement as HTMLElement).querySelectorAll<HTMLButtonElement>(
      '[aria-label="Page navigation"] button',
    );
    expect(buttons).toHaveLength(2);

    for (const button of buttons) {
      expect(button.classList.contains('hover:enabled:bg-foreground/10')).toBe(true);
      expect(button.classList.contains('dark:hover:enabled:bg-muted')).toBe(true);
      expect(button.classList.contains('dark:text-foreground')).toBe(true);
      expect(button.classList.contains('disabled:opacity-35')).toBe(true);
    }
  });

  it('has no AXE violations', async () => {
    preview.set(new Blob(['pdf-bytes'], { type: 'application/pdf' }));
    fixture.detectChanges();
    await expectNoAxeViolations(fixture);
  }, 20_000);

  describe('first-load skeleton (#456 §1)', () => {
    it('renders the skeleton and not the "no preview" text while generating with no blob yet', () => {
      previewStatus.set('generating');
      fixture.detectChanges();

      expect(fixture.nativeElement.querySelector('.fb-preview-skeleton')).toBeTruthy();
      expect(fixture.nativeElement.textContent).not.toContain('No preview available');
    });

    it('renders the "no preview" text and not the skeleton when unavailable', () => {
      previewStatus.set('unavailable');
      fixture.detectChanges();

      expect(fixture.nativeElement.querySelector('.fb-preview-skeleton')).toBeFalsy();
      expect(fixture.nativeElement.textContent).toContain('No preview available');
    });

    it('keeps rendering the canvas, not the skeleton, once a blob has arrived (anti-flicker)', () => {
      previewStatus.set('generating');
      preview.set(new Blob(['pdf-bytes'], { type: 'application/pdf' }));
      fixture.detectChanges();

      expect(fixture.nativeElement.querySelector('canvas')).toBeTruthy();
      expect(fixture.nativeElement.querySelector('.fb-preview-skeleton')).toBeFalsy();
    });

    it('fills the visible preview area without constraining the skeleton to A4 (#456)', () => {
      previewStatus.set('generating');
      fixture.detectChanges();

      const skeleton = fixture.nativeElement.querySelector('.fb-preview-skeleton') as HTMLElement;

      expect(skeleton.classList.contains('absolute')).toBe(true);
      expect(skeleton.classList.contains('inset-0')).toBe(true);
      expect(skeleton.classList.contains('w-full')).toBe(true);
      expect(skeleton.classList.contains('h-full')).toBe(true);
      expect(skeleton.className).not.toContain('aspect-');
      expect(fixture.nativeElement.querySelector('.fb-preview-skeleton-stage')).toBeFalsy();
    });

    it('animates individual skeleton lines without a page-wide sweep layer', () => {
      previewStatus.set('generating');
      fixture.detectChanges();

      const skeleton = fixture.nativeElement.querySelector('.fb-preview-skeleton') as HTMLElement;
      const lines = skeleton.querySelectorAll('.fb-preview-skeleton-line');

      expect(lines.length).toBeGreaterThan(1);
      expect(skeleton.querySelector('.fb-preview-skeleton-sweep')).toBeFalsy();
    });

    it('has no AXE violations while the skeleton is showing', async () => {
      previewStatus.set('generating');
      fixture.detectChanges();
      await expectNoAxeViolations(fixture);
    }, 20_000);

    it('keeps the live region in the DOM before it gains its announcement text', () => {
      // A screen reader only announces a live region's content changing while
      // that region is already in the document — a region that instead enters
      // the DOM already containing its text (an `@if`-scoped span, as this used
      // to be) is never observed being added in the first place, so nothing is
      // announced. Capture the exact node reference while empty, then assert
      // that same node — not a freshly inserted one — is what later gains the
      // text.
      fixture.detectChanges();
      const liveRegion = fixture.nativeElement.querySelector(
        '[role="status"][aria-live="polite"]',
      ) as HTMLElement;
      expect(liveRegion).toBeTruthy();
      expect(liveRegion.textContent).toBe('');

      previewStatus.set('generating');
      fixture.detectChanges();

      const liveRegionAfter = fixture.nativeElement.querySelector(
        '[role="status"][aria-live="polite"]',
      ) as HTMLElement;
      expect(liveRegionAfter).toBe(liveRegion);
      expect(liveRegionAfter.textContent).toBe('Generating preview');
    });
  });

  describe('page navigation', () => {
    it('clamps current page within [1, totalPages]', () => {
      component.totalPages.set(3);
      component.currentPage.set(1);

      component.prev();
      expect(component.currentPage()).toBe(1);

      component.next();
      expect(component.currentPage()).toBe(2);

      component.next();
      component.next();
      expect(component.currentPage()).toBe(3);
    });
  });

  describe('standalone pane (#425)', () => {
    // `renderPdf`/`renderCurrentPage` is an async chain (dynamic import, blob
    // read, pdfjs promises) that isn't tracked by Angular's zoneless
    // scheduler, so `fixture.whenStable()` can resolve before `lockCardWidth`
    // ever runs — and jsdom's `clientHeight` is always 0, so if the stub is
    // queried too late the height guard silently no-ops. Stubbing on the
    // prototype *before* the render starts means every element — including
    // the `#paper` div, which doesn't exist until `store.preview()` flips the
    // `@else if` — reports a realistic height from the moment it's created.
    let originalClientHeight: PropertyDescriptor | undefined;

    beforeEach(() => {
      originalClientHeight = Object.getOwnPropertyDescriptor(HTMLElement.prototype, 'clientHeight');
      Object.defineProperty(HTMLElement.prototype, 'clientHeight', {
        configurable: true,
        get: () => 600,
      });
    });

    afterEach(() => {
      if (originalClientHeight) {
        Object.defineProperty(HTMLElement.prototype, 'clientHeight', originalClientHeight);
      } else {
        Reflect.deleteProperty(HTMLElement.prototype, 'clientHeight');
      }
    });

    it('defaults to locking the card width', () => {
      expect(fixture.componentInstance.lockWidth()).toBe(true);
    });

    it('pins a min-width to fit the card height when locked (default)', async () => {
      const host = fixture.nativeElement as HTMLElement;

      preview.set(new Blob(['pdf-bytes'], { type: 'application/pdf' }));
      fixture.detectChanges();

      // 600px at the pdfjs mock's A4 aspect (595 / 842) is the exact figure
      // the issue cites as too wide for a 375px phone viewport:
      // Math.round(600 * (595 / 842)) = 424. Poll instead of racing a single
      // `whenStable()` — the render chain settles on its own timeline.
      await vi.waitFor(() => expect(host.style.minWidth).toBe('424px'));
      expect(fixture.componentInstance.scrollable()).toBe(false);
    });

    it('never pins a min-width when it is the only pane', async () => {
      const host = fixture.nativeElement as HTMLElement;

      preview.set(new Blob(['pdf-bytes'], { type: 'application/pdf' }));
      fixture.detectChanges();

      // Let it lock for real first (lockWidth defaults to true). Both the
      // locked and unlocked outcomes leave `minWidth` at its initial '' when
      // lockCardWidth never runs at all, so asserting '' from a cold start
      // proves nothing — deleting the guard block entirely would still pass.
      // Observing a genuine 424px -> '' transition is what actually depends
      // on the guard.
      await vi.waitFor(() => expect(host.style.minWidth).toBe('424px'));

      fixture.componentRef.setInput('lockWidth', false);
      // A distinct Blob reference is required, not just the same content —
      // the `store.preview()` effect keys off signal identity, and reusing
      // the first blob would never re-trigger lockCardWidth at all.
      preview.set(new Blob(['pdf-bytes-2'], { type: 'application/pdf' }));
      fixture.detectChanges();

      // Same 600px-tall card as above — without the guard this would
      // recompute the same 424px min-width and force horizontal overflow on
      // a 375px screen, which acceptance forbids.
      await vi.waitFor(() => expect(host.style.minWidth).toBe(''));
      expect(fixture.componentInstance.scrollable()).toBe(true);
    });
  });

  describe('double buffering, page and scroll preservation (#456 §2)', () => {
    // Same reasoning as the "standalone pane" and "ngOnDestroy" blocks above:
    // jsdom reports 0 for both client dimensions, and `renderCurrentPage`
    // bails on `width <= 0` before it ever touches a canvas. Stub both up
    // front so every render in this block actually runs.
    let originalClientHeight: PropertyDescriptor | undefined;
    let originalClientWidth: PropertyDescriptor | undefined;

    beforeEach(() => {
      originalClientHeight = Object.getOwnPropertyDescriptor(HTMLElement.prototype, 'clientHeight');
      originalClientWidth = Object.getOwnPropertyDescriptor(HTMLElement.prototype, 'clientWidth');
      Object.defineProperty(HTMLElement.prototype, 'clientHeight', { configurable: true, get: () => 600 });
      Object.defineProperty(HTMLElement.prototype, 'clientWidth', { configurable: true, get: () => 800 });
      // jsdom does not implement `Element.scrollTo` — only `Window.scrollTo` —
      // and `prev()`/`next()` call it on `#paper`, which is a real element in
      // these tests (unlike the plain "page navigation" spec above, which
      // never renders a preview at all).
      if (typeof HTMLElement.prototype.scrollTo !== 'function') {
        (HTMLElement.prototype as unknown as { scrollTo: () => void }).scrollTo = () => {};
      }
    });

    afterEach(() => {
      if (originalClientHeight) {
        Object.defineProperty(HTMLElement.prototype, 'clientHeight', originalClientHeight);
      } else {
        Reflect.deleteProperty(HTMLElement.prototype, 'clientHeight');
      }
      if (originalClientWidth) {
        Object.defineProperty(HTMLElement.prototype, 'clientWidth', originalClientWidth);
      } else {
        Reflect.deleteProperty(HTMLElement.prototype, 'clientWidth');
      }
    });

    it('renders into the back buffer without touching the front buffer mid-render', async () => {
      preview.set(new Blob(['pdf-bytes-1'], { type: 'application/pdf' }));
      fixture.detectChanges();
      await vi.waitFor(() => expect(component.totalPages()).toBe(3));

      const canvases = () =>
        Array.from(fixture.nativeElement.querySelectorAll('canvas')) as HTMLCanvasElement[];
      const frontBefore = canvases().find(c => c.classList.contains('fb-preview-buffer-front'))!;
      const backBefore = canvases().find(c => !c.classList.contains('fb-preview-buffer-front'))!;

      const widthSetSpy = vi.spyOn(frontBefore, 'width', 'set');
      const heightSetSpy = vi.spyOn(frontBefore, 'height', 'set');

      const doc = await lastMockedDoc();
      let resolveRender!: () => void;
      doc.getPage.mockImplementationOnce(() =>
        Promise.resolve({
          getViewport: vi.fn(() => ({ width: 595, height: 842 })),
          render: vi.fn(() =>
            createRenderTask(
              new Promise<void>(resolve => {
                resolveRender = resolve;
              }),
            ),
          ),
        }),
      );

      component.next();

      // The back buffer receives its new pixel dimensions synchronously,
      // before `page.render()`'s promise ever settles.
      await vi.waitFor(() => expect(backBefore.width).toBe(595));
      expect(backBefore.height).toBe(842);

      // The on-screen (front) canvas must not have been resized or cleared
      // while the render is still in flight.
      expect(widthSetSpy).not.toHaveBeenCalled();
      expect(heightSetSpy).not.toHaveBeenCalled();
      expect(frontBefore.classList.contains('fb-preview-buffer-front')).toBe(true);

      resolveRender();
      await vi.waitFor(() =>
        expect(backBefore.classList.contains('fb-preview-buffer-front')).toBe(true),
      );
    });

    it('keeps the first-load skeleton over the canvas until the revision is painted', async () => {
      hasRenderedPreview.set(false);
      previewStatus.set('generating');
      previewRevision.set(7);
      previewGeneration.set(3);
      preview.set(new Blob(['pdf-bytes'], { type: 'application/pdf' }));
      fixture.detectChanges();

      expect(fixture.nativeElement.querySelector('.fb-preview-skeleton')).toBeTruthy();
      await vi.waitFor(() => expect(confirmPreviewRendered).toHaveBeenCalledWith(7, 3));

      hasRenderedPreview.set(true);
      fixture.detectChanges();
      expect(fixture.nativeElement.querySelector('.fb-preview-skeleton')).toBeFalsy();
    });

    it('does not confirm a manual refresh when the previous document repaints while it is loading', async () => {
      previewRevision.set(1);
      previewGeneration.set(1);
      preview.set(new Blob(['pdf-bytes-1'], { type: 'application/pdf' }));
      fixture.detectChanges();
      await vi.waitFor(() => expect(confirmPreviewRendered).toHaveBeenCalledWith(1, 1));
      confirmPreviewRendered.mockClear();

      let resolveDocument!: (document: Awaited<ReturnType<typeof lastMockedDoc>>) => void;
      getDocumentMock.mockImplementationOnce(
        () => ({
          promise: new Promise(resolve => {
            resolveDocument = resolve;
          }),
        }),
      );

      // A manual refresh replaces the PDF without changing the persisted
      // resume revision, so document identity must disambiguate the frames.
      previewRevision.set(1);
      previewGeneration.set(2);
      preview.set(new Blob(['pdf-bytes-2'], { type: 'application/pdf' }));
      fixture.detectChanges();
      await vi.waitFor(() => expect(getDocumentMock.mock.calls.length).toBe(2));

      // A resize or page turn can still repaint the old document while PDF.js
      // parses the replacement. Equal revision numbers do not make that old
      // frame proof that the refreshed document is visible.
      await (component as unknown as { renderCurrentPage(): Promise<void> }).renderCurrentPage();
      expect(confirmPreviewRendered).not.toHaveBeenCalled();

      resolveDocument({
        numPages: 3,
        getPage: vi.fn(() =>
          Promise.resolve({
            getViewport: vi.fn(() => ({ width: 595, height: 842 })),
            render: vi.fn(() => createRenderTask(Promise.resolve())),
          }),
        ),
        destroy: vi.fn(() => Promise.resolve()),
      } as unknown as Awaited<ReturnType<typeof lastMockedDoc>>);

      await vi.waitFor(() => expect(confirmPreviewRendered).toHaveBeenCalledWith(1, 2));
    });

    it('keeps the outgoing canvas fully opaque throughout the crossfade (branch review: no concurrent fade-out)', async () => {
      // A crossfade that fades BOTH canvases at once (outgoing 1 -> 0 while
      // incoming 0 -> 1 on the same base class) dips composite alpha to
      // roughly 0.75 mid-swap, letting `--color-card` bleed through what
      // should read as an opaque PDF page — the exact darkening #456 §2
      // exists to remove. Assert on the applied inline styles/classes rather
      // than sampling rendered pixels, which jsdom cannot do.
      preview.set(new Blob(['pdf-bytes-1'], { type: 'application/pdf' }));
      fixture.detectChanges();
      await vi.waitFor(() => expect(component.totalPages()).toBe(3));

      const canvases = () =>
        Array.from(fixture.nativeElement.querySelectorAll('canvas')) as HTMLCanvasElement[];
      const frontBefore = canvases().find(c => c.classList.contains('fb-preview-buffer-front'))!;
      const backBefore = canvases().find(c => !c.classList.contains('fb-preview-buffer-front'))!;

      const doc = await lastMockedDoc();
      let resolveRender!: () => void;
      doc.getPage.mockImplementationOnce(() =>
        Promise.resolve({
          getViewport: vi.fn(() => ({ width: 595, height: 842 })),
          render: vi.fn(() =>
            createRenderTask(
              new Promise<void>(resolve => {
                resolveRender = resolve;
              }),
            ),
          ),
        }),
      );

      component.next();
      await vi.waitFor(() => expect(backBefore.width).toBe(595));

      // Mid-render: the on-screen (front) canvas must never have its own
      // opacity touched — no inline override at all, i.e. still at the CSS
      // resting value of 1. The hidden (back) canvas is prepared for its own
      // future fade-in by being reset to 0.
      expect(frontBefore.style.opacity).toBe('');
      expect(backBefore.style.opacity).toBe('0');

      resolveRender();
      await vi.waitFor(() =>
        expect(backBefore.classList.contains('fb-preview-buffer-front')).toBe(true),
      );

      // After promotion: the newly promoted canvas has its inline override
      // cleared so it fades in via CSS to the resting opacity of 1; the
      // demoted canvas — the one that used to be on screen — was never
      // touched and so was never faced with fading out at all.
      expect(backBefore.style.opacity).toBe('');
      expect(frontBefore.style.opacity).toBe('');
    });

    it('opens the very first document on page 1', async () => {
      preview.set(new Blob(['pdf-bytes'], { type: 'application/pdf' }));
      fixture.detectChanges();

      await vi.waitFor(() => expect(component.totalPages()).toBe(3));
      expect(component.currentPage()).toBe(1);
    });

    it('keeps the current page when a replacement document arrives', async () => {
      preview.set(new Blob(['pdf-bytes-1'], { type: 'application/pdf' }));
      fixture.detectChanges();
      await vi.waitFor(() => expect(component.totalPages()).toBe(3));

      component.next();
      await vi.waitFor(() => expect(component.currentPage()).toBe(2));

      preview.set(new Blob(['pdf-bytes-2'], { type: 'application/pdf' }));
      fixture.detectChanges();

      await vi.waitFor(() => expect(getDocumentMock.mock.calls.length).toBe(2));
      expect(component.currentPage()).toBe(2);
    });

    it('clamps the current page when a replacement document is shorter', async () => {
      preview.set(new Blob(['pdf-bytes-1'], { type: 'application/pdf' }));
      fixture.detectChanges();
      await vi.waitFor(() => expect(component.totalPages()).toBe(3));

      component.next();
      component.next();
      await vi.waitFor(() => expect(component.currentPage()).toBe(3));

      // Two pages, not one: clamping to the new last page (2) must be
      // distinguishable from the old "always reset to page 1" behaviour,
      // which would coincidentally also land on 1 if the replacement had
      // only a single page.
      getDocumentMock.mockImplementationOnce(
        () =>
          ({
            promise: Promise.resolve({
              numPages: 2,
              getPage: vi.fn(() =>
                Promise.resolve({
                  getViewport: vi.fn(() => ({ width: 595, height: 842 })),
                  render: vi.fn(() => createRenderTask(Promise.resolve())),
                }),
              ),
              destroy: vi.fn(() => Promise.resolve()),
            }),
          }) as unknown as { promise: Promise<MockDocument> },
      );

      preview.set(new Blob(['pdf-bytes-2'], { type: 'application/pdf' }));
      fixture.detectChanges();

      await vi.waitFor(() => expect(component.totalPages()).toBe(2));
      expect(component.currentPage()).toBe(2);
    });

    it('restores the scroll offset after a replacement resizes the wrapper', async () => {
      preview.set(new Blob(['pdf-bytes-1'], { type: 'application/pdf' }));
      fixture.detectChanges();
      await vi.waitFor(() => expect(component.totalPages()).toBe(3));

      const paperEl = (
        component as unknown as { paper: () => { nativeElement: HTMLDivElement } | undefined }
      ).paper()!.nativeElement;
      paperEl.scrollTop = 120;
      // jsdom neither lays out content nor clamps `scrollTop`, so it can't
      // reproduce the browser's "shrinking scrollHeight resets scrollTop"
      // failure mode on its own; asserting the setter was actively invoked
      // to restore 120 is what distinguishes the fix (which captures and
      // reapplies the offset around the wrapper resize) from the old
      // implementation, which never touches `scrollTop` at all.
      const scrollTopSetSpy = vi.spyOn(paperEl, 'scrollTop', 'set');

      preview.set(new Blob(['pdf-bytes-2'], { type: 'application/pdf' }));
      fixture.detectChanges();

      await vi.waitFor(() => expect(getDocumentMock.mock.calls.length).toBe(2));
      expect(scrollTopSetSpy).toHaveBeenCalledWith(120);
      expect(paperEl.scrollTop).toBe(120);
    });

    it('shrinks the demoted canvas geometry after a resize to a smaller frame, so it cannot contribute a stale bigger frame (branch review Finding 1)', async () => {
      // `a0f2932b` fixed the crossfade darkening by leaving the outgoing canvas at
      // resting opacity 1 for the whole transition on the premise that it is always
      // "already completely covered by the (same-sized, fully opaque) incoming
      // canvas" — true for a page turn or regeneration, but not for a resize down to
      // a SMALLER frame: without a fix, the demoted canvas keeps its old, bigger CSS
      // width/height, so it sticks out past the new, smaller `#frame` on both axes.
      // Stub `clientWidth` as a mutable value (unlike the block-level static 600/800
      // stub) so it can actually shrink between two renders, the way a real window
      // resize would.
      let widthValue = 800;
      Object.defineProperty(HTMLElement.prototype, 'clientWidth', {
        configurable: true,
        get: () => widthValue,
      });

      preview.set(new Blob(['pdf-bytes-1'], { type: 'application/pdf' }));
      fixture.detectChanges();
      await vi.waitFor(() => expect(component.totalPages()).toBe(3));

      const canvases = () =>
        Array.from(fixture.nativeElement.querySelectorAll('canvas')) as HTMLCanvasElement[];
      const largeFrameCanvas = canvases().find(c => c.classList.contains('fb-preview-buffer-front'))!;
      // natural.width (595) * fit (800 / 595) == 800 — the page fills the 800px card.
      expect(parseFloat(largeFrameCanvas.style.width)).toBeCloseTo(800, 0);

      // Shrink the frame and re-render, exactly as the `ResizeObserver` callback
      // would on a real window resize.
      widthValue = 200;
      const renderCurrentPage = (component as unknown as { renderCurrentPage(): Promise<void> })
        .renderCurrentPage.bind(component);
      await renderCurrentPage();
      fixture.detectChanges();

      // The render always targets the OTHER canvas and promotes it on completion, so
      // `largeFrameCanvas` — on screen a moment ago at 800px — is now the demoted one.
      expect(largeFrameCanvas.classList.contains('fb-preview-buffer-front')).toBe(false);
      // It must have been snapped down to the new, smaller frame size — not left at
      // its stale 800px width, which would stick out past the new 200px `#frame` and
      // inflate `#paper`'s scroll extent with nothing visible ever needing it.
      expect(parseFloat(largeFrameCanvas.style.width)).toBeCloseTo(200, 0);

      const frameEl = (
        component as unknown as { frame: () => { nativeElement: HTMLDivElement } | undefined }
      ).frame()!.nativeElement;
      expect(parseFloat(frameEl.style.width)).toBeCloseTo(200, 0);
    });
  });

  describe('concurrent render safety (#456 fix round — cancel superseded renders)', () => {
    let originalClientHeight: PropertyDescriptor | undefined;
    let originalClientWidth: PropertyDescriptor | undefined;

    beforeEach(() => {
      originalClientHeight = Object.getOwnPropertyDescriptor(HTMLElement.prototype, 'clientHeight');
      originalClientWidth = Object.getOwnPropertyDescriptor(HTMLElement.prototype, 'clientWidth');
      Object.defineProperty(HTMLElement.prototype, 'clientHeight', { configurable: true, get: () => 600 });
      Object.defineProperty(HTMLElement.prototype, 'clientWidth', { configurable: true, get: () => 800 });
    });

    afterEach(() => {
      if (originalClientHeight) {
        Object.defineProperty(HTMLElement.prototype, 'clientHeight', originalClientHeight);
      } else {
        Reflect.deleteProperty(HTMLElement.prototype, 'clientHeight');
      }
      if (originalClientWidth) {
        Object.defineProperty(HTMLElement.prototype, 'clientWidth', originalClientWidth);
      } else {
        Reflect.deleteProperty(HTMLElement.prototype, 'clientWidth');
      }
    });

    it('cancels a superseded render instead of racing it on the shared back canvas, and promotes only the newer frame', async () => {
      preview.set(new Blob(['pdf-bytes-1'], { type: 'application/pdf' }));
      fixture.detectChanges();
      await vi.waitFor(() => expect(component.totalPages()).toBe(3));

      const doc = await lastMockedDoc();

      // Mirrors real pdfjs: calling `render()` again on a canvas that is
      // still mid-render throws synchronously ("Cannot use the same canvas
      // during multiple render() operations") rather than quietly queuing.
      // Two overlapping renders sharing this lock is what actually
      // reproduces the bug from #456 — `backCanvas()` resolves to the same
      // DOM element for both calls until one of them promotes it.
      const busyCanvases = new Set<HTMLCanvasElement>();
      function lockedRenderTask(canvas: HTMLCanvasElement, inner: Promise<void>) {
        if (busyCanvases.has(canvas)) {
          throw new Error('Cannot use the same canvas during multiple render() operations');
        }
        busyCanvases.add(canvas);
        const task = createRenderTask(inner);
        task.promise.finally(() => busyCanvases.delete(canvas)).catch(() => {});
        return task;
      }

      let taskA: ReturnType<typeof createRenderTask> | undefined;
      let resolveB!: () => void;

      doc.getPage
        .mockImplementationOnce(() =>
          Promise.resolve({
            // Deliberately distinct from B's viewport below: the assertion at
            // the end only proves anything if the surviving pixel dimensions
            // could only have come from the later render.
            getViewport: vi.fn(() => ({ width: 111, height: 111 })),
            render: vi.fn(({ canvas }: { canvas: HTMLCanvasElement }) => {
              // Never resolves on its own — this render is only ever meant
              // to settle via cancellation below.
              taskA = lockedRenderTask(canvas, new Promise<void>(() => {}));
              return taskA;
            }),
          }),
        )
        .mockImplementationOnce(() =>
          Promise.resolve({
            getViewport: vi.fn(() => ({ width: 222, height: 222 })),
            render: vi.fn(({ canvas }: { canvas: HTMLCanvasElement }) =>
              lockedRenderTask(
                canvas,
                new Promise<void>(resolve => {
                  resolveB = resolve;
                }),
              ),
            ),
          }),
        );

      const renderCurrentPage = (component as unknown as { renderCurrentPage(): Promise<void> })
        .renderCurrentPage.bind(component);

      const call1 = renderCurrentPage();
      await vi.waitFor(() => expect(taskA).toBeDefined());
      const call2 = renderCurrentPage();
      await vi.waitFor(() => expect(resolveB).toBeDefined());

      resolveB();

      // Neither promise may reject: the superseded render (call1) must have
      // its RenderingCancelledException swallowed, not surfaced as an
      // unhandled rejection, and the superseding render (call2) must
      // complete normally.
      await expect(call2).resolves.toBeUndefined();
      await expect(call1).resolves.toBeUndefined();

      expect(taskA!.cancel).toHaveBeenCalledTimes(1);

      // The buffer flip is reflected via a `[class.fb-preview-buffer-front]`
      // template binding, which only updates the DOM on change detection.
      fixture.detectChanges();
      const frontCanvas = fixture.nativeElement.querySelector(
        'canvas.fb-preview-buffer-front',
      ) as HTMLCanvasElement;
      expect(frontCanvas.width).toBe(222);
    });

    it('surfaces a genuine render failure instead of swallowing it like a cancellation', async () => {
      preview.set(new Blob(['pdf-bytes-1'], { type: 'application/pdf' }));
      fixture.detectChanges();
      await vi.waitFor(() => expect(component.totalPages()).toBe(3));

      const doc = await lastMockedDoc();
      const busyCanvases = new Set<HTMLCanvasElement>();
      function lockedRenderTask(canvas: HTMLCanvasElement, inner: Promise<void>) {
        if (busyCanvases.has(canvas)) {
          throw new Error('Cannot use the same canvas during multiple render() operations');
        }
        busyCanvases.add(canvas);
        const task = createRenderTask(inner);
        task.promise.finally(() => busyCanvases.delete(canvas)).catch(() => {});
        return task;
      }

      let taskA: ReturnType<typeof createRenderTask> | undefined;
      const failure = new Error('pdfjs internal render error');

      doc.getPage
        .mockImplementationOnce(() =>
          Promise.resolve({
            getViewport: vi.fn(() => ({ width: 111, height: 111 })),
            render: vi.fn(({ canvas }: { canvas: HTMLCanvasElement }) => {
              taskA = lockedRenderTask(canvas, new Promise<void>(() => {}));
              return taskA;
            }),
          }),
        )
        .mockImplementationOnce(() =>
          Promise.resolve({
            getViewport: vi.fn(() => ({ width: 222, height: 222 })),
            render: vi.fn(({ canvas }: { canvas: HTMLCanvasElement }) =>
              lockedRenderTask(canvas, Promise.reject(failure)),
            ),
          }),
        );

      const renderCurrentPage = (component as unknown as { renderCurrentPage(): Promise<void> })
        .renderCurrentPage.bind(component);

      const call1 = renderCurrentPage();
      await vi.waitFor(() => expect(taskA).toBeDefined());
      const call2 = renderCurrentPage();

      // call1's cancellation is still swallowed — only call2's genuine
      // failure must surface.
      await expect(call1).resolves.toBeUndefined();
      await expect(call2).rejects.toBe(failure);
      expect(taskA!.cancel).toHaveBeenCalledTimes(1);
    });

    it('logs a genuine render failure from a fire-and-forget call site instead of leaving it unhandled (#456 fix round §3)', async () => {
      // `next()`, `prev()`, the resize observer, and `renderPdf` all call
      // `renderCurrentPage` without awaiting or returning its promise —
      // "surfaces a genuine render failure" above proves `renderCurrentPage`
      // itself still rejects correctly, but nothing at any of those call
      // sites would ever observe that rejection on its own. This asserts the
      // wrapper those call sites actually use (`renderCurrentPageSafely`)
      // catches it and logs it, rather than letting it reach nobody as an
      // unhandled rejection while the stale frame stays frozen on screen.
      preview.set(new Blob(['pdf-bytes-1'], { type: 'application/pdf' }));
      fixture.detectChanges();
      await vi.waitFor(() => expect(component.totalPages()).toBe(3));

      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      const doc = await lastMockedDoc();
      const failure = new Error('pdfjs internal render error');
      doc.getPage.mockImplementationOnce(() =>
        Promise.resolve({
          getViewport: vi.fn(() => ({ width: 595, height: 842 })),
          render: vi.fn(() => createRenderTask(Promise.reject(failure))),
        }),
      );

      component.next();

      await vi.waitFor(() =>
        expect(consoleErrorSpy).toHaveBeenCalledWith('Failed to render preview page', failure),
      );

      consoleErrorSpy.mockRestore();
    });

    it('logs a renderPdf failure instead of leaving it an unhandled rejection (branch review Finding 3)', async () => {
      // `8950933b` only wrapped `renderCurrentPage`'s fire-and-forget call
      // sites. The constructor effect's call to `renderPdf` (which covers
      // `loadPdfjs`, the blob read and `getDocument()` — none of which
      // `renderCurrentPageSafely`/`renderCurrentPage` ever touch) was missed:
      // a corrupt PDF rejecting `getDocument(...).promise` reached nobody. If
      // this rejection were still unhandled, this `await vi.waitFor` would
      // never observe the log and the test would time out rather than fail
      // fast — pin the exact wrapper message so a regression is unambiguous.
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
      const failure = new Error('Invalid PDF structure');
      getDocumentMock.mockImplementationOnce(
        () => ({ promise: Promise.reject(failure) }),
      );

      previewRevision.set(9);
      previewGeneration.set(4);
      preview.set(new Blob(['corrupt-pdf-bytes'], { type: 'application/pdf' }));
      fixture.detectChanges();

      await vi.waitFor(() =>
        expect(consoleErrorSpy).toHaveBeenCalledWith('Failed to load preview PDF', failure),
      );
      expect(reportPreviewRenderFailure).toHaveBeenCalledWith(9, 4, failure);

      consoleErrorSpy.mockRestore();
    });

    it('does not flip the buffer or resize the frame when torn down between render() settling and the swap (#456 fix round §2)', async () => {
      preview.set(new Blob(['pdf-bytes-1'], { type: 'application/pdf' }));
      fixture.detectChanges();
      await vi.waitFor(() => expect(component.totalPages()).toBe(3));

      const frontBefore = fixture.nativeElement.querySelector(
        'canvas.fb-preview-buffer-front',
      ) as HTMLCanvasElement;
      const frameEl = (
        component as unknown as { frame: () => { nativeElement: HTMLDivElement } | undefined }
      ).frame()!.nativeElement;
      const widthBefore = frameEl.style.width;
      const heightBefore = frameEl.style.height;

      const doc = await lastMockedDoc();
      let resolveRender!: () => void;
      doc.getPage.mockImplementationOnce(() =>
        Promise.resolve({
          getViewport: vi.fn(() => ({ width: 595, height: 842 })),
          render: vi.fn(() =>
            createRenderTask(
              new Promise<void>(resolve => {
                resolveRender = resolve;
              }),
            ),
          ),
        }),
      );

      const pending = (component as unknown as { renderCurrentPage(): Promise<void> })
        .renderCurrentPage();
      await vi.waitFor(() => expect(resolveRender).toBeDefined());

      // Resolve `page.render(...).promise` and destroy the component in the
      // same synchronous tick — `renderCurrentPage`'s continuation after that
      // `await` is queued as a microtask, so `ngOnDestroy` (also synchronous)
      // runs first and bumps the token before the continuation ever resumes.
      // This is the second `token !== this.renderToken` check's window,
      // which neither pre-existing `ngOnDestroy` spec reaches — both destroy
      // during the earlier `await this.pdfDoc.getPage(...)` window instead.
      resolveRender();
      expect(() => fixture.destroy()).not.toThrow();

      await expect(pending).resolves.toBeUndefined();

      expect(frameEl.style.width).toBe(widthBefore);
      expect(frameEl.style.height).toBe(heightBefore);
      expect(frontBefore.classList.contains('fb-preview-buffer-front')).toBe(true);
    });
  });

  describe('ngOnDestroy (#425 mobile editor<->preview toggle recreates Preview per tap)', () => {
    // Without a real layout engine, jsdom reports 0 for both dimensions, and
    // `renderCurrentPage` bails on `width <= 0` long before it would ever
    // reach `page.render(...)` — guard or no guard. Stubbing both, as the
    // "standalone pane" block above does for height alone, lets execution
    // actually reach the call the renderToken guard is meant to prevent.
    let originalClientHeight: PropertyDescriptor | undefined;
    let originalClientWidth: PropertyDescriptor | undefined;

    beforeEach(() => {
      originalClientHeight = Object.getOwnPropertyDescriptor(HTMLElement.prototype, 'clientHeight');
      originalClientWidth = Object.getOwnPropertyDescriptor(HTMLElement.prototype, 'clientWidth');
      Object.defineProperty(HTMLElement.prototype, 'clientHeight', { configurable: true, get: () => 600 });
      Object.defineProperty(HTMLElement.prototype, 'clientWidth', { configurable: true, get: () => 800 });
    });

    afterEach(() => {
      if (originalClientHeight) {
        Object.defineProperty(HTMLElement.prototype, 'clientHeight', originalClientHeight);
      } else {
        Reflect.deleteProperty(HTMLElement.prototype, 'clientHeight');
      }
      if (originalClientWidth) {
        Object.defineProperty(HTMLElement.prototype, 'clientWidth', originalClientWidth);
      } else {
        Reflect.deleteProperty(HTMLElement.prototype, 'clientWidth');
      }
    });

    it('destroys the pdf loading task and clears the reference', async () => {
      preview.set(new Blob(['pdf-bytes'], { type: 'application/pdf' }));
      fixture.detectChanges();

      // Wait for renderPdf's async chain (dynamic import, blob read, pdfjs
      // promises) to actually assign a pdfDoc before destroying it.
      await vi.waitFor(() => expect(component.totalPages()).toBe(3));
      const doc = await lastMockedDoc();

      fixture.destroy();

      expect(doc.loadingTask.destroy).toHaveBeenCalledTimes(1);
      expect((component as unknown as { pdfDoc: unknown }).pdfDoc).toBeNull();
    });

    it('does not throw or touch the destroyed document when torn down mid-render', async () => {
      preview.set(new Blob(['pdf-bytes'], { type: 'application/pdf' }));
      fixture.detectChanges();
      await vi.waitFor(() => expect(component.totalPages()).toBe(3));
      const doc = await lastMockedDoc();

      // Kick off a second render (e.g. a page turn) and destroy the component
      // before its `await this.pdfDoc.getPage(...)` resumes — the same shape
      // as the mobile toggle tearing this component down mid-render. The
      // renderToken guard must make the resumed render bail out instead of
      // running `page.render(...)` against a document that was just destroyed.
      const pending = (component as unknown as { renderCurrentPage(): Promise<void> })
        .renderCurrentPage();

      expect(() => fixture.destroy()).not.toThrow();

      await expect(pending).resolves.toBeUndefined();
      expect(doc.loadingTask.destroy).toHaveBeenCalledTimes(1);
    });
  });
});
