import { AdminLoginForm } from '@/features/auth/login-admin'

/**
 * Admin login page — composition wrapper.
 */
export function AdminLoginPage() {
  return (
    <main className="min-h-screen flex items-center justify-center bg-background px-4">
      <div className="w-full max-w-sm animate-fade-in">
        {/* Admin branding */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-12 h-12 rounded-xl bg-primary text-primary-foreground mb-3">
            <svg
              width="24"
              height="24"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              aria-hidden="true"
            >
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" />
            </svg>
          </div>
          <h1 className="text-xl font-semibold text-foreground">Admin Dashboard</h1>
          <p className="text-sm text-muted-foreground mt-1">Livestream Management System</p>
        </div>

        <AdminLoginForm />
      </div>
    </main>
  )
}
