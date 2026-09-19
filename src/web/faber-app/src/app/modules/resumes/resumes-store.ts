import { computed, DestroyRef, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EMPTY, Observable, of, Subject } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { ResumesClient } from './resumes-client';
import { Resume } from './resume-response';
import { Orderly } from './orderly';
import { PersonClient } from './builder/person/person-client';
import { Person } from './builder/person/person-response';
import { UpdatePersonRequest } from './builder/person/update-person-request';
import { EducationsClient } from './builder/education/educations-client';
import { Education } from './builder/education/education-response';
import { UpdateEducationRequest } from './builder/education/update-education-request';
import { ExperiencesClient } from './builder/experience/experiences-client';
import { Experience } from './builder/experience/experience-response';
import { UpdateExperienceRequest } from './builder/experience/update-experience-request';
import { SkillsClient } from './builder/skill/skills-client';
import { UpdateSkillRequest } from './builder/skill/update-skill-request';
import { LanguagesClient } from './builder/language/languages-client';
import { UpdateLanguageRequest } from './builder/language/update-language-request';
import { LinksClient } from './builder/link/links-client';
import { UpdateLinkRequest } from './builder/link/update-link-request';
import { CoursesClient } from './builder/course/courses-client';
import { Course } from './builder/course/course-response';
import { UpdateCourseRequest } from './builder/course/update-course-request';
import { ProjectsClient } from './builder/project/projects-client';
import { Project } from './builder/project/project-response';
import { UpdateProjectRequest } from './builder/project/update-project-request';
import { base64ToBlob } from '../../shared/utils/file-utils';
import { isRateLimited, retryOnRateLimit } from '../../shared/utils/rate-limit';

export type SaveStatus = 'idle' | 'saving' | 'saved' | 'error';
export type SyncErrorKind = 'save' | 'preview' | null;

/**
 * Where the preview pipeline stands: `'generating'` while the current persisted revision is
 * being produced or painted, `'ready'` only after PDF.js promotes that frame, and `'unavailable'`
 * when the last attempt failed or the preview pane was turned off.
 */
export type PreviewStatus = 'generating' | 'ready' | 'unavailable';

/**
 * How a store notice should read to the user: `'warning'` for a throttled request the user can
 * recover from by pausing, `'error'` for a genuine failure. Maps onto `FbToast`'s variant.
 */
export type NoticeVariant = 'error' | 'warning';

/** Direction of a bulk open/close request aimed at every accordion section in the editor. */
export type SectionBulkAction = 'expand' | 'collapse';

/**
 * One bulk open/close request. Carried as an object rather than a bare string so that every
 * call allocates a fresh value: two identical requests in a row (expand → the user closes a
 * section by hand → expand again) stay two distinct signal values, and the `effect()` in
 * `SectionList` that applies them re-runs for both.
 */
export interface SectionBulkRequest {
  readonly action: SectionBulkAction;
}

/**
 * Shown when a request was still throttled after `retryOnRateLimit` exhausted its retries —
 * the user is editing faster than the server's rate limit allows, and the fix is to pause,
 * not to report a broken app.
 */
const RATE_LIMIT_MESSAGE = 'Too many requests — slow down for a moment and try again.';

/**
 * One completed server round-trip reduced to a value. `guarded` uses this so a failure travels
 * as data rather than as an error notification — an error surfacing in one of the store's
 * long-lived pipes would unsubscribe it for the rest of the page's life.
 */
type SaveOutcome<T> =
  | { readonly ok: true; readonly value: T }
  | { readonly ok: false; readonly error: unknown };

interface PreviewDocument {
  readonly blob: Blob;
  readonly revision: number;
  readonly generation: number;
}

interface PreviewRequest {
  readonly revision: number;
  readonly session: number;
  readonly generation: number;
}

interface SaveJob {
  readonly key: string;
  readonly revision: number;
  readonly session: number;
  readonly errorMessage: string;
  readonly execute: () => Observable<SaveOutcome<void>>;
}

interface SaveQueue {
  timer: ReturnType<typeof setTimeout> | null;
  pending: SaveJob | null;
  ready: boolean;
  active: boolean;
  readonly immediate: SaveJob[];
}

@Injectable()
export class ResumesStore {
  private static readonly SAVE_DEBOUNCE_MS = 2000;
  private static readonly PERSON_KEY = 'person';

  private readonly destroyRef = inject(DestroyRef);
  private readonly resumesClient = inject(ResumesClient);
  private readonly personClient = inject(PersonClient);
  private readonly educationsClient = inject(EducationsClient);
  private readonly experiencesClient = inject(ExperiencesClient);
  private readonly skillsClient = inject(SkillsClient);
  private readonly languagesClient = inject(LanguagesClient);
  private readonly linksClient = inject(LinksClient);
  private readonly coursesClient = inject(CoursesClient);
  private readonly projectsClient = inject(ProjectsClient);

