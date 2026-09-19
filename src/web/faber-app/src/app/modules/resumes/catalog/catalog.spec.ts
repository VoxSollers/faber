import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { Dialog, DialogRef } from '@angular/cdk/dialog';
import { Router } from '@angular/router';
import { Catalog } from './catalog';
import { ResumesClient } from '../resumes-client';
import { FbConfirmDialog } from '../../../shared/components/fb-confirm-dialog/fb-confirm-dialog';
import { Resume } from '../resume-response';
import { toastHarness } from '../../../shared/testing/toast-harness';

const mockResume: Resume = {
  id: 'r1',
  createdAt: '2024-01-01T00:00:00Z',
  person: null,
  title: null,
  summary: '',
  localization: 'en-us',
  hobbies: '',
  experience: [],
  educations: [],
  courses: [],
  projects: [],
  links: [],
  skills: [],
  languages: [],
};

describe('Catalog', () => {
  let component: Catalog;
  let fixture: ComponentFixture<Catalog>;
  type MockResumesClient = Pick<
    Record<keyof ResumesClient, ReturnType<typeof vi.fn>>,
    'getAll' | 'create' | 'delete' | 'updateTitle'
  >;
  let mockClient: MockResumesClient;
  let mockDialog: { open: ReturnType<typeof vi.fn> };
  let mockRouter: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    mockClient = {
      getAll: vi.fn(),
      create: vi.fn(),
      delete: vi.fn(),
      updateTitle: vi.fn(),
    };
    mockDialog = { open: vi.fn() };
    mockRouter = { navigate: vi.fn() };

    mockClient.getAll.mockReturnValue(of([mockResume]));

    await TestBed.configureTestingModule({
      imports: [Catalog],
    })
      .overrideProvider(ResumesClient, { useValue: mockClient })
      .overrideProvider(Dialog, { useValue: mockDialog })
      .overrideProvider(Router, { useValue: mockRouter })
      .compileComponents();

    fixture = TestBed.createComponent(Catalog);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('loadResumes', () => {
    it('sets error when getAll fails', async () => {
      mockClient.getAll.mockReturnValue(throwError(() => new Error('network')));
      const freshFixture = TestBed.createComponent(Catalog);
      await freshFixture.whenStable();
      expect(freshFixture.componentInstance.error()).toBe('Failed to load resumes.');
      freshFixture.destroy();
    });

    it('renders toast when error is set', () => {
      const toast = toastHarness(fixture);
      component.error.set('Failed to load resumes.');
      fixture.detectChanges();
      expect(toast.query()).toBeTruthy();
      expect(toast.text()).toContain('Failed to load resumes.');
    });

    it('hides toast when error is cleared', () => {
      component.error.set('Some error');
      fixture.detectChanges();
      component.error.set(null);
      expect(component.error()).toBeNull();
    });

    it('clears error when toast is dismissed', () => {
      component.error.set('Failed to load resumes.');
      fixture.detectChanges();
      toastHarness(fixture).dismiss();
      expect(component.error()).toBeNull();
    });
  });

  describe('createResume', () => {
    it('sets error when create fails', () => {
      mockClient.create.mockReturnValue(throwError(() => new Error('network')));
      component.createResume();
      expect(component.error()).toBe('Failed to create resume.');
    });
  });

  describe('deleteResume', () => {
    it('opens FbConfirmDialog when delete is triggered', () => {
      mockDialog.open.mockReturnValue({ closed: of(undefined) } as unknown as DialogRef<boolean>);

      component.deleteResume('r1');

      expect(mockDialog.open).toHaveBeenCalledWith(
        FbConfirmDialog,
        expect.objectContaining({
          data: expect.objectContaining({
            title: expect.any(String),
            message: expect.any(String),
            confirmLabel: 'Delete',
          }),
          panelClass: 'fb-dialog-panel',
          backdropClass: 'fb-dialog-backdrop',
        })
      );
    });

    it('calls client.delete when user confirms', () => {
      mockDialog.open.mockReturnValue({ closed: of(true) } as unknown as DialogRef<boolean>);
      mockClient.delete.mockReturnValue(of(void 0));

      component.deleteResume('r1');

      expect(mockClient.delete).toHaveBeenCalledWith('r1');
    });

    it('removes deleted resume from the list when confirmed', () => {
      mockDialog.open.mockReturnValue({ closed: of(true) } as unknown as DialogRef<boolean>);
      mockClient.delete.mockReturnValue(of(void 0));

      component.deleteResume(mockResume.id);

      expect(component.resumes()).toEqual([]);
    });

    it('does not call client.delete when user cancels', () => {
      mockDialog.open.mockReturnValue({ closed: of(false) } as unknown as DialogRef<boolean>);

      component.deleteResume('r1');

      expect(mockClient.delete).not.toHaveBeenCalled();
    });

    it('does not call client.delete when dialog is dismissed', () => {
      mockDialog.open.mockReturnValue({ closed: of(undefined) } as unknown as DialogRef<boolean>);

      component.deleteResume('r1');

      expect(mockClient.delete).not.toHaveBeenCalled();
    });

    it('sets error when client.delete fails', () => {
      mockDialog.open.mockReturnValue({ closed: of(true) } as unknown as DialogRef<boolean>);
      mockClient.delete.mockReturnValue(throwError(() => new Error('network')));

      component.deleteResume('r1');

      expect(component.error()).toBe('Failed to delete resume.');
    });

    it('does not remove resume from list when user cancels', () => {
      mockDialog.open.mockReturnValue({ closed: of(false) } as unknown as DialogRef<boolean>);

      component.deleteResume(mockResume.id);

      expect(component.resumes()).toEqual([mockResume]);
    });

    it('does not remove resume from list when dialog is dismissed', () => {
      mockDialog.open.mockReturnValue({ closed: of(undefined) } as unknown as DialogRef<boolean>);

      component.deleteResume(mockResume.id);

      expect(component.resumes()).toEqual([mockResume]);
    });
  });

  describe('renameResume', () => {
    it('calls client.updateTitle and updates the resume title in the local list', () => {
      mockClient.updateTitle.mockReturnValue(of(void 0));

      component.renameResume(mockResume.id, 'New Title');

      expect(mockClient.updateTitle).toHaveBeenCalledWith(mockResume.id, 'New Title');
      expect(component.resumes()).toEqual([{ ...mockResume, title: 'New Title' }]);
    });

    it('sets error when client.updateTitle fails', () => {
      mockClient.updateTitle.mockReturnValue(throwError(() => new Error('network')));

      component.renameResume(mockResume.id, 'New Title');

      expect(component.error()).toBe('Failed to rename resume.');
    });
  });
});
