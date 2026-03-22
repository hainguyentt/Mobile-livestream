import { apiClient } from '@/shared/lib/api/client'

export async function login(email: string, password: string): Promise<void> {
  await apiClient.post('/api/v1/auth/login', { email, password })
}

export async function logout(): Promise<void> {
  await apiClient.post('/api/v1/auth/logout')
}
