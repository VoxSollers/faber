import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ResumeCard } from './resume-card';
import { Resume } from '../../resume-response';

const mockResume = {
  id: '1',
  person: { firstname: 'John', lastname: 'Doe', jobTitle: 'Engineer' },
  title: 'John Doe Resume',
  createdAt: '2026-01-15T00:00:00.000Z',
} as unknown as Resume;

const mockUntitledResume = {
  id: '2',
  person: null,
  title: null,
  createdAt: '2026-01-15T00:00:00.000Z',
} as unknown as Resume;

describe('ResumeCard', () => {
  let component: ResumeCard;
  let fixture: ComponentFixture<ResumeCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ResumeCard],
    }).compileComponents();

    fixture = TestBed.createComponent(ResumeCard);
    fixture.componentRef.setInput('resume', mockResume);
    fixture.detectChanges();
    await fixture.whenStable();
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should emit open when Edit button is clicked', () => {
    const spy = vi.fn();
    component.open.subscribe(spy);
    const editBtn = fixture.nativeElement.querySelector('[data-testid="edit-btn"]') as HTMLButtonElement;
    editBtn.click();
    expect(spy).toHaveBeenCalledWith('1');
  });

  it('should emit open when card is clicked', () => {
    const spy = vi.fn();
    component.open.subscribe(spy);
    const card = fixture.nativeElement.querySelector('[role="button"]') as HTMLElement;
    card.click();
    expect(spy).toHaveBeenCalledWith('1');
  });

  it('should emit delete when Delete button is clicked', () => {
    const spy = vi.fn();
    component.delete.subscribe(spy);
    const deleteBtn = fixture.nativeElement.querySelector('[data-testid="delete-btn"]') as HTMLButtonElement;
    deleteBtn.click();
    expect(spy).toHaveBeenCalledWith('1');
  });

  it('should not propagate click on Edit button to card', () => {
    const cardSpy = vi.fn();
    component.open.subscribe(cardSpy);
    const editBtn = fixture.nativeElement.querySelector('[data-testid="edit-btn"]') as HTMLButtonElement;
    // Clicking the button emits once (from button handler), not twice
    editBtn.click();
    expect(cardSpy).toHaveBeenCalledTimes(1);
  });

  it('should not propagate click on Delete button to card', () => {
    const openSpy = vi.fn();
    component.open.subscribe(openSpy);
    const deleteBtn = fixture.nativeElement.querySelector('[data-testid="delete-btn"]') as HTMLButtonElement;
    deleteBtn.click();
    expect(openSpy).not.toHaveBeenCalled();
  });

  it('renders as a bordered card surface (#369 — parity with account card)', () => {
    const root = fixture.nativeElement.querySelector('[role="button"]') as HTMLElement;
    expect(root.className).toContain('bg-card');
    expect(root.className).toContain('border-line');
  });

  it('does not render the person job title (#478)', () => {
    expect((fixture.nativeElement.textContent ?? '')).not.toContain('Engineer');
  });

  describe('inline title editing', () => {
    it('emits titleChange with the new value when the title is edited and committed', () => {
      const spy = vi.fn();
      component.titleChange.subscribe(spy);

      const titleBtn = fixture.nativeElement.querySelector('[data-testid="title-edit-btn"]') as HTMLButtonElement;
      titleBtn.click();
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input[aria-label="Resume title"]') as HTMLInputElement;
      input.value = 'Updated Title';
      input.dispatchEvent(new Event('input'));
      input.dispatchEvent(new Event('blur'));

      expect(spy).toHaveBeenCalledWith({ id: '1', title: 'Updated Title' });
    });

    it('does not emit titleChange when opening the editor on an untitled card and blurring without changes', async () => {
      fixture.componentRef.setInput('resume', mockUntitledResume);
      fixture.detectChanges();
      await fixture.whenStable();

      const spy = vi.fn();
      component.titleChange.subscribe(spy);

      const titleBtn = fixture.nativeElement.querySelector('[data-testid="title-edit-btn"]') as HTMLButtonElement;
      titleBtn.click();
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input[aria-label="Resume title"]') as HTMLInputElement;
      input.dispatchEvent(new Event('blur'));

      expect(spy).not.toHaveBeenCalled();
    });

    it('emits an empty title when a titled resume is cleared', () => {
      const spy = vi.fn();
      component.titleChange.subscribe(spy);

      const titleBtn = fixture.nativeElement.querySelector('[data-testid="title-edit-btn"]') as HTMLButtonElement;
      titleBtn.click();
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input[aria-label="Resume title"]') as HTMLInputElement;
      input.value = '   ';
      input.dispatchEvent(new Event('blur'));

      expect(spy).toHaveBeenCalledWith({ id: '1', title: '' });
    });

    it('does not emit open when clicking to edit the title', () => {
      const openSpy = vi.fn();
      component.open.subscribe(openSpy);

      const titleBtn = fixture.nativeElement.querySelector('[data-testid="title-edit-btn"]') as HTMLButtonElement;
      titleBtn.click();

      expect(openSpy).not.toHaveBeenCalled();
    });

    it('focuses the title input when editing starts', () => {
      const titleBtn = fixture.nativeElement.querySelector('[data-testid="title-edit-btn"]') as HTMLButtonElement;
      titleBtn.click();
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input[aria-label="Resume title"]') as HTMLInputElement;
      expect(document.activeElement).toBe(input);
    });

    it('does not emit open when pressing space while editing the title', () => {
      const openSpy = vi.fn();
      component.open.subscribe(openSpy);

      const titleBtn = fixture.nativeElement.querySelector('[data-testid="title-edit-btn"]') as HTMLButtonElement;
      titleBtn.click();
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input[aria-label="Resume title"]') as HTMLInputElement;
      input.dispatchEvent(new KeyboardEvent('keydown', { key: ' ', bubbles: true }));

      expect(openSpy).not.toHaveBeenCalled();
    });

    it('does not emit open a second time when pressing Enter to commit the title', () => {
      const openSpy = vi.fn();
      component.open.subscribe(openSpy);

      const titleBtn = fixture.nativeElement.querySelector('[data-testid="title-edit-btn"]') as HTMLButtonElement;
      titleBtn.click();
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input[aria-label="Resume title"]') as HTMLInputElement;
      input.value = 'New Title';
      input.dispatchEvent(new Event('input'));
      input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', bubbles: true }));

      expect(openSpy).not.toHaveBeenCalled();
    });

    it('sizes the editor from a mirror of its own text so the card cannot jump', () => {
      const titleBtn = fixture.nativeElement.querySelector('[data-testid="title-edit-btn"]') as HTMLButtonElement;
      titleBtn.click();
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input[aria-label="Resume title"]') as HTMLInputElement;
      const sizer = fixture.nativeElement.querySelector('[data-testid="title-sizer"]') as HTMLElement;

      expect(input.getAttribute('size')).toBe('1');
      expect(sizer.textContent?.trim()).toBe('John Doe Resume');

      input.value = 'A much longer resume title';
      input.dispatchEvent(new Event('input'));
      fixture.detectChanges();

      expect(sizer.textContent?.trim()).toBe('A much longer resume title');
    });

    it('falls back to the placeholder text in the sizer so an empty title keeps a box', () => {
      const titleBtn = fixture.nativeElement.querySelector('[data-testid="title-edit-btn"]') as HTMLButtonElement;
      titleBtn.click();
      fixture.detectChanges();

      const input = fixture.nativeElement.querySelector('input[aria-label="Resume title"]') as HTMLInputElement;
      input.value = '';
      input.dispatchEvent(new Event('input'));
      fixture.detectChanges();

      const sizer = fixture.nativeElement.querySelector('[data-testid="title-sizer"]') as HTMLElement;
      expect(sizer.textContent?.trim()).toBe('Untitled Resume');
    });

    it('does not start editing when the title text itself is clicked', () => {
      const titleText = fixture.nativeElement.querySelector('span.truncate') as HTMLElement;
      titleText.click();
      fixture.detectChanges();

      expect(fixture.nativeElement.querySelector('input[aria-label="Resume title"]')).toBeNull();
    });

    it('shows the active-state underline only in edit mode', () => {
      expect(fixture.nativeElement.querySelector('[data-testid="title-edit-underline"]')).toBeNull();

      const titleBtn = fixture.nativeElement.querySelector('[data-testid="title-edit-btn"]') as HTMLButtonElement;
      titleBtn.click();
      fixture.detectChanges();

      expect(fixture.nativeElement.querySelector('[data-testid="title-edit-underline"]')).toBeTruthy();
    });
  });
});
