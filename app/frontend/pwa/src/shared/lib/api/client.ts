import axios from 'axios'

// In dev, Next.js rewrites /api/* → http://localhost:5174/api/* (same-origin, no CORS cookie issues)
// In prod, set NEXT_PUBLIC_API_URL to the backend URL
const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? ''

/**
 * Shared Axios instance for all API calls.
 * Credentials (httpOnly cookies) are sent automatically via withCredentials.
 */
export const apiClient = axios.create({
  baseURL: BASE_URL,
  withCredentials: true,
  headers: { 'Content-Type': 'application/json' },
})

// Response interceptor: handle 401 by attempting silent token refresh before failing.
// Skip refresh for auth endpoints (login/register) — 401 there means bad credentials.
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config
    const isAuthEndpoint = originalRequest?.url?.includes('/api/v1/auth/login') ||
      originalRequest?.url?.includes('/api/v1/auth/register')

    if (error.response?.status === 401 && !originalRequest._retried && !isAuthEndpoint) {
      originalRequest._retried = true
      try {
        await apiClient.post('/api/v1/auth/refresh')
        return apiClient(originalRequest)
      } catch {
        // Refresh failed — redirect to login
        if (typeof window !== 'undefined') {
          window.location.href = '/login'
        }
      }
    }

    return Promise.reject(error)
  }
)
