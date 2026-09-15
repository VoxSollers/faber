import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FbToast } from './fb-toast';
import { expectNoAxeViolations } from '../../testing/axe';

describe('FbToast', () => {
  let component: FbToast;
  let fixture: ComponentFixture<FbToast>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FbToast],
    }).compileComponents();

    fixture = TestBed.createComponent(FbToast);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => vi.useRealTimers());

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should default variant to "error"', () => {
    expect(component.variant()).toBe('error');
  });

  it('should not render when message is null', () => {
    fixture.componentRef.setInput('message', null);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
  });

  it('should render when message is set', () => {
    fixture.componentRef.setInput('message', 'Something went wrong');
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeTruthy();
    expect(fixture.nativeElement.textContent).toContain('Something went wrong');
  });

  it('should render on the shared glass surface', () => {
    fixture.componentRef.setInput('message', 'Something went wrong');
    fixture.detectChanges();
    const pill = fixture.nativeElement.querySelector('.glass-surface');
    expect(pill).not.toBeNull();
  });

  it('should emit dismissed when dismiss button is clicked', () => {
    vi.useFakeTimers({ toFake: ['setTimeout', 'clearTimeout'] });
    fixture.componentRef.setInput('message', 'Error occurred');
    fixture.detectChanges();
    let dismissed = false;
    component.dismissed.subscribe(() => (dismissed = true));
    fixture.nativeElement.querySelector('button[aria-label="Dismiss"]').click();
    expect(dismissed).toBe(true);
    // Timer must be cleared — advancing 7s should not emit again
    dismissed = false;
    vi.advanceTimersByTime(7000);
    expect(dismissed).toBe(false);
  });

  it('should auto-dismiss after 7 seconds', () => {
    vi.useFakeTimers({ toFake: ['setTimeout', 'clearTimeout'] });
    fixture.componentRef.setInput('message', 'Auto dismiss me');
    fixture.detectChanges();
    let dismissed = false;
    component.dismissed.subscribe(() => (dismissed = true));
    vi.advanceTimersByTime(7000);
    expect(dismissed).toBe(true);
  });

  it('should apply success variant classes', () => {
    fixture.componentRef.setInput('message', 'Done');
    fixture.componentRef.setInput('variant', 'success');
    fixture.detectChanges();
    const container = fixture.nativeElement.querySelector('.glass-surface');
    expect(container.classList).toContain('text-success-text');
    expect(container.classList).toContain('bg-success/10');
  });

  it('should apply warning variant classes', () => {
    fixture.componentRef.setInput('message', 'Warning');
    fixture.componentRef.setInput('variant', 'warning');
    fixture.detectChanges();
    const container = fixture.nativeElement.querySelector('.glass-surface');
    expect(container.classList).toContain('text-warning-text');
    expect(container.classList).toContain('bg-warning/10');
  });

  it('should apply info variant classes', () => {
    fixture.componentRef.setInput('message', 'Info');
    fixture.componentRef.setInput('variant', 'info');
    fixture.detectChanges();
    const container = fixture.nativeElement.querySelector('.glass-surface');
    expect(container.classList).toContain('text-info-text');
    expect(container.classList).toContain('bg-info/10');
  });

  it('should apply error variant classes by default', () => {
    fixture.componentRef.setInput('message', 'Error');
    fixture.detectChanges();
    const container = fixture.nativeElement.querySelector('.glass-surface');
    expect(container.classList).toContain('text-destructive-text');
    expect(container.classList).toContain('bg-destructive/10');
  });

  // --- ARIA role / live region per variant ---

  it('should use role="status" and aria-live="polite" for the success variant', () => {
    fixture.componentRef.setInput('message', 'Done');
    fixture.componentRef.setInput('variant', 'success');
    fixture.detectChanges();
    const container = fixture.nativeElement.querySelector('.glass-surface');
    expect(container.getAttribute('role')).toBe('status');
    expect(container.getAttribute('aria-live')).toBe('polite');
  });

  it('should use role="status" and aria-live="polite" for the info variant', () => {
    fixture.componentRef.setInput('message', 'Info');
    fixture.componentRef.setInput('variant', 'info');
    fixture.detectChanges();
    const container = fixture.nativeElement.querySelector('.glass-surface');
    expect(container.getAttribute('role')).toBe('status');
    expect(container.getAttribute('aria-live')).toBe('polite');
  });

  it('should use role="alert" and aria-live="assertive" for the error variant', () => {
    fixture.componentRef.setInput('message', 'Error');
    fixture.componentRef.setInput('variant', 'error');
    fixture.detectChanges();
    const container = fixture.nativeElement.querySelector('.glass-surface');
    expect(container.getAttribute('role')).toBe('alert');
    expect(container.getAttribute('aria-live')).toBe('assertive');
  });

  it('should use role="alert" and aria-live="assertive" for the warning variant', () => {
    fixture.componentRef.setInput('message', 'Warning');
    fixture.componentRef.setInput('variant', 'warning');
    fixture.detectChanges();
    const container = fixture.nativeElement.querySelector('.glass-surface');
    expect(container.getAttribute('role')).toBe('alert');
    expect(container.getAttribute('aria-live')).toBe('assertive');
  });

  // --- Accessibility ---

  it('should have no AXE violations', async () => {
    fixture.componentRef.setInput('message', 'Something went wrong');
    fixture.detectChanges();
    await fixture.whenStable();
    await expectNoAxeViolations(fixture);
  }, 20000);

  it('should have no AXE violations in dark mode', async () => {
    document.documentElement.classList.add('dark');
    try {
      fixture.componentRef.setInput('message', 'Something went wrong');
      fixture.detectChanges();
      await fixture.whenStable();
      await expectNoAxeViolations(fixture);
    } finally {
      document.documentElement.classList.remove('dark');
    }
  }, 20000);
});
