import axios from 'axios'

const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5174'

/**
 * Shared Axios instance for admin API calls.
 */
export const apiClient = axios.create({
  baseURL: BASE_URL,
  withCredentials: true,
  headers: { 'Content-Type': 'application/json' },
})

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401 && !error.config._retried) {
      error.config._retried = true
      try {
        await apiClient.post('/api/v1/auth/refresh')
        return apiClient(error.config)
      } catch {
        if (typeof window !== 'undefined') {
          window.location.href = '/login'
        }
      }
    }
    return Promise.reject(error)
  }
)
