/** Common pagination params */
export interface PaginationParams {
  page: number
  pageSize: number
}

/** Generic API error shape */
export interface ApiError {
  message: string
  statusCode: number
}

/** OTP purpose enum */
export type OtpPurpose = 'EmailVerification' | 'PhoneVerification' | 'PasswordReset'
