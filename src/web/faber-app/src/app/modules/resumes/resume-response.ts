import { Person } from './builder/person/person-response';
import { Education } from './builder/education/education-response';
import { Experience } from './builder/experience/experience-response';
import { Skill } from './builder/skill/skill-response';
import { Language } from './builder/language/language-response';
import { Link } from './builder/link/link-response';
import { Course } from './builder/course/course-response';

export interface Resume {
  id: string;
  createdAt: string;
  person: Person | null;
  title: string | null;
  summary: string;
  localization: string;
  hobbies: string;
  experience: Experience[];
  educations: Education[];
  courses: Course[];
  links: Link[];
  skills: Skill[];
  languages: Language[];
}
