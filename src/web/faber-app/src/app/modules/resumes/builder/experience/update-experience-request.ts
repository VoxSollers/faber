export interface UpdateExperienceRequest {
  jobTitle: string;
  employer: string;
  startDate: string | null;
  endDate: string | null;
  city: string;
  description: string;
}
