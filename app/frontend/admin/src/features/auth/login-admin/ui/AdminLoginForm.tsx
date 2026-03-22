'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { useAdminStore } from '@/entities/admin'
import { Button } from '@/shared/ui/button'
import { Input } from '@/shared/ui/input'

/**
 * Admin login form — handles email/password submission and redirects on success.
 */
export function AdminLoginForm() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const router = useRouter()
  const { login, isLoading, error } = useAdminStore()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    await login(email, password)
    if (!useAdminStore.getState().error) {
      router.push('/dashboard')
    }
  }

  return (
    <div className="bg-card rounded-lg border border-border shadow-sm p-6 space-y-4">
      {error && (
        <p role="alert" className="text-destructive text-sm bg-destructive/10 rounded-md px-3 py-2">
          {error}
        </p>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="space-y-1.5">
          <label htmlFor="email" className="block text-sm font-medium text-foreground">
            Email
          </label>
          <Input
            id="email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            autoComplete="email"
            placeholder="admin@example.com"
          />
        </div>

        <div className="space-y-1.5">
          <label htmlFor="password" className="block text-sm font-medium text-foreground">
            Password
          </label>
          <Input
            id="password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            autoComplete="current-password"
          />
        </div>

        <Button
          type="submit"
          disabled={isLoading}
          className="w-full h-11"
        >
          {isLoading ? 'Logging in...' : 'Log In'}
        </Button>
      </form>
    </div>
  )
}
