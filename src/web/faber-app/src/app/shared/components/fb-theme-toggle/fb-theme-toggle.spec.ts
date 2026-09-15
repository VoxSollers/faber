import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { FbThemeToggle } from './fb-theme-toggle';
import { ThemeService } from '../../../core/services/theme';
import { expectNoAxeViolations } from '../../testing/axe';

describe('FbThemeToggle', () => {
  let fixture: ComponentFixture<FbThemeToggle>;

  const mockThemeService = {
    isDark: signal(false),
    toggle: vi.fn(),
  };

  beforeEach(async () => {
    mockThemeService.isDark.set(false);
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [FbThemeToggle],
      providers: [{ provide: ThemeService, useValue: mockThemeService }],
    }).compileComponents();

    fixture = TestBed.createComponent(FbThemeToggle);
    fixture.detectChanges();
  });

  function button(): HTMLButtonElement {
    return fixture.nativeElement.querySelector('[data-testid="theme-toggle"]');
  }

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('renders a theme toggle button', () => {
    expect(button()).toBeTruthy();
  });

  it('has aria-label "Switch to dark mode" in light mode', () => {
    mockThemeService.isDark.set(false);
    fixture.detectChanges();
    expect(button().getAttribute('aria-label')).toBe('Switch to dark mode');
  });

  it('has aria-label "Switch to light mode" in dark mode', () => {
    mockThemeService.isDark.set(true);
    fixture.detectChanges();
    expect(button().getAttribute('aria-label')).toBe('Switch to light mode');
  });

  it('calls themeService.toggle() when clicked', () => {
    button().click();
    expect(mockThemeService.toggle).toHaveBeenCalled();
  });

  it('renders exactly one lucide-icon at a time', () => {
    expect(fixture.nativeElement.querySelectorAll('lucide-icon').length).toBe(1);
    mockThemeService.isDark.set(true);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('lucide-icon').length).toBe(1);
  });

  // jsdom can't evaluate colour, so this asserts structure/ARIA only; it also
  // exercises the .dark code path. Real contrast is verified in the browser.
  it('should have no AXE violations (incl. dark mode) in both toggle states', async () => {
    document.documentElement.classList.add('dark');
    try {
      await expectNoAxeViolations(fixture);
      mockThemeService.isDark.set(true);
      fixture.detectChanges();
      await expectNoAxeViolations(fixture);
    } finally {
      document.documentElement.classList.remove('dark');
    }
  }, 20000);
});
