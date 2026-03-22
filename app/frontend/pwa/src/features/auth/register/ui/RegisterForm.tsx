'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter } from '@/shared/lib/navigation'
import { useAuthStore } from '@/entities/user'
import { Button, Input } from '@/shared/ui'

/**
 * Email/password registration form.
 */
export function RegisterForm() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const router = useRouter()
  const { register, isLoading, error } = useAuthStore()
  const t = useTranslations('register')
  const tc = useTranslations('common')
  const ta = useTranslations('auth')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    await register(email, password)
    if (!useAuthStore.getState().error) {
      router.push(`/verify-email?email=${encodeURIComponent(email)}`)
    }
  }

  return (
    <div className="w-full space-y-4">
      {error && (
        <p role="alert" className="text-rose-500 text-sm text-center bg-rose-50 rounded-lg px-3 py-2">
          {error.startsWith('AUTH_') ? ta(error as Parameters<typeof ta>[0]) : ta('AUTH_SERVER_ERROR')}
        </p>
      )}

      <form onSubmit={handleSubmit} className="space-y-3" noValidate>
        <div className="space-y-1">
          <label htmlFor="reg-email" className="block text-sm font-medium text-gray-700">
            {t('emailLabel')}
          </label>
          <Input
            id="reg-email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            autoComplete="email"
            placeholder="example@email.com"
          />
        </div>

        <div className="space-y-1">
          <label htmlFor="reg-password" className="block text-sm font-medium text-gray-700">
            {t('passwordLabel')}
          </label>
          <Input
            id="reg-password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            autoComplete="new-password"
          />
        </div>

        <Button type="submit" disabled={isLoading} className="w-full" size="lg">
          {isLoading ? tc('processing') : t('submitButton')}
        </Button>
      </form>
    </div>
  )
}
