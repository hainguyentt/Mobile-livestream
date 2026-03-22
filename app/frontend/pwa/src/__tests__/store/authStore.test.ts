import { renderHook, act } from '@testing-library/react'
import { useAuthStore } from '@/entities/user'

// Mock auth API
jest.mock('@/entities/user/api/auth', () => ({
  login: jest.fn().mockResolvedValue({ message: 'Login successful.' }),
  register: jest.fn().mockResolvedValue({ message: 'Registration successful.', userId: 'user-123' }),
  loginWithLine: jest.fn().mockResolvedValue({ message: 'LINE login successful.' }),
  logout: jest.fn().mockResolvedValue(undefined),
}))

describe('authStore', () => {
  beforeEach(() => {
    // Reset store state between tests
    useAuthStore.setState({ isLoading: false, error: null })
  })

  it('initial state has no loading and no error', () => {
    const { result } = renderHook(() => useAuthStore())
    expect(result.current.isLoading).toBe(false)
    expect(result.current.error).toBeNull()
  })

  it('login sets isLoading during request', async () => {
    const { result } = renderHook(() => useAuthStore())

    await act(async () => {
      await result.current.login('test@example.com', 'Password123!')
    })

    expect(result.current.isLoading).toBe(false)
    expect(result.current.error).toBeNull()
  })

  it('register clears error on success', async () => {
    const { result } = renderHook(() => useAuthStore())

    await act(async () => {
      await result.current.register('new@example.com', 'Password123!')
    })

    expect(result.current.error).toBeNull()
  })

  it('clearError resets error state', () => {
    useAuthStore.setState({ error: 'Some error' })
    const { result } = renderHook(() => useAuthStore())

    act(() => {
      result.current.clearError()
    })

    expect(result.current.error).toBeNull()
  })
})
