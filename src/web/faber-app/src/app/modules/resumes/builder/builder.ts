import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  ElementRef,
  inject,
  Injector,
  OnInit,
  signal,
  viewChild,
} from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Dialog } from '@angular/cdk/dialog';
import {
  LucideAngularModule,
  LucideIconProvider,
  LUCIDE_ICONS,
  User,
  FileText,
  Briefcase,
  GraduationCap,
  Zap,
  Globe,
  Link2,
  BookOpen,
  Heart,
  ChevronLeft,
  ChevronRight,
  ChevronsUpDown,
  ChevronsDownUp,
  ArrowLeft,
  Check,
  CircleCheck,
  CircleAlert,
  Download,
  Eye,
  Pencil,
  Menu,
} from 'lucide-angular';
import { ResumesStore } from '../resumes-store';
import { Editor } from './editor/editor';
import { Preview } from './preview/preview';
import { BuilderActions } from './builder-actions/builder-actions';
import { SectionSheet, SECTION_SHEET_ITEMS } from './section-sheet/section-sheet';
import { FbSpinner } from '../../../shared/components/fb-spinner/fb-spinner';
import { FbToast } from '../../../shared/components/fb-toast/fb-toast';
import { FbThemeToggle } from '../../../shared/components/fb-theme-toggle/fb-theme-toggle';
import { FbInitials } from '../../../shared/components/fb-initials/fb-initials';
import { AppUserMenu } from '../../../core/layouts/app-navbar/app-user-menu/app-user-menu';
import { AuthStore } from '../../../core/auth/auth-store';
import { APP_ROUTES } from '../../../core/routes/app-routes';

