import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { DialogRef } from '@angular/cdk/dialog';
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
  ChevronsUpDown,
  ChevronsDownUp,
} from 'lucide-angular';
import { ResumesStore } from '../../resumes-store';

/** One jump target in the builder's section navigation. */
export interface SectionSheetItem {
  readonly id: string;
  readonly label: string;
  readonly icon: string;
}

/**
 * The builder's nine sections, in editor order. Shared by the desktop rail and
 * the mobile sheet so the two lists cannot drift apart.
 */
export const SECTION_SHEET_ITEMS: readonly SectionSheetItem[] = [
  { id: 'person',     label: 'Personal Details',     icon: 'user' },
  { id: 'summary',    label: 'Professional Summary', icon: 'file-text' },
  { id: 'experience', label: 'Experience',            icon: 'briefcase' },
  { id: 'education',  label: 'Education',            icon: 'graduation-cap' },
  { id: 'skill',      label: 'Skills',               icon: 'zap' },
  { id: 'language',   label: 'Languages',            icon: 'globe' },
  { id: 'link',       label: 'Links',                icon: 'link-2' },
  { id: 'course',     label: 'Courses',              icon: 'book-open' },
  { id: 'hobby',      label: 'Hobbies',              icon: 'heart' },
];

/**
 * Mobile section navigation. The desktop rail is a persistent 60px column —
 * a sixth of a 375px screen spent duplicating what scrolling already does
 * (#425) — so below `md` the same targets live here instead, costing nothing
 * until opened. Focus trap, Escape and the backdrop come from CDK Dialog.
 */
@Component({
  selector: 'app-section-sheet',
  imports: [LucideAngularModule],
  providers: [
    {
      provide: LUCIDE_ICONS,
      multi: true,
      useValue: new LucideIconProvider({
        User, FileText, Briefcase, GraduationCap, Zap, Globe,
        Link2, BookOpen, Heart, ChevronsUpDown, ChevronsDownUp,
      }),
    },
  ],
  templateUrl: './section-sheet.html',
  styleUrl: './section-sheet.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SectionSheet {
  private readonly store = inject(ResumesStore);
  private readonly dialogRef = inject(DialogRef<void>);

  protected readonly items = SECTION_SHEET_ITEMS;

  protected select(id: string): void {
    this.store.requestSection(id);
    this.dialogRef.close();
  }

  protected expandAll(): void {
    this.store.requestAllSections('expand');
    this.dialogRef.close();
  }

  protected collapseAll(): void {
    this.store.requestAllSections('collapse');
    this.dialogRef.close();
  }

  protected close(): void {
    this.dialogRef.close();
  }
}