  private readonly state = signal<Resume | null>(null);
  private readonly previewDocumentSignal = signal<PreviewDocument | null>(null);
  private readonly loadingSignal = signal(false);
  private readonly errorSignal = signal<string | null>(null);
  private readonly noticeVariantSignal = signal<NoticeVariant>('error');
  private readonly previewEnabledSignal = signal(false);
  private readonly previewStatusSignal = signal<PreviewStatus>('unavailable');
  private readonly previewFailureRevisionSignal = signal<number | null>(null);
  private readonly currentRevisionSignal = signal(0);
  private readonly persistedRevisionSignal = signal(-1);
  private readonly renderedPreviewRevisionSignal = signal(-1);
  private readonly dirtySaveKeys = signal<ReadonlyMap<string, number>>(new Map());
  // Requests for one key are serialized by `SaveQueue`, so Set membership is the complete
  // in-flight state. Different keys may still save concurrently.
  private readonly inFlightSaveKeys = signal<ReadonlySet<string>>(new Set());
  // Sticky per key: a failure is only cleared by a later *success of that same key*, never by
  // an unrelated key succeeding (§3.2/§3.3 — see `saveStatus` below).
  private readonly failedSaveKeys = signal<ReadonlySet<string>>(new Set());
  private readonly requestedSectionSignal = signal<string | null>(null);
  private readonly requestedAllSectionsSignal = signal<SectionBulkRequest | null>(null);
  private readonly downloadingSignal = signal(false);
  private readonly saveQueues = new Map<string, SaveQueue>();
  private latestPersonDraft: UpdatePersonRequest | null = null;
  private session = 0;
  private previewGenerationCounter = 0;

  readonly resume = this.state.asReadonly();
  readonly isLoading = this.loadingSignal.asReadonly();
  readonly preview = computed(() => this.previewDocumentSignal()?.blob ?? null);
  readonly previewRevision = computed(() => this.previewDocumentSignal()?.revision ?? null);
  readonly previewGeneration = computed(() => this.previewDocumentSignal()?.generation ?? null);
  readonly previewStatus = this.previewStatusSignal.asReadonly();
  readonly error = this.errorSignal.asReadonly();
  readonly noticeVariant = this.noticeVariantSignal.asReadonly();
  // Derived, not written directly, so there is exactly one place that decides the status —
  // see the priority order in the comment above the computation below.
  readonly saveStatus = computed<SaveStatus>(() => {
    if (this.failedSaveKeys().size > 0) return 'error';
    if (
      this.previewEnabledSignal()
      && this.previewFailureRevisionSignal() === this.currentRevisionSignal()
    ) return 'error';
    if (!this.state()) return 'idle';
    if (this.dirtySaveKeys().size > 0 || this.inFlightSaveKeys().size > 0) return 'saving';
    if (
      this.previewEnabledSignal()
      && (
        this.previewStatusSignal() === 'generating'
        || this.renderedPreviewRevisionSignal() !== this.persistedRevisionSignal()
      )
    ) return 'saving';
    return 'saved';
  });
  readonly syncErrorKind = computed<SyncErrorKind>(() => {
    if (this.failedSaveKeys().size > 0) return 'save';
    if (
      this.previewEnabledSignal()
      && this.previewFailureRevisionSignal() === this.currentRevisionSignal()
    ) return 'preview';
    return null;
  });
  readonly hasRenderedPreview = computed(() => this.renderedPreviewRevisionSignal() >= 0);
  readonly requestedSection = this.requestedSectionSignal.asReadonly();
  readonly requestedAllSections = this.requestedAllSectionsSignal.asReadonly();
  /** True while an on-demand PDF request is in flight — no preview blob was cached. */
  readonly isDownloading = this.downloadingSignal.asReadonly();
  readonly canDownload = computed(() =>
    Boolean(this.state())
    && this.dirtySaveKeys().size === 0
    && this.inFlightSaveKeys().size === 0
    && this.failedSaveKeys().size === 0,
  );

  readonly person = computed(() => this.state()?.person);
  readonly title = computed(() => this.state()?.title ?? '');
  readonly summary = computed(() => this.state()?.summary ?? '');
  readonly hobbies = computed(() => this.state()?.hobbies ?? '');
  readonly localization = computed(() => this.state()?.localization ?? '');
  readonly educations = computed(() => this.state()?.educations ?? []);
  readonly experiences = computed(() => this.state()?.experience ?? []);
  readonly skills = computed(() => this.state()?.skills ?? []);
  readonly languages = computed(() => this.state()?.languages ?? []);
  readonly links = computed(() => this.state()?.links ?? []);
  readonly courses = computed(() => this.state()?.courses ?? []);
  readonly projects = computed(() => this.state()?.projects ?? []);

  private readonly previewRefresh$ = new Subject<PreviewRequest | null>();

