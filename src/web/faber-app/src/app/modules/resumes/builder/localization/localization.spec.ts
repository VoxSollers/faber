import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { LUCIDE_ICONS, LucideIconProvider, Globe, CircleCheck } from 'lucide-angular';
import { Localization } from './localization';
import { ResumesStore } from '../../resumes-store';
import { mockResumesStore } from '../../../../shared/testing/mock-resumes-store';
import { expectNoAxeViolations } from '../../../../shared/testing/axe';

describe('Localization', () => {
  let fixture: ComponentFixture<Localization>;

  const localization = signal('en-us');
  const updateLocalization = vi.fn();

  beforeEach(async () => {
    vi.clearAllMocks();
    localization.set('en-us');

    await TestBed.configureTestingModule({
      imports: [Localization],
      providers: [
        {
          provide: ResumesStore,
          useValue: { ...mockResumesStore(), localization, updateLocalization },
        },
        {
          provide: LUCIDE_ICONS,
          multi: true,
          useValue: new LucideIconProvider({ Globe, CircleCheck }),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Localization);
    await fixture.whenStable();
    fixture.detectChanges();
  });

  function trigger(): HTMLButtonElement {
    return fixture.nativeElement.querySelector('[aria-haspopup="menu"]') as HTMLButtonElement;
  }

  function options(): HTMLButtonElement[] {
    return Array.from(fixture.nativeElement.querySelectorAll('[role="menuitemradio"]'));
  }

  it('should create', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('toggles the language menu open on trigger click', () => {
    expect(trigger().getAttribute('aria-expanded')).toBe('false');

    trigger().click();
    fixture.detectChanges();

    expect(trigger().getAttribute('aria-expanded')).toBe('true');
    expect(options().length).toBe(2);
  });

  it('marks the active language from the store', () => {
    trigger().click();
    fixture.detectChanges();

    const checked = options().filter(o => o.getAttribute('aria-checked') === 'true');
    expect(checked.length).toBe(1);
    expect(checked[0].textContent).toContain('English');
  });

  it('calls updateLocalization and closes when a language is selected', () => {
    trigger().click();
    fixture.detectChanges();

    const polish = options().find(o => o.textContent?.includes('Polish'))!;
    polish.click();
    fixture.detectChanges();

    expect(updateLocalization).toHaveBeenCalledWith('pl-pl');
    expect(trigger().getAttribute('aria-expanded')).toBe('false');
  });

  it('renders the language menu on the shared glass surface', () => {
    const menu = fixture.nativeElement.querySelector('.lang-menu');
    expect(menu.classList.contains('glass-surface')).toBe(true);
  });

  it('has no AXE violations', async () => {
    await expectNoAxeViolations(fixture);
  }, 20_000);

  describe('placement (#425)', () => {
    it('opens leftwards by default', () => {
      fixture.detectChanges();
      const menu = fixture.nativeElement.querySelector('.lang-menu') as HTMLElement;
      expect(menu.classList.contains('lang-menu--left')).toBe(true);
      expect(menu.classList.contains('lang-menu--top')).toBe(false);
    });

    it('opens upwards when placed for the mobile bottom bar', () => {
      fixture.componentRef.setInput('placement', 'top');
      fixture.detectChanges();
      const menu = fixture.nativeElement.querySelector('.lang-menu') as HTMLElement;
      expect(menu.classList.contains('lang-menu--top')).toBe(true);
      expect(menu.classList.contains('lang-menu--left')).toBe(false);
    });

    it('gives the trigger a 44px touch target below md', () => {
      fixture.detectChanges();
      const trigger = fixture.nativeElement.querySelector('button[aria-haspopup="menu"]') as HTMLElement;
      expect(trigger.classList.contains('max-md:size-11')).toBe(true);
    });
  });
});
