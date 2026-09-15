export interface UpdatePersonRequest {
  jobTitle: string;
  firstname: string;
  lastname: string;
  email: string;
  phone: string;
  country: string;
  city: string;
  street: string;
  postCode: string;
  nationality: string;
  dateOfBirth: string | null;
  drivingLicense: string;
}