@Component({
  selector: 'app-builder',
  imports: [Editor, Preview, BuilderActions, FbSpinner, LucideAngularModule, FbToast, FbThemeToggle, FbInitials, AppUserMenu],
  providers: [
    {
      provide: LUCIDE_ICONS,
      multi: true,
      useValue: new LucideIconProvider({
        User, FileText, Briefcase, GraduationCap, Zap, Globe,
        Link2, BookOpen, Heart, ChevronLeft, ChevronRight, ChevronsUpDown, ChevronsDownUp,
        ArrowLeft, Check, CircleCheck, CircleAlert, Download, Eye, Pencil, Menu,
      }),
    },
  ],
  templateUrl: './builder.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Builder implements OnInit {
  protected readonly store = inject(ResumesStore);
  protected readonly authStore = inject(AuthStore);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly document = inject(DOCUMENT);
  private readonly dialog = inject(Dialog);
  private readonly injector = inject(Injector);

  protected readonly isUserMenuVisible = signal(false);
  protected readonly isWideScreen = signal(
    typeof window !== 'undefined'
      ? window.matchMedia('(min-width: 768px)').matches
      : true,
  );

  /** Mobile only: the preview replaces the editor rather than sharing the row. */
  protected readonly isMobilePreviewOpen = signal(false);

  constructor() {
    effect(() => {
      this.store.setPreviewEnabled(this.isWideScreen() || this.isMobilePreviewOpen());
    });

    effect(() => {
      if (this.isWideScreen()) {
        this.isMobilePreviewOpen.set(false);
      }
    });

    effect(() => {
      if (this.isEditingTitle()) {
        this.titleInput()?.nativeElement.focus();
      }
    });

    if (typeof window !== 'undefined') {
      const mq = window.matchMedia('(min-width: 768px)');
      const onChange = (e: MediaQueryListEvent) => this.isWideScreen.set(e.matches);
      mq.addEventListener('change', onChange);
      this.destroyRef.onDestroy(() => mq.removeEventListener('change', onChange));
    }
  }

  private readonly profileAnchor = viewChild<ElementRef>('profileAnchor');
  private readonly userMenuRef = viewChild('userMenu', { read: ElementRef });
  private readonly titleInput = viewChild<ElementRef<HTMLInputElement>>('titleInput');
  private readonly anchorRect = signal<DOMRect | null>(null);

  protected readonly userMenuTop = computed(() => {
    const rect = this.anchorRect();
    return rect ? rect.bottom + 8 : 0;
  });

  protected readonly userMenuRight = computed(() => {
    const rect = this.anchorRect();
    return rect ? window.innerWidth - rect.right : 0;
  });

  protected readonly resumeTitle = computed(() => this.store.title() || 'Resume');

  protected readonly isEditingTitle = signal(false);

  /**
   * Live mirror of what is typed in the editor. An invisible sizer span renders
   * this alongside the input in the same grid cell, so the editing box is always
   * exactly as wide as its own text — which is what keeps the pill from resizing
   * and keeps the accent underline flush with the title.
   */
  protected readonly draftTitle = signal('');

  protected startEditingTitle(): void {
    this.draftTitle.set(this.store.title());
    this.isEditingTitle.set(true);
  }

  protected commitTitle(value: string): void {
    const trimmed = value.trim();
    if (trimmed !== this.store.title()) {
      this.store.updateTitle(trimmed);
    }
    this.isEditingTitle.set(false);
  }

  protected cancelEditingTitle(): void {
    this.isEditingTitle.set(false);
  }

  protected readonly railItems = SECTION_SHEET_ITEMS;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.store.loadResume(id);
    this.registerOutsideClickListener();
  }

  // Capture phase runs document → target before any bubble-phase
  // stopPropagation (e.g. the Material datepicker toggle, which lives inside
  // the builder), so a click outside the menu is detected even when the
  // target swallows it on the way up.
  private registerOutsideClickListener(): void {
    const handler = (event: MouseEvent): void => this.handleOutsideClick(event);
    this.document.addEventListener('click', handler, { capture: true });
    this.destroyRef.onDestroy(() =>
      this.document.removeEventListener('click', handler, { capture: true }),
    );
  }

  private handleOutsideClick(event: MouseEvent): void {
    if (!this.isUserMenuVisible()) {
      return;
    }
    const target = event.target as Node;
    const menu = this.userMenuRef()?.nativeElement as HTMLElement | undefined;
    const trigger = this.profileAnchor()?.nativeElement as HTMLElement | undefined;
    if (menu?.contains(target) || trigger?.contains(target)) {
      return;
    }
    this.isUserMenuVisible.set(false);
  }

  protected scrollTo(sectionId: string): void {
    this.store.requestSection(sectionId);
  }

  protected expandAllSections(): void {
    this.store.requestAllSections('expand');
  }

  protected collapseAllSections(): void {
    this.store.requestAllSections('collapse');
  }

  protected togglePreview(): void {
    this.isMobilePreviewOpen.update(open => !open);
  }

  protected openSectionSheet(): void {
    this.dialog.open(SectionSheet, {
      // `ResumesStore` is provided on the resume route, not the root injector
      // (app.routes.ts), and `Dialog.open()` doesn't inherit the caller's
      // injector by default — it resolves the dialog's content against its
      // own (root) injector. Without forwarding `injector` here, `SectionSheet`
      // injecting `ResumesStore` throws NG0201 the moment it's attached,
      // aborting the panel after the backdrop is already up (#425 follow-up).
      injector: this.injector,
      panelClass: 'fb-sheet-panel',
      backdropClass: 'fb-dialog-backdrop',
      width: '100%',
      maxWidth: '100%',
      ariaLabel: 'Resume sections',
      ariaModal: true,
    });
  }

  protected goBack(): void {
    this.router.navigate([APP_ROUTES.resumes]);
  }

  protected toggleUserMenu(): void {
    const anchor = this.profileAnchor();
    if (anchor) {
      this.anchorRect.set(anchor.nativeElement.getBoundingClientRect());
    }
    this.isUserMenuVisible.set(!this.isUserMenuVisible());
  }
}
