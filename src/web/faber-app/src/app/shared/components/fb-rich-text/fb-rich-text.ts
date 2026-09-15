import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  forwardRef,
  inject,
  input,
  NgZone,
  OnDestroy,
  signal,
  viewChild,
} from '@angular/core';
import {
  AbstractControl,
  ControlValueAccessor,
  NG_VALIDATORS,
  NG_VALUE_ACCESSOR,
  ValidationErrors,
  Validator,
} from '@angular/forms';
import { Editor } from '@tiptap/core';
import StarterKit from '@tiptap/starter-kit';
import Placeholder from '@tiptap/extension-placeholder';
import TextAlign from '@tiptap/extension-text-align';
import { BubbleMenuPlugin } from '@tiptap/extension-bubble-menu';
import { Fragment, Node as ProseMirrorNode, Slice } from '@tiptap/pm/model';
import {
  Bold,
  Italic,
  List,
  ListOrdered,
  LUCIDE_ICONS,
  LucideAngularModule,
  LucideIconProvider,
  Strikethrough,
  TextAlignCenter,
  TextAlignEnd,
  TextAlignJustify,
  TextAlignStart,
  Underline as UnderlineIcon,
} from 'lucide-angular';

@Component({
  selector: 'fb-rich-text',
  templateUrl: './fb-rich-text.html',
  styleUrl: './fb-rich-text.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [LucideAngularModule],
  host: { class: 'block' },
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => FbRichText),
      multi: true,
    },
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => FbRichText),
      multi: true,
    },
    {
      provide: LUCIDE_ICONS,
      multi: true,
      useValue: new LucideIconProvider({
        Bold,
        Italic,
        List,
        ListOrdered,
        Strikethrough,
        TextAlignCenter,
        TextAlignEnd,
        TextAlignJustify,
        TextAlignStart,
        Underline: UnderlineIcon,
      }),
    },
  ],
})
export class FbRichText implements ControlValueAccessor, Validator, OnDestroy {
  readonly placeholder = input('');
  readonly maxLength = input<number | undefined>(undefined);

  private readonly editorEl = viewChild.required<ElementRef>('editorEl');
  private readonly bubbleEl = viewChild.required<ElementRef<HTMLElement>>('bubbleEl');
  private readonly zone = inject(NgZone);

  private editor?: Editor;
  private pendingValue?: string;

  readonly focused = signal(false);
  readonly characterCount = signal(0);
  readonly activeFormats = signal({
    bold: false,
    italic: false,
    underline: false,
    strike: false,
    bulletList: false,
    orderedList: false,
    link: false,
    alignLeft: false,
    alignCenter: false,
    alignRight: false,
    alignJustify: false,
  });

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  // Find the nearest scrollable ancestor to use as the bubble menu's
  // offset parent. With strategy:'absolute', the bubble and the editor
  // content share the same scroll context — they scroll together natively
  // with zero JS tracking and zero jitter.
  private findScrollParent(el: HTMLElement): HTMLElement {
    let node = el.parentElement;
    while (node && node !== document.body) {
      const { overflow, overflowY } = getComputedStyle(node);
      if (/auto|scroll/.test(overflow + overflowY)) {
        return node;
      }
      node = node.parentElement;
    }
    return document.body;
  }

  constructor() {
    afterNextRender(() => {
      this.editor = new Editor({
        element: this.editorEl().nativeElement,
        extensions: [
          StarterKit.configure({ link: { openOnClick: false } }),
          Placeholder.configure({ placeholder: this.placeholder() }),
          TextAlign.configure({ types: ['heading', 'paragraph'] }),
        ],
        editorProps: {
          attributes: {
            role: 'textbox',
            'aria-multiline': 'true',
            'aria-label': this.placeholder() || 'Rich text editor',
          },
          // On focus / selection change, ProseMirror scrolls the selection into
          // the nearest scrollable ancestor. This editor has no internal scroll
          // (it grows with its content), so that only ever scrolls the outer
          // section-list pane — yanking it to the top when a section near the
          // bottom (e.g. Hobbies) is edited. Suppress PM's own scroll; the bubble
          // menu keeps tracking the selection natively, so its scroll-follow UX
          // is untouched.
          handleScrollToSelection: () => true,
          handleTextInput: (view, from, to, text) => {
            const availableCharacterCount = this.availableCharacterCount(view.state.doc, from, to);
            if (availableCharacterCount === undefined || text.length <= availableCharacterCount) {
              return false;
            }

            if (availableCharacterCount > 0) {
              view.dispatch(view.state.tr.insertText(text.slice(0, availableCharacterCount), from, to));
            }

            return true;
          },
          handlePaste: (view, _event, slice) => {
            const { from, to } = view.state.selection;
            const availableCharacterCount = this.availableCharacterCount(view.state.doc, from, to);
            if (availableCharacterCount === undefined) {
              return false;
            }

            const pastedCharacterCount = slice.content.textBetween(0, slice.content.size, '');
            if (pastedCharacterCount.length <= availableCharacterCount) {
              return false;
            }

            if (availableCharacterCount > 0) {
              view.dispatch(
                view.state.tr.replaceRange(from, to, this.truncateSlice(slice, availableCharacterCount)),
              );
            }

            return true;
          },
        },
        onUpdate: ({ editor }) => {
          this.zone.run(() => {
            this.onChange(editor.getHTML());
            this.updateCharacterCount();
          });
        },
        onFocus: () => {
          this.zone.run(() => this.focused.set(true));
        },
        onBlur: () => {
          this.zone.run(() => {
            this.focused.set(false);
            this.onTouched();
          });
        },
        onTransaction: () => {
          this.zone.run(() => {
            this.updateActiveFormats();
            this.updateCharacterCount();
          });
        },
      });

      // Appended to the nearest scrollable ancestor (strategy:'absolute'),
      // so the bubble lives in the same scroll coordinate space as the editor
      // content. Native scroll keeps them aligned with no JS tracking or jitter.
      // The scroll container has position:relative so it becomes the offset parent.
      const scrollParent = this.findScrollParent(this.editorEl().nativeElement);
      this.editor.registerPlugin(
        BubbleMenuPlugin({
          pluginKey: 'bubbleMenu',
          editor: this.editor,
          element: this.bubbleEl().nativeElement,
          appendTo: () => scrollParent,
          updateDelay: 0,
          shouldShow: ({ editor, state }) => editor.isFocused && !state.selection.empty,
          options: {
            strategy: 'absolute',
            placement: 'top',
            offset: 8,
            flip: true,
            shift: { padding: 8 },
          },
        }),
      );

      // Flushing the parked initial value must not look like a user edit:
      // without emitUpdate:false Tiptap fires onUpdate -> onChange, which marks
      // the control dirty and triggers a debounced save on section expand (#423).
      if (this.pendingValue !== undefined) {
        this.editor.commands.setContent(this.pendingValue, { emitUpdate: false });
        this.pendingValue = undefined;
      }
      this.updateCharacterCount();
    });
  }

