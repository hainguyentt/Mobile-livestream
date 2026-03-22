import { create } from 'zustand'
import * as adminApi from '../api/admin.queries'

interface AdminState {
  isLoading: boolean
  error: string | null
}

interface AdminActions {
  login: (email: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

/**
 * Zustand store for admin authentication state.
 * Handles login and logout flows.
 */
export const useAdminStore = create<AdminState & AdminActions>((set) => ({
  isLoading: false,
  error: null,

  login: async (email, password) => {
    set({ isLoading: true, error: null })
    try {
      await adminApi.login(email, password)
      set({ isLoading: false })
    } catch (err) {
      set({ error: err instanceof Error ? err.message : 'Login failed', isLoading: false })
    }
  },

  logout: async () => {
    set({ isLoading: true })
    try {
      await adminApi.logout()
      set({ isLoading: false })
    } catch {
      set({ isLoading: false })
    }
  },
}))
