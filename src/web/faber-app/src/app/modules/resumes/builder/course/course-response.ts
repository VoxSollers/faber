import { Orderly } from '../../orderly';

export interface Course extends Orderly {
  school: string;
  name: string;
  startDate: string;
  endDate: string;
  description: string;
}
