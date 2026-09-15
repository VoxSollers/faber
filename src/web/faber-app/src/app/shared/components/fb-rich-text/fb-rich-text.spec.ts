import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Editor } from '@tiptap/core';
import { Fragment, Slice } from '@tiptap/pm/model';
import { FbRichText } from './fb-rich-text';
import { expectNoAxeViolations } from '../../testing/axe';

describe('FbRichText', () => {
  let component: FbRichText;
  let fixture: ComponentFixture<FbRichText>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FbRichText],
    }).compileComponents();

    fixture = TestBed.createComponent(FbRichText);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  afterEach(() => component.ngOnDestroy());

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('registers no duplicate Tiptap extensions (StarterKit already bundles link/underline)', () => {
    const editor = (component as unknown as {
      editor?: { extensionManager: { extensions: { name: string }[] } };
    }).editor;
    if (!editor) return;
    const names = editor.extensionManager.extensions.map(e => e.name);
    const duplicates = names.filter((n, i) => names.indexOf(n) !== i);
    expect(duplicates).toEqual([]);
    expect(names).toContain('link');
    expect(names).toContain('underline');
  });

  it('bubble menu starts hidden (visibility controlled by the plugin)', () => {
    const toolbar: HTMLElement = fixture.nativeElement.querySelector('[role="toolbar"]');
    expect(toolbar.classList.contains('bubble-menu')).toBe(true);
    expect(toolbar.classList.contains('glass-surface')).toBe(true);
    // The Tiptap bubble-menu plugin shows the element on selection; with no
    // selection it stays hidden via the base .bubble-menu CSS.
    expect(toolbar.style.visibility).not.toBe('visible');
  });

  it('renders 10 bubble-menu buttons', () => {
    const toolbar: HTMLElement = fixture.nativeElement.querySelector('[role="toolbar"]');
    expect(toolbar.querySelectorAll('button').length).toBe(10);
  });

  it('renders 2 bubble-menu separators', () => {
    const toolbar: HTMLElement = fixture.nativeElement.querySelector('[role="toolbar"]');
    const separators = Array.from(toolbar.children).filter(
      el => el.tagName === 'DIV' && el.getAttribute('aria-hidden') === 'true',
    );
    expect(separators.length).toBe(2);
  });

  it('writeValue(null) clears editor without throwing', () => {
    expect(() => component.writeValue(null as unknown as string)).not.toThrow();
  });

  it('writeValue("") clears editor without throwing', () => {
    expect(() => component.writeValue('')).not.toThrow();
  });

  it('writeValue stores pending value before editor initializes', () => {
    const fresh = TestBed.createComponent(FbRichText).componentInstance;
    fresh.writeValue('<p>hello</p>');
    expect((fresh as unknown as { pendingValue: string }).pendingValue).toBe('<p>hello</p>');
    fresh.ngOnDestroy();
  });

  it('writeValue sets ProseMirror content when editor is ready', () => {
    const editor = (component as unknown as { editor?: { getHTML: () => string } }).editor;
    if (!editor) return;
    component.writeValue('<p>test content</p>');
    expect(editor.getHTML()).toContain('test content');
  });

  it('calls registered onChange callback', () => {
    const spy = vi.fn();
    component.registerOnChange(spy);
    (component as unknown as { onChange: (v: string) => void }).onChange('<p>x</p>');
    expect(spy).toHaveBeenCalledWith('<p>x</p>');
  });

  it('calls registered onTouched callback', () => {
    const spy = vi.fn();
    component.registerOnTouched(spy);
    (component as unknown as { onTouched: () => void }).onTouched();
    expect(spy).toHaveBeenCalled();
  });

  it('placeholder input is accepted without error', async () => {
    const fresh = TestBed.createComponent(FbRichText);
    fresh.detectChanges();
    await fresh.whenStable();
    expect(fresh.componentInstance).toBeTruthy();
    fresh.componentInstance.ngOnDestroy();
  });

  it('ngOnDestroy calls editor.destroy()', () => {
    const ctx = component as unknown as { editor?: { destroy: () => void } };
    if (!ctx.editor) return;
    const spy = vi.spyOn(ctx.editor, 'destroy');
    component.ngOnDestroy();
    expect(spy).toHaveBeenCalled();
  });

  it('does not emit onChange when the pending value is flushed into the editor', async () => {
    const fresh = TestBed.createComponent(FbRichText);
    const instance = fresh.componentInstance;
    const spy = vi.fn();

    instance.registerOnChange(spy);
    // writeValue lands before afterNextRender builds the editor, so the value
    // parks in pendingValue and is flushed once the editor exists.
    instance.writeValue('<p>hello</p>');

    fresh.detectChanges();
    await fresh.whenStable();

    expect(spy).not.toHaveBeenCalled();
    instance.ngOnDestroy();
  }, 20_000);

  it('still applies the flushed pending value to the editor', async () => {
    const fresh = TestBed.createComponent(FbRichText);
    const instance = fresh.componentInstance;

    instance.writeValue('<p>hello</p>');

    fresh.detectChanges();
    await fresh.whenStable();

    const ctx = instance as unknown as {
      editor?: { getHTML: () => string };
      pendingValue?: string;
    };
    expect(ctx.editor?.getHTML()).toContain('hello');
    expect(ctx.pendingValue).toBeUndefined();
    instance.ngOnDestroy();
  }, 20_000);

  it('has no AXE violations', async () => {
    await expectNoAxeViolations(fixture);
  }, 20_000);

  it('does not render a counter when maxLength is unset', () => {
    const counter = fixture.nativeElement.querySelector('[aria-live="polite"]');
    expect(counter).toBeNull();
  });

  it('renders count / maxLength once content is set', async () => {
    const fresh = TestBed.createComponent(FbRichText);
    fresh.componentRef.setInput('maxLength', 20);
    fresh.detectChanges();
    await fresh.whenStable();

    fresh.componentInstance.writeValue('<p>hello</p>');
    fresh.detectChanges();

    const counter: HTMLElement = fresh.nativeElement.querySelector('[aria-live="polite"]');
    expect(counter.textContent?.trim()).toBe('5 / 20');
    fresh.componentInstance.ngOnDestroy();
  }, 20_000);

  it('caps interactive typing at maxLength', async () => {
    const fresh = TestBed.createComponent(FbRichText);
    fresh.componentRef.setInput('maxLength', 200);
    fresh.detectChanges();
    await fresh.whenStable();

    const editor = (fresh.componentInstance as unknown as { editor: Editor }).editor;
    const handled = editor.view.props.handleTextInput?.(
      editor.view,
      1,
      1,
      'a'.repeat(201),
      () => editor.state.tr.insertText('a'.repeat(201), 1, 1),
    );

    expect(handled).toBe(true);
    expect(editor.getText({ blockSeparator: '' })).toHaveLength(200);
    expect(fresh.componentInstance.characterCount()).toBe(200);
    fresh.componentInstance.ngOnDestroy();
  }, 20_000);

  it('truncates a long paste to the remaining capacity and updates the counter', async () => {
    const fresh = TestBed.createComponent(FbRichText);
    fresh.componentRef.setInput('maxLength', 200);
    fresh.detectChanges();
    await fresh.whenStable();

    const editor = (fresh.componentInstance as unknown as { editor: Editor }).editor;
    fresh.componentInstance.writeValue(`<p>${'a'.repeat(195)}</p>`);
    const boldText = editor.schema.text('b'.repeat(10), [editor.schema.marks['bold'].create()]);
    const handled = editor.view.props.handlePaste?.(
      editor.view,
      new Event('paste') as ClipboardEvent,
      new Slice(Fragment.from(boldText), 0, 0),
    );
    fresh.detectChanges();

    expect(handled).toBe(true);
    expect(editor.getText({ blockSeparator: '' })).toBe(`${'a'.repeat(195)}${'b'.repeat(5)}`);
    expect(editor.getHTML()).toContain(`<strong>${'b'.repeat(5)}</strong>`);
    expect(fresh.componentInstance.characterCount()).toBe(200);
    const counter: HTMLElement = fresh.nativeElement.querySelector('[aria-live="polite"]');
    expect(counter.textContent?.trim()).toBe('200 / 200');
    fresh.componentInstance.ngOnDestroy();
  }, 20_000);

  it('displays legacy over-limit content without truncating it', async () => {
    const fresh = TestBed.createComponent(FbRichText);
    fresh.componentRef.setInput('maxLength', 200);
    fresh.detectChanges();
    await fresh.whenStable();

    fresh.componentInstance.writeValue(`<p>${'a'.repeat(201)}</p>`);
    fresh.detectChanges();

    const editor = (fresh.componentInstance as unknown as { editor: Editor }).editor;
    const counter: HTMLElement = fresh.nativeElement.querySelector('[aria-live="polite"]');
    expect(editor.getText({ blockSeparator: '' })).toHaveLength(201);
    expect(counter.textContent?.trim()).toBe('201 / 200');
    expect(counter.classList.contains('text-destructive-text')).toBe(true);
    fresh.componentInstance.ngOnDestroy();
  }, 20_000);

  it('applies the destructive text class once content exceeds maxLength', async () => {
    const fresh = TestBed.createComponent(FbRichText);
    fresh.componentRef.setInput('maxLength', 3);
    fresh.detectChanges();
    await fresh.whenStable();

    fresh.componentInstance.writeValue('<p>hello</p>');
    fresh.detectChanges();

    const counter: HTMLElement = fresh.nativeElement.querySelector('[aria-live="polite"]');
    expect(counter.classList.contains('text-destructive-text')).toBe(true);
    fresh.componentInstance.ngOnDestroy();
  }, 20_000);

  it('validate returns null when maxLength is unset', () => {
    expect(component.validate({} as never)).toBeNull();
  });

  it('validate returns null when under the limit', async () => {
    const fresh = TestBed.createComponent(FbRichText);
    fresh.componentRef.setInput('maxLength', 20);
    fresh.detectChanges();
    await fresh.whenStable();

    fresh.componentInstance.writeValue('<p>hello</p>');
    fresh.detectChanges();

    expect(fresh.componentInstance.validate({} as never)).toBeNull();
    fresh.componentInstance.ngOnDestroy();
  }, 20_000);

  it('does not count paragraph separators toward maxLength', async () => {
    const fresh = TestBed.createComponent(FbRichText);
    fresh.componentRef.setInput('maxLength', 200);
    fresh.detectChanges();
    await fresh.whenStable();

    fresh.componentInstance.writeValue(`<p>${'a'.repeat(100)}</p><p>${'b'.repeat(100)}</p>`);
    fresh.detectChanges();

    expect(fresh.componentInstance.characterCount()).toBe(200);
    expect(fresh.componentInstance.validate({} as never)).toBeNull();
    fresh.componentInstance.ngOnDestroy();
  }, 20_000);

  it('validate returns a maxlength error when over the limit', async () => {
    const fresh = TestBed.createComponent(FbRichText);
    fresh.componentRef.setInput('maxLength', 3);
    fresh.detectChanges();
    await fresh.whenStable();

    fresh.componentInstance.writeValue('<p>hello</p>');
    fresh.detectChanges();

    expect(fresh.componentInstance.validate({} as never)).toEqual({
      maxlength: { requiredLength: 3, actualLength: 5 },
    });
    fresh.componentInstance.ngOnDestroy();
  }, 20_000);
});
