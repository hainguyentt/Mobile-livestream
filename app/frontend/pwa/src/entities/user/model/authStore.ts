import { create } from 'zustand'
import axios from 'axios'
import * as authApi from '../api/auth'

interface AuthState {
  isLoading: boolean
  error: string | null
}

interface AuthActions {
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string) => Promise<void>
  loginWithLine: (code: string) => Promise<void>
  logout: () => Promise<void>
  clearError: () => void
}

function getErrorMessage(err: unknown): string {
  if (axios.isAxiosError(err)) {
    const status = err.response?.status
    // Map HTTP status codes to user-friendly error codes
    // Components translate these codes — store stays locale-agnostic
    if (status === 401) return 'AUTH_INVALID_CREDENTIALS'
    if (status === 403) return 'AUTH_FORBIDDEN'
    if (status === 409) return 'AUTH_EMAIL_TAKEN'
    if (status === 422 || status === 400) {
      // Use backend message if available, otherwise generic
      return err.response?.data?.message ?? 'AUTH_VALIDATION_ERROR'
    }
    if (status === 429) return 'AUTH_RATE_LIMITED'
    return 'AUTH_SERVER_ERROR'
  }
  return 'AUTH_SERVER_ERROR'
}

/**
 * Zustand store for authentication state.
 * Handles login, logout, LINE OAuth, and error state.
 */
export const useAuthStore = create<AuthState & AuthActions>((set) => ({
  isLoading: false,
  error: null,

  login: async (email, password) => {
    set({ isLoading: true, error: null })
    try {
      await authApi.login(email, password)
      set({ isLoading: false })
    } catch (err) {
      set({ error: getErrorMessage(err), isLoading: false })
    }
  },

  register: async (email, password) => {
    set({ isLoading: true, error: null })
    try {
      await authApi.register(email, password)
      set({ isLoading: false })
    } catch (err) {
      set({ error: getErrorMessage(err), isLoading: false })
    }
  },

  loginWithLine: async (code) => {
    set({ isLoading: true, error: null })
    try {
      await authApi.loginWithLine(code)
      set({ isLoading: false })
    } catch (err) {
      set({ error: getErrorMessage(err), isLoading: false })
    }
  },

  logout: async () => {
    set({ isLoading: true, error: null })
    try {
      await authApi.logout()
      set({ isLoading: false })
    } catch (err) {
      set({ error: getErrorMessage(err), isLoading: false })
    }
  },

  clearError: () => set({ error: null }),
}))
