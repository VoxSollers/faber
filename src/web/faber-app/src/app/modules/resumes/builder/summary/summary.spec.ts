import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { By } from '@angular/platform-browser';
import { Summary } from './summary';
import { ResumesStore } from '../../resumes-store';
import { mockResumesStore } from '../../../../shared/testing/mock-resumes-store';
import { FbRichText } from '../../../../shared/components/fb-rich-text/fb-rich-text';

describe('Summary', () => {
  let component: Summary;
  let fixture: ComponentFixture<Summary>;

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [Summary],
      providers: [
        { provide: ResumesStore, useValue: mockResumesStore() },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Summary);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('does not persist the summary when the section is opened with existing content', async () => {
    // The section component is created lazily on expand, so this fixture
    // creation is the expand. Seed non-empty HTML: an empty document produces
    // no ProseMirror transaction and would make this test vacuous.
    TestBed.resetTestingModule();
    vi.useFakeTimers();
    try {
      const store = { ...mockResumesStore(), summary: signal('<p>Existing summary</p>') };
      await TestBed.configureTestingModule({
        imports: [Summary],
        providers: [{ provide: ResumesStore, useValue: store }],
      }).compileComponents();

      const f = TestBed.createComponent(Summary);
      f.detectChanges();
      await f.whenStable();

      // Allow any accidentally emitted asynchronous form event to run.
      vi.advanceTimersByTime(3000);

      expect(store.updateSummary).not.toHaveBeenCalled();
      expect(f.componentInstance['control'].pristine).toBe(true);
    } finally {
      vi.useRealTimers();
    }
  }, 20_000);

  it('does not persist a summary that exceeds the limit through fb-rich-text validation', async () => {
    const store = TestBed.inject(ResumesStore);
    fixture.detectChanges();
    await fixture.whenStable();

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

      expect(store.updateSummary).not.toHaveBeenCalled();
    } finally {
      vi.useRealTimers();
    }
  }, 20_000);
});
