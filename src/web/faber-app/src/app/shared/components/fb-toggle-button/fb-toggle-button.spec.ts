import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FbToggleButton } from './fb-toggle-button';
import { expectNoAxeViolations } from '../../testing/axe';

describe('FbToggleButton', () => {
  let component: FbToggleButton;
  let fixture: ComponentFixture<FbToggleButton>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FbToggleButton],
    }).compileComponents();

    fixture = TestBed.createComponent(FbToggleButton);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should default isOpen to false', () => {
    expect(component.isOpen()).toBe(false);
  });

  it('should reflect isOpen input when set to true', () => {
    fixture.componentRef.setInput('isOpen', true);
    expect(component.isOpen()).toBe(true);
  });

  it('closed state uses a gray surface in dark mode, not near-white foreground (#369)', () => {
    const btn = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    expect(btn.className).toContain('dark:bg-surface');
    expect(btn.className).toContain('dark:text-foreground');
  });

  it('should emit clicked event when onButtonClick is called', () => {
    let emitted = false;
    component.clicked.subscribe(() => (emitted = true));

    component.onButtonClick();

    expect(emitted).toBe(true);
  });

  // jsdom can't evaluate colour, so this asserts structure/ARIA only; it also
  // exercises the .dark code path. Real contrast is verified in the browser.
  it('should have no AXE violations (incl. dark mode) in both states', async () => {
    document.documentElement.classList.add('dark');
    try {
      await expectNoAxeViolations(fixture);
      fixture.componentRef.setInput('isOpen', true);
      fixture.detectChanges();
      await expectNoAxeViolations(fixture);
    } finally {
      document.documentElement.classList.remove('dark');
    }
  }, 20000);
});
