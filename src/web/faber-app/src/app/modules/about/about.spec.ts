import { TestBed } from '@angular/core/testing';
import { About } from './about';

describe('About', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [About],
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(About);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('GitHub button uses theme-aware tokens, not hardcoded grays (#370)', async () => {
    const fixture = TestBed.createComponent(About);
    await fixture.whenStable();
    const github = (fixture.nativeElement as HTMLElement).querySelector<HTMLAnchorElement>(
      'a[href*="github.com"]',
    );
    expect(github).not.toBeNull();
    expect(github!.className).toContain('bg-foreground');
    expect(github!.className).toContain('text-background');
    expect(github!.className).not.toContain('bg-gray-900');
    expect(github!.className).not.toContain('text-white');
  });
});
