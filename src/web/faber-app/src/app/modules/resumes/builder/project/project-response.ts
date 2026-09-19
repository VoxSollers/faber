import { Orderly } from '../../orderly';

export interface Project extends Orderly {
  tagline: string | null;
  name: string | null;
  url: string | null;
  startDate: string | null;
  endDate: string | null;
  description: string | null;
}
