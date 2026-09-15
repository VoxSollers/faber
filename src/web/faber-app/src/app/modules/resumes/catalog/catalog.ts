import { ChangeDetectionStrategy, Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { Dialog } from '@angular/cdk/dialog';
import { LucideAngularModule, LucideIconProvider, LUCIDE_ICONS, BadgePlus } from 'lucide-angular';
import { ResumesClient } from '../resumes-client';
import { Resume } from '../resume-response';
import { ResumeCard } from './resume-card/resume-card';
import { FbButton } from '../../../shared/components/fb-button/fb-button';
import { FbSpinner } from '../../../shared/components/fb-spinner/fb-spinner';
import { FbConfirmDialog } from '../../../shared/components/fb-confirm-dialog/fb-confirm-dialog';
import { FbToast } from '../../../shared/components/fb-toast/fb-toast';

@Component({
  selector: 'app-catalog',
  imports: [ResumeCard, LucideAngularModule, FbButton, FbSpinner, FbToast],
  providers: [{ provide: LUCIDE_ICONS, multi: true, useValue: new LucideIconProvider({ BadgePlus }) }],
  templateUrl: './catalog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Catalog implements OnInit {
  private readonly client = inject(ResumesClient);
  private readonly router = inject(Router);
  private readonly dialog = inject(Dialog);
  private readonly destroyRef = inject(DestroyRef);

  readonly resumes = signal<Resume[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadResumes();
  }

  private loadResumes(): void {
    this.loading.set(true);
    this.client.getAll().subscribe({
      next: resumes => {
        this.resumes.set(resumes);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to load resumes.');
        this.loading.set(false);
      },
    });
  }

  createResume(): void {
    this.client.create().subscribe({
      next: resume => this.router.navigate(['/resumes', resume.id]),
      error: () => this.error.set('Failed to create resume.'),
    });
  }

  deleteResume(id: string): void {
    const dialogRef = this.dialog.open(FbConfirmDialog, {
      data: {
        title: 'Delete Resume',
        message: 'Are you sure you want to delete this resume? This action cannot be undone.',
        confirmLabel: 'Delete',
      },
      panelClass: 'fb-dialog-panel',
      backdropClass: 'fb-dialog-backdrop',
    });

    dialogRef.closed.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(confirmed => {
      if (confirmed) {
        this.client.delete(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
          next: () => this.resumes.update(list => list.filter(r => r.id !== id)),
          error: () => this.error.set('Failed to delete resume.'),
        });
      }
    });
  }

  navigateTo(id: string): void {
    this.router.navigate(['/resumes', id]);
  }

  renameResume(id: string, title: string): void {
    this.client.updateTitle(id, title).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => this.resumes.update(list =>
        list.map(r => (r.id === id ? { ...r, title } : r)),
      ),
      error: () => this.error.set('Failed to rename resume.'),
    });
  }
}