  constructor() {
    this.previewRefresh$
      .pipe(
        switchMap(request => {
          if (!request) return EMPTY;
          const resume = this.state();
          if (!resume || !this.previewEnabledSignal() || request.session !== this.session) return EMPTY;
          return this.guarded(this.resumesClient.generate(resume.id)).pipe(
            map(outcome => ({ request, outcome })),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(({ request, outcome }) => {
        if (!this.isCurrentPreviewRequest(request)) return;
        if (!outcome.ok) {
          this.reportFailure(outcome.error, 'Failed to generate preview.');
          this.previewStatusSignal.set('unavailable');
          this.previewFailureRevisionSignal.set(request.revision);
          return;
        }
        this.previewDocumentSignal.set({
          blob: base64ToBlob(outcome.value),
          revision: request.revision,
          generation: request.generation,
        });
      });

    this.destroyRef.onDestroy(() => this.clearSaveQueues());
  }

  loadResume(id: string): void {
    this.resetSynchronizationState();
    const session = this.session;
    this.loadingSignal.set(true);
    this.errorSignal.set(null);
    this.noticeVariantSignal.set('error');
    this.resumesClient.getById(id).pipe(retryOnRateLimit()).subscribe({
      next: resume => {
        if (session !== this.session) return;
        this.state.set(resume);
        this.persistedRevisionSignal.set(this.currentRevisionSignal());
        this.loadingSignal.set(false);
        this.requestPreviewRefresh();
      },
      error: (error: unknown) => {
        if (session !== this.session) return;
        this.reportFailure(error, 'Failed to load resume.');
        this.loadingSignal.set(false);
      },
    });
  }

  clearError(): void {
    this.errorSignal.set(null);
    this.noticeVariantSignal.set('error');
  }

  refreshPreview(): void {
    this.requestPreviewRefresh();
  }

  requestSection(id: string): void {
    this.requestedSectionSignal.set(id);
  }

  /** Asks every editor section to open (`'expand'`) or close (`'collapse'`) in one action. */
  requestAllSections(action: SectionBulkAction): void {
    this.requestedAllSectionsSignal.set({ action });
  }

  setPreviewEnabled(value: boolean): void {
    if (this.previewEnabledSignal() === value) return;
    this.previewEnabledSignal.set(value);
    if (value) {
      this.requestPreviewRefresh();
    } else {
      this.previewRefresh$.next(null);
      this.previewDocumentSignal.set(null);
      this.renderedPreviewRevisionSignal.set(-1);
      this.previewFailureRevisionSignal.set(null);
      // Otherwise a preview generated before the pane was disabled would leave the status
      // stranded on 'ready' with no blob to show for it.
      this.previewStatusSignal.set('unavailable');
    }
  }

  // Starts a render only for a fully persisted revision. Dirty and in-flight edits are already
  // represented by saveStatus, so a manual refresh during one of those waits for that batch to
  // settle instead of asking the server to render a knowingly stale snapshot.
  private requestPreviewRefresh(): void {
    if (!this.state() || !this.previewEnabledSignal()) {
      this.previewStatusSignal.set('unavailable');
      return;
    }
    this.previewStatusSignal.set('generating');
    if (this.dirtySaveKeys().size > 0 || this.inFlightSaveKeys().size > 0) return;
    this.startPreviewGeneration(this.persistedRevisionSignal());
  }

  /** Marks an API-produced document as the revision that is actually visible to the user. */
  confirmPreviewRendered(revision: number, generation: number): void {
    if (
      !this.previewEnabledSignal()
      || this.previewDocumentSignal()?.revision !== revision
      || this.previewDocumentSignal()?.generation !== generation
      || generation !== this.previewGenerationCounter
      || revision !== this.currentRevisionSignal()
      || revision !== this.persistedRevisionSignal()
      || this.dirtySaveKeys().size > 0
      || this.inFlightSaveKeys().size > 0
    ) return;

    this.renderedPreviewRevisionSignal.set(revision);
    this.previewStatusSignal.set('ready');
    if (this.previewFailureRevisionSignal() === revision) {
      this.previewFailureRevisionSignal.set(null);
      if (this.failedSaveKeys().size === 0) this.clearError();
    }
  }

  /** Routes a PDF.js parse/render failure into the same synchronization state as generation. */
  reportPreviewRenderFailure(revision: number, generation: number, error: unknown): void {
    if (
      !this.previewEnabledSignal()
      || this.previewDocumentSignal()?.revision !== revision
      || this.previewDocumentSignal()?.generation !== generation
      || generation !== this.previewGenerationCounter
      || revision !== this.currentRevisionSignal()
    ) return;

    this.reportFailure(error, 'Failed to render preview.');
    this.previewFailureRevisionSignal.set(revision);
    this.previewStatusSignal.set('unavailable');
  }

  /**
   * Saves the resume as a PDF. The cached preview blob is used when one exists;
   * otherwise the server renders one on demand. The preview blob is null whenever
   * the preview pane is off (every narrow screen, #425), so a cache-only download
   * made the app's primary output unreachable on a phone.
   */
  downloadPdf(): void {
    const resume = this.state();
    if (!resume || !this.canDownload() || this.downloadingSignal()) {
      return;
    }
    const document = this.previewDocumentSignal();
    const revision = this.persistedRevisionSignal();
    if (
      document?.revision === revision
      && this.renderedPreviewRevisionSignal() === revision
      && this.previewFailureRevisionSignal() !== revision
    ) {
      this.saveBlob(document.blob);
      return;
    }
    this.downloadingSignal.set(true);
    this.resumesClient.download(resume.id).pipe(retryOnRateLimit()).subscribe({
      next: pdf => {
        this.downloadingSignal.set(false);
        this.saveBlob(pdf);
      },
      error: (error: unknown) => {
        this.downloadingSignal.set(false);
        this.reportFailure(error, 'Failed to download PDF.');
      },
    });
  }

  private saveBlob(blob: Blob): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `resume-${this.state()?.id ?? 'download'}.pdf`;
    // Some mobile browsers and in-app WebViews only honour a programmatic click on
    // an <a download> once the element is actually attached to the document.
    document.body.appendChild(a);
    a.click();
    a.remove();
    // Revoking right after click() races the browser's own read of the blob: this
    // is now the only download path on mobile (#425), where the click can still be
    // queued to start reading the blob asynchronously — revoking before that read
    // begins silently kills the download with no error surfaced anywhere. A 0ms
    // timeout defers the revoke to the next macrotask, after the browser has had a
    // turn to start reading. Do NOT "simplify" this back to a synchronous revoke.
    setTimeout(() => URL.revokeObjectURL(url), 0);
  }

  // Wraps a single request so the pipe it feeds can never be terminated by it: a throttled
  // request is retried on the server's own schedule, and anything still failing afterwards is
  // converted into an `ok: false` emission instead of an error notification.
  private guarded<T>(source: Observable<T>): Observable<SaveOutcome<T>> {
    return source.pipe(
      retryOnRateLimit(),
      map((value): SaveOutcome<T> => ({ ok: true, value })),
      catchError((error: unknown) => of<SaveOutcome<T>>({ ok: false, error })),
    );
  }

  // Turns a failed request into user-visible copy. An exhausted 429 is a different situation
  // from a broken request — the user only has to slow down — so it gets its own wording and
  // the softer warning styling instead of the caller's generic error message.
  private reportFailure(error: unknown, message: string): void {
    const rateLimited = isRateLimited(error);
    this.noticeVariantSignal.set(rateLimited ? 'warning' : 'error');
    this.errorSignal.set(rateLimited ? RATE_LIMIT_MESSAGE : message);
  }

  private queueDebouncedSave<T>(
    key: string,
    source: () => Observable<T>,
    onNext: (value: T) => void,
    errorMsg: string,
  ): void {
    const job = this.createSaveJob(key, source, onNext, errorMsg);
    const queue = this.getSaveQueue(key);
    queue.pending = job;
    queue.ready = false;
    if (queue.timer) clearTimeout(queue.timer);
    queue.timer = setTimeout(() => {
      queue.timer = null;
      queue.ready = true;
      this.drainSaveQueue(key);
    }, ResumesStore.SAVE_DEBOUNCE_MS);
  }

  private queueImmediateSave<T>(
    key: string,
    source: () => Observable<T>,
    onNext: (value: T) => void,
    errorMsg: string,
    replacePending = false,
  ): void {
    if (replacePending) this.cancelPendingSave(key);
    const job = this.createSaveJob(key, source, onNext, errorMsg);
    const queue = this.getSaveQueue(key);
    queue.immediate.push(job);
    this.drainSaveQueue(key);
  }

  private createSaveJob<T>(
    key: string,
    source: () => Observable<T>,
    onNext: (value: T) => void,
    errorMessage: string,
  ): SaveJob {
    const revision = this.markChanged(key);
    const session = this.session;
    return {
      key,
      revision,
      session,
      errorMessage,
      execute: () => this.guarded(source()).pipe(
        map((outcome): SaveOutcome<void> => {
          if (!outcome.ok) return outcome;
          if (session === this.session) onNext(outcome.value);
          return { ok: true, value: undefined };
        }),
      ),
    };
  }

  private markChanged(key: string): number {
    const revision = this.currentRevisionSignal() + 1;
    this.currentRevisionSignal.set(revision);
    this.dirtySaveKeys.update(keys => {
      const next = new Map(keys);
      next.set(key, revision);
      return next;
    });
    this.previewRefresh$.next(null);
    if (this.previewEnabledSignal()) this.previewStatusSignal.set('generating');
    if (this.previewFailureRevisionSignal() !== null) {
      this.previewFailureRevisionSignal.set(null);
      if (this.failedSaveKeys().size === 0) this.clearError();
    }
    return revision;
  }

  private getSaveQueue(key: string): SaveQueue {
    const existing = this.saveQueues.get(key);
    if (existing) return existing;
    const queue: SaveQueue = {
      timer: null,
      pending: null,
      ready: false,
      active: false,
      immediate: [],
    };
    this.saveQueues.set(key, queue);
    return queue;
  }

  private cancelPendingSave(key: string): void {
    const queue = this.saveQueues.get(key);
    if (!queue) return;
    if (queue.timer) clearTimeout(queue.timer);
    queue.timer = null;
    queue.pending = null;
    queue.ready = false;
  }

  private drainSaveQueue(key: string): void {
    const queue = this.saveQueues.get(key);
    if (!queue || queue.active) return;

    const job = queue.immediate.shift()
      ?? (queue.ready ? queue.pending : null);
    if (!job) {
      if (!queue.timer && !queue.pending && queue.immediate.length === 0) {
        this.saveQueues.delete(key);
      }
      return;
    }
    if (job === queue.pending) {
      queue.pending = null;
      queue.ready = false;
    }

    queue.active = true;
    this.beginSave(key);
    job.execute().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(outcome => {
      if (job.session !== this.session) return;
      queue.active = false;
      if (outcome.ok) {
        this.clearDirtyRevision(job.key, job.revision);
        this.settleSave(job.key, 'saved');
      } else {
        this.reportFailure(outcome.error, job.errorMessage);
        this.settleSave(job.key, 'error');
      }
      this.drainSaveQueue(key);
      this.completePersistedRevisionIfSettled();
    });
  }

  private clearDirtyRevision(key: string, revision: number): void {
    this.dirtySaveKeys.update(keys => {
      if (keys.get(key) !== revision) return keys;
      const next = new Map(keys);
      next.delete(key);
      return next;
    });
  }

  private completePersistedRevisionIfSettled(): void {
    if (
      !this.state()
      || this.dirtySaveKeys().size > 0
      || this.inFlightSaveKeys().size > 0
      || this.failedSaveKeys().size > 0
    ) return;

    this.persistedRevisionSignal.set(this.currentRevisionSignal());
    if (this.previewEnabledSignal()) {
      this.startPreviewGeneration(this.persistedRevisionSignal());
    } else {
      this.previewStatusSignal.set('unavailable');
    }
  }

  private startPreviewGeneration(revision: number): void {
    if (!this.state() || !this.previewEnabledSignal() || revision < 0) {
      this.previewStatusSignal.set('unavailable');
      return;
    }
    if (this.previewFailureRevisionSignal() !== null) {
      this.previewFailureRevisionSignal.set(null);
      if (this.failedSaveKeys().size === 0) this.clearError();
    }
    this.previewStatusSignal.set('generating');
    this.previewRefresh$.next({
      revision,
      session: this.session,
      generation: ++this.previewGenerationCounter,
    });
  }

  private isCurrentPreviewRequest(request: PreviewRequest): boolean {
    return request.session === this.session
      && this.previewEnabledSignal()
      && request.generation === this.previewGenerationCounter
      && request.revision === this.currentRevisionSignal()
      && request.revision === this.persistedRevisionSignal()
      && this.dirtySaveKeys().size === 0
      && this.inFlightSaveKeys().size === 0;
  }

  private clearSaveQueues(): void {
    for (const queue of this.saveQueues.values()) {
      if (queue.timer) clearTimeout(queue.timer);
    }
    this.saveQueues.clear();
  }

  private resetSynchronizationState(): void {
    this.session++;
    this.clearSaveQueues();
    this.previewRefresh$.next(null);
    this.state.set(null);
    this.previewDocumentSignal.set(null);
    this.previewStatusSignal.set('unavailable');
    this.previewFailureRevisionSignal.set(null);
    this.currentRevisionSignal.set(0);
    this.persistedRevisionSignal.set(-1);
    this.renderedPreviewRevisionSignal.set(-1);
    this.dirtySaveKeys.set(new Map());
    this.inFlightSaveKeys.set(new Set());
    this.failedSaveKeys.set(new Set());
    this.latestPersonDraft = null;
    this.previewGenerationCounter = 0;
  }

  private beginSave(key: string): void {
    this.inFlightSaveKeys.update(keys => {
      const next = new Set(keys);
      next.add(key);
      return next;
    });
  }

  private settleSave(key: string, result: 'saved' | 'error'): void {
    this.inFlightSaveKeys.update(keys => {
      const next = new Set(keys);
      next.delete(key);
      return next;
    });
    if (result === 'saved') {
      // Same-key requests are serialized, so a success always supersedes any earlier failure
      // for this key. Failures for unrelated keys remain sticky.
      this.failedSaveKeys.update(keys => {
        if (!keys.has(key)) return keys;
        const next = new Set(keys);
        next.delete(key);
        return next;
      });
    } else {
      this.failedSaveKeys.update(keys => (keys.has(key) ? keys : new Set(keys).add(key)));
    }
  }

  updatePerson(data: UpdatePersonRequest): void {
    const resume = this.state();
    if (!resume) return;
    const payload: UpdatePersonRequest = { ...data, dateOfBirth: data.dateOfBirth || null };
    this.latestPersonDraft = payload;
    if (resume.person) {
      this.state.update(r => r ? {
        ...r,
        person: { ...r.person!, ...payload, dateOfBirth: payload.dateOfBirth ?? '' },
      } : r);
    }
    this.queueDebouncedSave(
      ResumesStore.PERSON_KEY,
      () => {
        const current = this.state();
        if (!current) return of(null as Person | null);
        return current.person
          ? this.personClient.update(current.id, current.person.id, payload).pipe(
              map(() => null as Person | null),
            )
          : this.personClient.create(current.id, payload).pipe(
              map(person => person as Person | null),
            );
      },
      person => {
        const latest = this.latestPersonDraft ?? payload;
        const formedTitle = [payload.firstname, payload.lastname]
          .map(part => part?.trim())
          .filter((part): part is string => Boolean(part))
          .join(' ')
          .slice(0, 100);
        this.state.update(r => r ? {
          ...r,
          ...(person
            ? { person: { ...person, ...latest, id: person.id, dateOfBirth: latest.dateOfBirth ?? '' } }
            : {}),
          title: r.title ?? (formedTitle || null),
        } : r);
      },
      'Failed to save person details.',
    );
  }

  updateTitle(value: string): void {
    const resume = this.state();
    if (!resume) return;
    this.queueImmediateSave(
      'title',
      () => this.resumesClient.updateTitle(resume.id, value),
      () => this.state.update(r => r ? { ...r, title: value } : r),
      'Failed to save title.',
    );
  }

  updateSummary(html: string): void {
    const resume = this.state();
    if (!resume) return;
    this.state.update(r => r ? { ...r, summary: html } : r);
    this.queueDebouncedSave(
      'summary',
      () => this.resumesClient.updateSummary(resume.id, html),
      () => undefined,
      'Failed to save summary.',
    );
  }

  updateHobbies(hobbies: string): void {
    const resume = this.state();
    if (!resume) return;
    this.state.update(r => r ? { ...r, hobbies } : r);
    this.queueDebouncedSave(
      'hobbies',
      () => this.resumesClient.updateHobbies(resume.id, hobbies),
      () => undefined,
      'Failed to save hobbies.',
    );
  }

  updateLocalization(localization: string): void {
    const resume = this.state();
    if (!resume) return;
    this.state.update(r => r ? { ...r, localization } : r);
    this.queueImmediateSave(
      'localization',
      () => this.resumesClient.updateLocalization(resume.id, localization),
      () => undefined,
      'Failed to save localization.',
    );
  }

  // Education
  addEducation(): void {
    const resume = this.state();
    if (!resume) return;
    this.queueImmediateSave(
      'education:add',
      () => this.educationsClient.create(resume.id),
      item => this.state.update(r => r ? { ...r, educations: [...r.educations, item] } : r),
      'Failed to add education.',
    );
  }

  updateEducation(data: UpdateEducationRequest & { id: string }): void {
    const resume = this.state();
    if (!resume) return;
    const payload = { ...data, startDate: data.startDate || null, endDate: data.endDate || null };
    this.queueDebouncedSave(
      `education:${data.id}`,
      () => this.educationsClient.update(resume.id, payload),
      () => {
        this.state.update(r => r ? {
          ...r,
          educations: r.educations.map(e => e.id === data.id ? { ...e, ...payload } as Education : e),
        } : r);
      },
      'Failed to save education.',
    );
  }

  deleteEducation(item: Orderly): void {
    const resume = this.state();
    if (!resume) return;
    this.queueImmediateSave(
      `education:${item.id}`,
      () => this.educationsClient.delete(resume.id, item.id),
      () => this.state.update(r => r ? {
        ...r,
        educations: r.educations.filter(e => e.id !== item.id),
      } : r),
      'Failed to delete education.',
      true,
    );
  }

  reorderEducations(event: CdkDragDrop<Orderly[]>): void {
    const resume = this.state();
    if (!resume) return;
    const previous = resume.educations;
    const list = [...previous];
    moveItemInArray(list, event.previousIndex, event.currentIndex);
    const reindex = list.map((item, i) => ({ ...item, order: i }));
    this.state.update((r) => (r ? { ...r, educations: reindex } : r));
    this.queueImmediateSave(
      'education:reorder',
      () => this.educationsClient.reorder(
        resume.id,
        reindex.map((i) => i.id),
      ),
      () => undefined,
      'Failed to reorder educations.',
    );
  }

  // Experience
  addExperience(): void {
    const resume = this.state();
    if (!resume) return;
    this.queueImmediateSave(
      'experience:add',
      () => this.experiencesClient.create(resume.id),
      item => this.state.update(r => r ? { ...r, experience: [...r.experience, item] } : r),
      'Failed to add experience.',
    );
  }

  updateExperience(data: UpdateExperienceRequest & { id: string }): void {
    const resume = this.state();
    if (!resume) return;
    const payload = { ...data, startDate: data.startDate || null, endDate: data.endDate || null };
    this.queueDebouncedSave(
      `experience:${data.id}`,
      () => this.experiencesClient.update(resume.id, payload),
      () => {
        this.state.update(r => r ? {
          ...r,
          experience: r.experience.map(e => e.id === data.id ? { ...e, ...payload } as Experience : e),
        } : r);
      },
      'Failed to save experience.',
    );
  }

  deleteExperience(item: Orderly): void {
    const resume = this.state();
    if (!resume) return;
    this.queueImmediateSave(
      `experience:${item.id}`,
      () => this.experiencesClient.delete(resume.id, item.id),
      () => this.state.update(r => r ? {
        ...r,
        experience: r.experience.filter(e => e.id !== item.id),
      } : r),
      'Failed to delete experience.',
      true,
    );
  }

  reorderExperiences(event: CdkDragDrop<Orderly[]>): void {
    const resume = this.state();
    if (!resume) return;
    const previous = resume.experience;
    const list = [...previous];
    moveItemInArray(list, event.previousIndex, event.currentIndex);
    const reindex = list.map((item, i) => ({ ...item, order: i }));
    this.state.update((r) => (r ? { ...r, experience: reindex } : r));
    this.queueImmediateSave(
      'experience:reorder',
      () => this.experiencesClient.reorder(
        resume.id,
        reindex.map((i) => i.id),
      ),
      () => undefined,
      'Failed to reorder experiences.',
    );
  }

  // Skills
  addSkill(): void {
    const resume = this.state();
    if (!resume) return;
    this.queueImmediateSave(
      'skill:add',
      () => this.skillsClient.create(resume.id),
      item => this.state.update(r => r ? { ...r, skills: [...r.skills, item] } : r),
      'Failed to add skill.',
    );
  }

  updateSkill(data: UpdateSkillRequest & { id: string }): void {
    const resume = this.state();
    if (!resume) return;
    this.queueDebouncedSave(
      `skill:${data.id}`,
      () => this.skillsClient.update(resume.id, data),
      () => {
        this.state.update(r => r ? {
          ...r,
          skills: r.skills.map(s => s.id === data.id ? { ...s, ...data } : s),
        } : r);
      },
      'Failed to save skill.',
    );
  }

  deleteSkill(item: Orderly): void {
    const resume = this.state();
    if (!resume) return;
    this.queueImmediateSave(
      `skill:${item.id}`,
      () => this.skillsClient.delete(resume.id, item.id),
      () => this.state.update(r => r ? {
        ...r,
        skills: r.skills.filter(s => s.id !== item.id),
      } : r),
      'Failed to delete skill.',
      true,
    );
  }

  reorderSkills(event: CdkDragDrop<Orderly[]>): void {
    const resume = this.state();
    if (!resume) return;
    const previous = resume.skills;
    const list = [...previous];
    moveItemInArray(list, event.previousIndex, event.currentIndex);
    const reindex = list.map((item, i) => ({ ...item, order: i }));
    this.state.update((r) => (r ? { ...r, skills: reindex } : r));
    this.queueImmediateSave(
      'skill:reorder',
      () => this.skillsClient.reorder(
        resume.id,
        reindex.map((i) => i.id),
      ),
      () => undefined,
      'Failed to reorder skills.',
    );
  }

  // Languages
  addLanguage(): void {
    const resume = this.state();
    if (!resume) return;
    this.queueImmediateSave(
      'language:add',
      () => this.languagesClient.create(resume.id),
      item => this.state.update(r => r ? { ...r, languages: [...r.languages, item] } : r),
      'Failed to add language.',
    );
  }

  updateLanguage(data: UpdateLanguageRequest & { id: string }): void {
    const resume = this.state();
    if (!resume) return;
    this.queueDebouncedSave(
      `language:${data.id}`,
      () => this.languagesClient.update(resume.id, data),
      () => {
        this.state.update(r => r ? {
          ...r,
          languages: r.languages.map(l => l.id === data.id ? { ...l, ...data } : l),
        } : r);
      },
      'Failed to save language.',
    );
  }

  deleteLanguage(item: Orderly): void {
    const resume = this.state();
    if (!resume) return;
    this.queueImmediateSave(
      `language:${item.id}`,
      () => this.languagesClient.delete(resume.id, item.id),
      () => this.state.update(r => r ? {
        ...r,
        languages: r.languages.filter(l => l.id !== item.id),
      } : r),
      'Failed to delete language.',
      true,
    );
  }

  reorderLanguages(event: CdkDragDrop<Orderly[]>): void {
    const resume = this.state();
    if (!resume) return;
    const previous = resume.languages;
    const list = [...previous];
    moveItemInArray(list, event.previousIndex, event.currentIndex);
    const reindex = list.map((item, i) => ({ ...item, order: i }));
    this.state.update((r) => (r ? { ...r, languages: reindex } : r));
    this.queueImmediateSave(
      'language:reorder',
      () => this.languagesClient.reorder(
        resume.id,
        reindex.map((i) => i.id),
      ),
      () => undefined,
      'Failed to reorder languages.',
    );
  }

  // Links
  addLink(): void {
    const resume = this.state();
    if (!resume) return;
    this.queueImmediateSave(
      'link:add',
      () => this.linksClient.create(resume.id),
      item => this.state.update(r => r ? { ...r, links: [...r.links, item] } : r),
      'Failed to add link.',
    );
  }

  updateLink(data: UpdateLinkRequest & { id: string }): void {
    const resume = this.state();
    if (!resume) return;
    this.queueDebouncedSave(
      `link:${data.id}`,
      () => this.linksClient.update(resume.id, data),
      () => {
        this.state.update(r => r ? {
          ...r,
          links: r.links.map(l => l.id === data.id ? { ...l, ...data } : l),
        } : r);
      },
      'Failed to save link.',
    );
  }

  deleteLink(item: Orderly): void {
    const resume = this.state();
    if (!resume) return;
    this.queueImmediateSave(
      `link:${item.id}`,
      () => this.linksClient.delete(resume.id, item.id),
      () => this.state.update(r => r ? {
        ...r,
        links: r.links.filter(l => l.id !== item.id),
      } : r),
      'Failed to delete link.',
      true,
    );
  }

  reorderLinks(event: CdkDragDrop<Orderly[]>): void {
    const resume = this.state();
    if (!resume) return;
    const previous = resume.links;
    const list = [...previous];
    moveItemInArray(list, event.previousIndex, event.currentIndex);
    const reindex = list.map((item, i) => ({ ...item, order: i }));
    this.state.update((r) => (r ? { ...r, links: reindex } : r));
    this.queueImmediateSave(
      'link:reorder',
      () => this.linksClient.reorder(
        resume.id,
        reindex.map((i) => i.id),
      ),
      () => undefined,
      'Failed to reorder links.',
    );
  }

  // Courses
  addCourse(): void {
    const resume = this.state();
    if (!resume) return;
    this.queueImmediateSave(
      'course:add',
      () => this.coursesClient.create(resume.id),
      item => this.state.update(r => r ? { ...r, courses: [...r.courses, item] } : r),
      'Failed to add course.',
    );
  }

  updateCourse(data: UpdateCourseRequest & { id: string }): void {
    const resume = this.state();
    if (!resume) return;
    const payload = { ...data, startDate: data.startDate || null, endDate: data.endDate || null };
    this.queueDebouncedSave(
      `course:${data.id}`,
      () => this.coursesClient.update(resume.id, payload),
      () => {
        this.state.update(r => r ? {
          ...r,
          courses: r.courses.map(c => c.id === data.id ? { ...c, ...payload } as Course : c),
        } : r);
      },
      'Failed to save course.',
    );
  }

  deleteCourse(item: Orderly): void {
    const resume = this.state();
    if (!resume) return;
    this.queueImmediateSave(
      `course:${item.id}`,
      () => this.coursesClient.delete(resume.id, item.id),
      () => this.state.update(r => r ? {
        ...r,
        courses: r.courses.filter(c => c.id !== item.id),
      } : r),
      'Failed to delete course.',
      true,
    );
  }

  reorderCourses(event: CdkDragDrop<Orderly[]>): void {
    const resume = this.state();
    if (!resume) return;
    const previous = resume.courses;
    const list = [...previous];
    moveItemInArray(list, event.previousIndex, event.currentIndex);
    const reindex = list.map((item, i) => ({ ...item, order: i }));
    this.state.update((r) => (r ? { ...r, courses: reindex } : r));
    this.queueImmediateSave(
      'course:reorder',
      () => this.coursesClient.reorder(
        resume.id,
        reindex.map((i) => i.id),
      ),
      () => undefined,
      'Failed to reorder courses.',
    );
  }

  // Projects
  // Project endpoints return the created resource only for POST. For the 204
  // mutations, apply the server-confirmed request only after it succeeds.
  addProject(): void {
    const resume = this.state();
    if (!resume) return;
    this.queueImmediateSave(
      'project:add',
      () => this.projectsClient.create(resume.id),
      item => this.state.update(r => r ? { ...r, projects: [...r.projects, item] } : r),
      'Failed to add project.',
    );
  }

  updateProject(data: UpdateProjectRequest & { id: string }): void {
    const resume = this.state();
    if (!resume) return;
    const payload = {
      ...data,
      startDate: data.startDate || null,
      endDate: data.endDate || null,
      url: data.url?.trim() || null,
    };
    this.queueDebouncedSave(
      `project:${data.id}`,
      () => this.projectsClient.update(resume.id, payload),
      () => this.state.update(r => r ? {
        ...r,
        projects: r.projects.map(project => project.id === data.id
          ? { ...project, ...payload } as Project
          : project),
      } : r),
      'Failed to save project.',
    );
  }

  deleteProject(item: Orderly): void {
    const resume = this.state();
    if (!resume) return;
    this.queueImmediateSave(
      `project:${item.id}`,
      () => this.projectsClient.delete(resume.id, item.id),
      () => this.state.update(r => r ? {
        ...r,
        projects: r.projects.filter(project => project.id !== item.id),
      } : r),
      'Failed to delete project.',
      true,
    );
  }

  reorderProjects(event: CdkDragDrop<Orderly[]>): void {
    const resume = this.state();
    if (!resume) return;
    const list = [...resume.projects];
    moveItemInArray(list, event.previousIndex, event.currentIndex);
    const reindexed = list.map((project, index) => ({ ...project, order: index }));
    this.queueImmediateSave(
      'project:reorder',
      () => this.projectsClient.reorder(resume.id, reindexed.map(project => project.id)),
      () => this.state.update(r => r ? { ...r, projects: reindexed } : r),
      'Failed to reorder projects.',
    );
  }
}
