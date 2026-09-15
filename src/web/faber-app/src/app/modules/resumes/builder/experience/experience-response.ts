import { Orderly } from '../../orderly';

export interface Experience extends Orderly {
  jobTitle: string;
  employer: string;
  startDate: string;
  endDate: string;
  city: string;
  description: string;
}
