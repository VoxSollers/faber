export interface ResetPasswordRequest {
  combinedKey: { value: string };
  newPassword: string;
  confirmNewPassword: string;
}
