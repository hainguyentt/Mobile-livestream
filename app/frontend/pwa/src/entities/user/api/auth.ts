import { apiClient } from '@/shared/lib/api/client'
import type { AuthResponse, RegisterResponse } from '../model/types'

/**
 * Registers a new user with email and password.
 */
export async function register(email: string, password: string): Promise<RegisterResponse> {
  const { data } = await apiClient.post<RegisterResponse>('/api/v1/auth/register', { email, password })
  return data
}

/**
 * Authenticates with email/password. Sets httpOnly access_token cookie on success.
 */
export async function login(email: string, password: string): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/api/v1/auth/login', { email, password })
  return data
}

/**
 * Exchanges LINE authorization code for tokens.
 */
export async function loginWithLine(code: string): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/api/v1/auth/login/line', { code })
  return data
}

/**
 * Logs out and clears httpOnly cookies.
 */
export async function logout(): Promise<void> {
  await apiClient.post('/api/v1/auth/logout')
}

/**
 * Sends an OTP code to the given target.
 */
export async function sendOtp(target: string, purpose: string): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/api/v1/auth/otp/email/send', { target, purpose })
  return data
}

/**
 * Verifies an OTP code.
 */
export async function verifyOtp(target: string, code: string, purpose: string): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/api/v1/auth/otp/email/verify', { target, code, purpose })
  return data
}

/**
 * Resets password using OTP code.
 */
export async function resetPassword(email: string, newPassword: string, otpCode: string): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/api/v1/auth/password/reset', { email, newPassword, otpCode })
  return data
}
