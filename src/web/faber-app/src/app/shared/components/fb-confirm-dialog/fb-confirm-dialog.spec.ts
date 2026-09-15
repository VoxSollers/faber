import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DIALOG_DATA, DialogRef } from '@angular/cdk/dialog';
import { FbConfirmDialog } from './fb-confirm-dialog';

describe('FbConfirmDialog', () => {
  let fixture: ComponentFixture<FbConfirmDialog>;
  let closeSpy: ReturnType<typeof vi.fn>;

  beforeEach(async () => {
    closeSpy = vi.fn();

    await TestBed.configureTestingModule({
      imports: [FbConfirmDialog],
    })
      .overrideProvider(DIALOG_DATA, {
        useValue: { title: 'Delete Resume', message: 'Are you sure?' },
      })
      .overrideProvider(DialogRef, { useValue: { close: closeSpy } })
      .compileComponents();

    fixture = TestBed.createComponent(FbConfirmDialog);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('renders title from DIALOG_DATA', () => {
    const el: HTMLElement = fixture.nativeElement.querySelector('#confirm-dialog-title');
    expect(el.textContent?.trim()).toBe('Delete Resume');
  });

  it('renders message from DIALOG_DATA', () => {
    const el: HTMLElement = fixture.nativeElement.querySelector('#confirm-dialog-message');
    expect(el.textContent?.trim()).toBe('Are you sure?');
  });

  it('confirm button calls dialogRef.close(true)', () => {
    fixture.nativeElement.querySelector('[data-testid="confirm-btn"]').click();
    expect(closeSpy).toHaveBeenCalledWith(true);
  });

  it('cancel button calls dialogRef.close(false)', () => {
    fixture.nativeElement.querySelector('[data-testid="cancel-btn"]').click();
    expect(closeSpy).toHaveBeenCalledWith(false);
  });

  it('title element has id for ARIA reference', () => {
    expect(fixture.nativeElement.querySelector('#confirm-dialog-title')).not.toBeNull();
  });

  it('message element has id for ARIA reference', () => {
    expect(fixture.nativeElement.querySelector('#confirm-dialog-message')).not.toBeNull();
  });

  it('confirm button shows default label "Confirm" when confirmLabel is not provided', () => {
    const btn: HTMLElement = fixture.nativeElement.querySelector('[data-testid="confirm-btn"]');
    expect(btn.textContent?.trim()).toBe('Confirm');
  });
});

describe('FbConfirmDialog with custom confirmLabel', () => {
  let customFixture: ComponentFixture<FbConfirmDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FbConfirmDialog],
    })
      .overrideProvider(DIALOG_DATA, {
        useValue: { title: 'Delete', message: 'Sure?', confirmLabel: 'Yes, delete' },
      })
      .overrideProvider(DialogRef, { useValue: { close: vi.fn() } })
      .compileComponents();

    customFixture = TestBed.createComponent(FbConfirmDialog);
    customFixture.detectChanges();
  });

  it('confirm button shows custom confirmLabel when provided', () => {
    const btn: HTMLElement = customFixture.nativeElement.querySelector('[data-testid="confirm-btn"]');
    expect(btn.textContent?.trim()).toBe('Yes, delete');
  });
});
