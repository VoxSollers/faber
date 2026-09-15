export interface UpdateEducationRequest {
  school: string;
  degree: string;
  startDate: string | null;
  endDate: string | null;
  city: string;
  description: string;
}
