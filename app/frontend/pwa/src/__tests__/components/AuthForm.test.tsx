import { render, screen } from '@testing-library/react'
import { LoginEmailForm } from '@/features/auth/login-email'

// Mock next-intl — return key as translation value for predictable test assertions
jest.mock('next-intl', () => ({
  useTranslations: (namespace: string) => {
    const t = (key: string) => `${namespace}.${key}`
    t.rich = (key: string) => `${namespace}.${key}`
    return t
  },
}))

// Mock navigation
jest.mock('@/shared/lib/navigation', () => ({
  useRouter: () => ({ push: jest.fn() }),
}))

// Mock entities/user — define mock inside factory to avoid hoisting issues
jest.mock('@/entities/user', () => {
  const loginFn = jest.fn()
  const store = jest.fn(() => ({
    login: loginFn,
    isLoading: false,
    error: null,
  }))
  store.getState = jest.fn(() => ({ error: null }))
  return {
    useAuthStore: store,
    authApi: {},
  }
})

// Mock shared/ui to avoid class-variance-authority ESM issue in Jest
jest.mock('@/shared/ui', () => ({
  Button: ({ children, disabled, type, onClick }: React.ButtonHTMLAttributes<HTMLButtonElement>) => (
    <button type={type} disabled={disabled} onClick={onClick}>{children}</button>
  ),
  Input: ({ id, type, value, onChange, required, autoComplete, placeholder }: React.InputHTMLAttributes<HTMLInputElement>) => (
    <input id={id} type={type} value={value as string} onChange={onChange} required={required} autoComplete={autoComplete} placeholder={placeholder} />
  ),
  Separator: () => <hr />,
}))

describe('LoginEmailForm', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('renders login form with email and password fields', () => {
    render(<LoginEmailForm />)

    expect(screen.getByLabelText('login.emailLabel')).toBeInTheDocument()
    expect(screen.getByLabelText('login.passwordLabel')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'login.submitButton' })).toBeInTheDocument()
  })

  it('shows LINE login button', () => {
    render(<LoginEmailForm />)
    expect(screen.getByText('login.lineButton')).toBeInTheDocument()
  })

  it('renders email and password inputs', () => {
    render(<LoginEmailForm />)
    expect(screen.getByLabelText('login.emailLabel')).toHaveAttribute('type', 'email')
    expect(screen.getByLabelText('login.passwordLabel')).toHaveAttribute('type', 'password')
  })
})
