import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  Signal,
  Type,
} from '@angular/core';
import { NgComponentOutlet } from '@angular/common';
import { ResumesStore } from '../../resumes-store';
import { Person } from '../person/person';
import { Summary } from '../summary/summary';
import { Experience } from '../experience/experience';
import { Education } from '../education/education';
import { Skill } from '../skill/skill';
import { Language } from '../language/language';
import { Link } from '../link/link';
import { Course } from '../course/course';
import { Hobby } from '../hobby/hobby';
import { FbAccordion } from '../../../../shared/components/fb-accordion/fb-accordion';
import { FbAccordionItem } from '../../../../shared/components/fb-accordion-item/fb-accordion-item';

interface SectionDef {
  id: string;
  label: string;
  icon: string;
  component: Type<unknown>;
  count?: Signal<number>;
}

@Component({
  selector: 'app-section-list',
  imports: [NgComponentOutlet, FbAccordion, FbAccordionItem],
  templateUrl: './section-list.html',
  styleUrl: './section-list.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SectionList {
  protected readonly store = inject(ResumesStore);

  private readonly experienceCount = computed(() => this.store.experiences().length);
  private readonly educationCount  = computed(() => this.store.educations().length);
  private readonly skillCount      = computed(() => this.store.skills().length);
  private readonly languageCount   = computed(() => this.store.languages().length);
  private readonly linkCount       = computed(() => this.store.links().length);
  private readonly courseCount     = computed(() => this.store.courses().length);

  private readonly openState = signal<Record<string, boolean>>({ person: true });

  constructor() {
    effect(() => {
      const id = this.store.requestedSection();
      if (!id) return;
      this.setOpen(id, true);
      document.getElementById(`s-${id}`)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });

    // A bulk request carries a freshly allocated object per call, so an identical request
    // repeated after the user hand-closed a section still re-runs this effect.
    effect(() => {
      const request = this.store.requestedAllSections();
      if (!request) return;
      this.setAllOpen(request.action === 'expand');
    });
  }

  protected readonly sections: SectionDef[] = [
    { id: 'person',     label: 'Personal Details',     icon: 'user',           component: Person },
    { id: 'summary',    label: 'Professional Summary', icon: 'file-text',      component: Summary },
    { id: 'experience', label: 'Experience',            icon: 'briefcase',      component: Experience, count: this.experienceCount },
    { id: 'education',  label: 'Education',            icon: 'graduation-cap', component: Education,  count: this.educationCount },
    { id: 'skill',      label: 'Skills',               icon: 'zap',            component: Skill,      count: this.skillCount },
    { id: 'language',   label: 'Languages',            icon: 'globe',          component: Language,   count: this.languageCount },
    { id: 'link',       label: 'Links',                icon: 'link-2',         component: Link,       count: this.linkCount },
    { id: 'course',     label: 'Courses',              icon: 'book-open',      component: Course,     count: this.courseCount },
    { id: 'hobby',      label: 'Hobbies',              icon: 'heart',          component: Hobby },
  ];

  protected isOpen = (id: string): boolean => this.openState()[id] ?? false;

  protected setOpen(id: string, value: boolean): void {
    this.openState.update(s => ({ ...s, [id]: value }));
  }

  private setAllOpen(value: boolean): void {
    this.openState.set(Object.fromEntries(this.sections.map(section => [section.id, value])));
  }
}
