import { ComponentFixture, TestBed } from '@angular/core/testing';
import {
  LUCIDE_ICONS, LucideIconProvider,
  User, FileText, Briefcase, GraduationCap, Zap, Globe, Link2, BookOpen, Heart,
} from 'lucide-angular';
import { vi } from 'vitest';
import { Editor } from './editor';
import { ResumesStore } from '../../resumes-store';
import { mockResumesStore } from '../../../../shared/testing/mock-resumes-store';

describe('Editor', () => {
  let component: Editor;
  let fixture: ComponentFixture<Editor>;

  beforeEach(async () => {
    vi.clearAllMocks();

    await TestBed.configureTestingModule({
      imports: [Editor],
      providers: [
        { provide: ResumesStore, useValue: mockResumesStore() },
        {
          provide: LUCIDE_ICONS,
          multi: true,
          useValue: new LucideIconProvider({
            User, FileText, Briefcase, GraduationCap, Zap, Globe, Link2, BookOpen, Heart,
          }),
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(Editor);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
