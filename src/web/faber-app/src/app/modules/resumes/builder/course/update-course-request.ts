export interface UpdateCourseRequest {
  school: string;
  name: string;
  startDate: string | null;
  endDate: string | null;
  description: string;
}
