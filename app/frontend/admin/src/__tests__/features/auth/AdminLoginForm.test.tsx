import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { AdminLoginForm } from '@/features/auth/login-admin'
import { useAdminStore } from '@/entities/admin'

// Mock next/navigation
const mockPush = jest.fn()
jest.mock('next/navigation', () => ({
  useRouter: () => ({ push: mockPush }),
}))

// Mock the store
jest.mock('@/entities/admin', () => ({
  useAdminStore: jest.fn(),
}))

const mockUseAdminStore = useAdminStore as jest.MockedFunction<typeof useAdminStore>

function setupStore(overrides: Partial<{ isLoading: boolean; error: string | null; login: jest.Mock }> = {}) {
  const mockLogin = overrides.login ?? jest.fn()
  mockUseAdminStore.mockReturnValue({
    isLoading: overrides.isLoading ?? false,
    error: overrides.error ?? null,
    login: mockLogin,
    logout: jest.fn(),
  })
  // Also mock getState for post-login error check
  ;(useAdminStore as unknown as { getState: () => { error: string | null } }).getState = () => ({
    error: overrides.error ?? null,
  })
  return { mockLogin }
}

describe('AdminLoginForm', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('renders_emailAndPasswordInputs', () => {
    setupStore()
    render(<AdminLoginForm />)

    expect(screen.getByLabelText('Email')).toBeInTheDocument()
    expect(screen.getByLabelText('Password')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /log in/i })).toBeInTheDocument()
  })

  it('submit_validCredentials_callsLogin', async () => {
    const { mockLogin } = setupStore()
    mockLogin.mockResolvedValueOnce(undefined)

    render(<AdminLoginForm />)

    await userEvent.type(screen.getByLabelText('Email'), 'admin@example.com')
    await userEvent.type(screen.getByLabelText('Password'), 'password123')
    await userEvent.click(screen.getByRole('button', { name: /log in/i }))

    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith('admin@example.com', 'password123')
    })
  })

  it('submit_success_redirectsToDashboard', async () => {
    const { mockLogin } = setupStore()
    mockLogin.mockResolvedValueOnce(undefined)

    render(<AdminLoginForm />)

    await userEvent.type(screen.getByLabelText('Email'), 'admin@example.com')
    await userEvent.type(screen.getByLabelText('Password'), 'password123')
    await userEvent.click(screen.getByRole('button', { name: /log in/i }))

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith('/dashboard')
    })
  })

  it('loading_state_disablesButton', () => {
    setupStore({ isLoading: true })
    render(<AdminLoginForm />)

    const button = screen.getByRole('button', { name: /logging in/i })
    expect(button).toBeDisabled()
  })

  it('error_state_showsAlertMessage', () => {
    setupStore({ error: 'Invalid credentials' })
    render(<AdminLoginForm />)

    expect(screen.getByRole('alert')).toHaveTextContent('Invalid credentials')
  })

  it('no_error_doesNotShowAlert', () => {
    setupStore({ error: null })
    render(<AdminLoginForm />)

    expect(screen.queryByRole('alert')).not.toBeInTheDocument()
  })
})