  private updateActiveFormats(): void {
    if (!this.editor) return;
    this.activeFormats.set({
      bold: this.editor.isActive('bold'),
      italic: this.editor.isActive('italic'),
      underline: this.editor.isActive('underline'),
      strike: this.editor.isActive('strike'),
      bulletList: this.editor.isActive('bulletList'),
      orderedList: this.editor.isActive('orderedList'),
      link: this.editor.isActive('link'),
      alignLeft: this.editor.isActive({ textAlign: 'left' }),
      alignCenter: this.editor.isActive({ textAlign: 'center' }),
      alignRight: this.editor.isActive({ textAlign: 'right' }),
      alignJustify: this.editor.isActive({ textAlign: 'justify' }),
    });
  }

  private updateCharacterCount(): void {
    if (!this.editor) return;
    this.characterCount.set(this.editor.getText({ blockSeparator: '' }).length);
  }

  private availableCharacterCount(doc: ProseMirrorNode, from: number, to: number): number | undefined {
    const maxLength = this.maxLength();
    if (maxLength === undefined) {
      return undefined;
    }

    const currentCharacterCount = doc.textBetween(0, doc.content.size, '').length;
    const selectedCharacterCount = doc.textBetween(from, to, '').length;
    return Math.max(maxLength - currentCharacterCount + selectedCharacterCount, 0);
  }

  private truncateSlice(slice: Slice, maximumLength: number): Slice {
    let remainingCharacterCount = maximumLength;

    const truncateNode = (node: ProseMirrorNode): ProseMirrorNode | undefined => {
      if (remainingCharacterCount === 0) {
        return undefined;
      }

      if (node.isText) {
        const text = node.text ?? '';
        if (text.length <= remainingCharacterCount) {
          remainingCharacterCount -= text.length;
          return node;
        }

        const truncatedNode = node.cut(0, remainingCharacterCount);
        remainingCharacterCount = 0;
        return truncatedNode;
      }

      if (node.isLeaf) {
        return node;
      }

      const children: ProseMirrorNode[] = [];
      node.forEach(child => {
        const truncatedChild = truncateNode(child);
        if (truncatedChild) {
          children.push(truncatedChild);
        }
      });

      return children.length > 0 ? node.copy(Fragment.from(children)) : undefined;
    };

    const nodes: ProseMirrorNode[] = [];
    slice.content.forEach(node => {
      const truncatedNode = truncateNode(node);
      if (truncatedNode) {
        nodes.push(truncatedNode);
      }
    });

    return new Slice(Fragment.from(nodes), slice.openStart, slice.openEnd);
  }

  writeValue(value: string): void {
    if (this.editor) {
      const html = value ?? '';
      if (html !== this.editor.getHTML()) {
        this.editor.commands.setContent(html, { emitUpdate: false });
        this.updateCharacterCount();
      }
    } else {
      this.pendingValue = value ?? '';
    }
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  validate(control: AbstractControl): ValidationErrors | null {
    void control;
    const maxLength = this.maxLength();
    if (maxLength === undefined || this.characterCount() <= maxLength) {
      return null;
    }
    return {
      maxlength: { requiredLength: maxLength, actualLength: this.characterCount() },
    };
  }

  toggleBold(): void { this.editor?.chain().focus().toggleBold().run(); }
  toggleItalic(): void { this.editor?.chain().focus().toggleItalic().run(); }
  toggleUnderline(): void { this.editor?.chain().focus().toggleUnderline().run(); }
  toggleStrike(): void { this.editor?.chain().focus().toggleStrike().run(); }
  toggleBulletList(): void { this.editor?.chain().focus().toggleBulletList().run(); }
  toggleOrderedList(): void { this.editor?.chain().focus().toggleOrderedList().run(); }

  setAlignLeft(): void { this.editor?.chain().focus().setTextAlign('left').run(); }
  setAlignCenter(): void { this.editor?.chain().focus().setTextAlign('center').run(); }
  setAlignRight(): void { this.editor?.chain().focus().setTextAlign('right').run(); }
  setAlignJustify(): void { this.editor?.chain().focus().setTextAlign('justify').run(); }

  ngOnDestroy(): void {
    this.editor?.destroy();
  }
}
