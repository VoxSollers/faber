import { TestBed } from '@angular/core/testing';
import { ThemeService } from './theme';

function mockMatchMedia(prefersDark: boolean): void {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    value: vi.fn().mockImplementation((query: string) => ({
      matches: query === '(prefers-color-scheme: dark)' ? prefersDark : false,
      media: query,
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })),
  });
}

describe('ThemeService', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.classList.remove('dark');
    mockMatchMedia(false);
    TestBed.configureTestingModule({});
  });

  afterEach(() => {
    localStorage.clear();
    document.documentElement.classList.remove('dark');
  });

  describe('initial state — no stored preference', () => {
    it('defaults to light regardless of system preference (light)', () => {
      mockMatchMedia(false);
      expect(TestBed.inject(ThemeService).isDark()).toBe(false);
    });

    it('defaults to light regardless of system preference (dark)', () => {
      mockMatchMedia(true);
      expect(TestBed.inject(ThemeService).isDark()).toBe(false);
    });

    it('does not apply .dark class regardless of system preference', () => {
      mockMatchMedia(true);
      TestBed.inject(ThemeService);
      TestBed.flushEffects();
      expect(document.documentElement.classList.contains('dark')).toBe(false);
    });

    it('does not apply .dark class when system prefers light', () => {
      mockMatchMedia(false);
      TestBed.inject(ThemeService);
      TestBed.flushEffects();
      expect(document.documentElement.classList.contains('dark')).toBe(false);
    });
  });

  describe('initial state — stored preference', () => {
    it('reads dark from localStorage regardless of system preference', () => {
      localStorage.setItem('faber-theme', 'dark');
      mockMatchMedia(false);
      expect(TestBed.inject(ThemeService).isDark()).toBe(true);
    });

    it('reads light from localStorage regardless of system preference', () => {
      localStorage.setItem('faber-theme', 'light');
      mockMatchMedia(true);
      expect(TestBed.inject(ThemeService).isDark()).toBe(false);
    });
  });

  describe('toggle()', () => {
    let service: ThemeService;

    beforeEach(() => {
      service = TestBed.inject(ThemeService);
    });

    it('flips isDark from false to true', () => {
      service.toggle();
      expect(service.isDark()).toBe(true);
    });

    it('flips isDark from true to false on second call', () => {
      service.toggle();
      service.toggle();
      expect(service.isDark()).toBe(false);
    });

    it('adds .dark class when toggling to dark', () => {
      service.toggle();
      TestBed.flushEffects();
      expect(document.documentElement.classList.contains('dark')).toBe(true);
    });

    it('removes .dark class when toggling back to light', () => {
      service.toggle();
      service.toggle();
      TestBed.flushEffects();
      expect(document.documentElement.classList.contains('dark')).toBe(false);
    });

    it('persists "dark" to localStorage', () => {
      service.toggle();
      expect(localStorage.getItem('faber-theme')).toBe('dark');
    });

    it('persists "light" to localStorage on second toggle', () => {
      service.toggle();
      service.toggle();
      expect(localStorage.getItem('faber-theme')).toBe('light');
    });
  });

  it('exposes isDark as a readonly signal (no set method)', () => {
    const service = TestBed.inject(ThemeService);
    expect(typeof (service.isDark as unknown as Record<string, unknown>)['set']).toBe('undefined');
  });
});
