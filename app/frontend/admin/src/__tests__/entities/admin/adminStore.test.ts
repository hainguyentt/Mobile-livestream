import { renderHook, act } from '@testing-library/react'
import { useAdminStore } from '@/entities/admin'
import * as adminApi from '@/entities/admin/api/admin.queries'

jest.mock('@/entities/admin/api/admin.queries')

const mockLogin = adminApi.login as jest.MockedFunction<typeof adminApi.login>
const mockLogout = adminApi.logout as jest.MockedFunction<typeof adminApi.logout>

describe('useAdminStore', () => {
  beforeEach(() => {
    // Reset store state between tests
    useAdminStore.setState({ isLoading: false, error: null })
    jest.clearAllMocks()
  })

  it('initial state is correct', () => {
    const { result } = renderHook(() => useAdminStore())
    expect(result.current.isLoading).toBe(false)
    expect(result.current.error).toBeNull()
  })

  it('login_success_clearsErrorAndLoading', async () => {
    mockLogin.mockResolvedValueOnce(undefined)

    const { result } = renderHook(() => useAdminStore())
    await act(async () => {
      await result.current.login('admin@example.com', 'password123')
    })

    expect(result.current.isLoading).toBe(false)
    expect(result.current.error).toBeNull()
    expect(mockLogin).toHaveBeenCalledWith('admin@example.com', 'password123')
  })

  it('login_failure_setsErrorMessage', async () => {
    mockLogin.mockRejectedValueOnce(new Error('Invalid credentials'))

    const { result } = renderHook(() => useAdminStore())
    await act(async () => {
      await result.current.login('admin@example.com', 'wrongpassword')
    })

    expect(result.current.isLoading).toBe(false)
    expect(result.current.error).toBe('Invalid credentials')
  })

  it('login_unknownError_setsGenericMessage', async () => {
    mockLogin.mockRejectedValueOnce('unexpected error')

    const { result } = renderHook(() => useAdminStore())
    await act(async () => {
      await result.current.login('admin@example.com', 'password')
    })

    expect(result.current.error).toBe('Login failed')
  })

  it('logout_success_clearsLoading', async () => {
    mockLogout.mockResolvedValueOnce(undefined)

    const { result } = renderHook(() => useAdminStore())
    await act(async () => {
      await result.current.logout()
    })

    expect(result.current.isLoading).toBe(false)
    expect(mockLogout).toHaveBeenCalledTimes(1)
  })

  it('logout_failure_stillClearsLoading', async () => {
    mockLogout.mockRejectedValueOnce(new Error('Network error'))

    const { result } = renderHook(() => useAdminStore())
    await act(async () => {
      await result.current.logout()
    })

    expect(result.current.isLoading).toBe(false)
  })
})
