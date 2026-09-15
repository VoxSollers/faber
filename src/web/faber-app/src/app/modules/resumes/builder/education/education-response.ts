import { Orderly } from '../../orderly';

export interface Education extends Orderly {
  school: string;
  degree: string;
  startDate: string;
  endDate: string;
  city: string;
  description: string;
}
