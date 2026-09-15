import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal, WritableSignal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { Hobby } from './hobby';
import { ResumesStore } from '../../resumes-store';
import { mockResumesStore } from '../../../../shared/testing/mock-resumes-store';
import { expectNoAxeViolations } from '../../../../shared/testing/axe';
import { FbRichText } from '../../../../shared/components/fb-rich-text/fb-rich-text';

describe('Hobby', () => {
  let fixture: ComponentFixture<Hobby>;
  let component: Hobby;
  let store: Partial<ResumesStore>;
  let hobbies: WritableSignal<string>;

  beforeEach(async () => {
    vi.clearAllMocks();
    hobbies = signal('');
    store = { ...mockResumesStore(), hobbies };

    await TestBed.configureTestingModule({
      imports: [Hobby],
      providers: [{ provide: ResumesStore, useValue: store }],
    }).compileComponents();

    fixture = TestBed.createComponent(Hobby);
    component = fixture.componentInstance;
    await fixture.whenStable();
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should render the fb-rich-text editor', () => {
    expect(fixture.nativeElement.querySelector('fb-rich-text')).toBeTruthy();
  });

  it('should sync the store value into the control', async () => {
    hobbies.set('<p>Reading</p>');
    fixture.detectChanges();
    await fixture.whenStable();
    expect(component['control'].value).toBe('<p>Reading</p>');
  });

  it('forwards hobbies immediately so the store owns the debounce lifecycle', () => {
    component['control'].setValue('<p>Hiking</p>');
    expect(store.updateHobbies).toHaveBeenCalledWith('<p>Hiking</p>');
  });

  it('does not persist hobbies that exceed the limit through fb-rich-text validation', async () => {
    const richText = fixture.debugElement.query(By.directive(FbRichText)).componentInstance as FbRichText;
    const editor = (richText as unknown as {
      editor: { commands: { setContent: (content: string) => boolean } };
    }).editor;

    vi.useFakeTimers();
    try {
      editor.commands.setContent(`<p>${'a'.repeat(201)}</p>`);
      fixture.detectChanges();

      expect(component['control'].errors).toEqual({
        maxlength: { requiredLength: 200, actualLength: 201 },
      });

      vi.advanceTimersByTime(2000);

      expect(store.updateHobbies).not.toHaveBeenCalled();
    } finally {
      vi.useRealTimers();
    }
  }, 20_000);

  it('does not persist hobbies when the section is opened with existing content', async () => {
    TestBed.resetTestingModule();
    vi.useFakeTimers();
    try {
      const isolated = { ...mockResumesStore(), hobbies: signal('<p>Reading</p>') };
      await TestBed.configureTestingModule({
        imports: [Hobby],
        providers: [{ provide: ResumesStore, useValue: isolated }],
      }).compileComponents();

      const f = TestBed.createComponent(Hobby);
      f.detectChanges();
      await f.whenStable();

      vi.advanceTimersByTime(3000);

      expect(isolated.updateHobbies).not.toHaveBeenCalled();
      expect(f.componentInstance['control'].pristine).toBe(true);
    } finally {
      vi.useRealTimers();
    }
  }, 20_000);

  it('should have no AXE violations', async () => {
    await expectNoAxeViolations(fixture);
  }, 20000);
});
