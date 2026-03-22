import { create } from 'zustand'
import axios from 'axios'
import * as profilesApi from '../api/profiles'
import type { UserProfile } from './types'

interface ProfileState {
  profile: UserProfile | null
  isLoading: boolean
  error: string | null
}

interface ProfileActions {
  fetchProfile: () => Promise<void>
  updateProfile: (bio: string | null, interests: string[], preferredLanguage: string) => Promise<void>
  clearError: () => void
}

function getErrorMessage(err: unknown): string {
  if (err instanceof Error) return err.message
  return 'An unexpected error occurred'
}

/**
 * Zustand store for user profile state.
 * Handles profile fetching and updates.
 */
export const useProfileStore = create<ProfileState & ProfileActions>((set) => ({
  profile: null,
  isLoading: false,
  error: null,

  fetchProfile: async () => {
    set({ isLoading: true, error: null })
    try {
      const profile = await profilesApi.getMyProfile()
      set({ profile, isLoading: false })
    } catch (err) {
      // 404 means no profile yet — not an error, just empty state
      if (axios.isAxiosError(err) && err.response?.status === 404) {
        set({ profile: null, isLoading: false })
      } else {
        set({ error: getErrorMessage(err), isLoading: false })
      }
    }
  },

  updateProfile: async (bio, interests, preferredLanguage) => {
    set({ isLoading: true, error: null })
    try {
      const profile = await profilesApi.updateProfile(bio, interests, preferredLanguage)
      set({ profile, isLoading: false })
    } catch (err) {
      set({ error: getErrorMessage(err), isLoading: false })
    }
  },

  clearError: () => set({ error: null }),
}))
