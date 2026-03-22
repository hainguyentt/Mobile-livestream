'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter } from '@/shared/lib/navigation'
import { useAuthStore } from '@/entities/user'
import { Button, Input, Separator } from '@/shared/ui'/**
 * Email/password login form with LINE login button.
 * LINE button is displayed first per UX-03-1 (LINE is #1 priority in JP market).
 */
export function LoginEmailForm() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const router = useRouter()
  const { login, isLoading, error } = useAuthStore()
  const t = useTranslations('login')
  const tc = useTranslations('common')
  const ta = useTranslations('auth')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    await login(email, password)
    // Check error state after login attempt
    const currentError = useAuthStore.getState().error
    if (!currentError) {
      router.push('/profile')
    }
  }

  return (
    <div className="w-full space-y-4">
      {error && (
        <p role="alert" className="text-rose-500 text-sm text-center bg-rose-50 rounded-lg px-3 py-2">
          {error.startsWith('AUTH_') ? ta(error as Parameters<typeof ta>[0]) : ta('AUTH_SERVER_ERROR')}
        </p>
      )}

      {/* LINE login — most prominent (UX-03-1) */}
      <a
        href="/login/line"
        className="inline-flex items-center justify-center w-full h-12 px-8 rounded-lg text-base font-semibold text-white bg-[#06C755] hover:bg-[#05b34c] transition-all duration-150 active:scale-[0.97]"
      >
        <svg className="mr-2 h-5 w-5" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true" width="20" height="20">
          <path d="M19.365 9.863c.349 0 .63.285.63.631 0 .345-.281.63-.63.63H17.61v1.125h1.755c.349 0 .63.283.63.63 0 .344-.281.629-.63.629h-2.386c-.345 0-.627-.285-.627-.629V8.108c0-.345.282-.63.627-.63h2.386c.349 0 .63.285.63.63 0 .349-.281.63-.63.63H17.61v1.125h1.755zm-3.855 3.016c0 .27-.174.51-.432.596-.064.021-.133.031-.199.031-.211 0-.391-.09-.51-.25l-2.443-3.317v2.94c0 .344-.279.629-.631.629-.346 0-.626-.285-.626-.629V8.108c0-.27.173-.51.43-.595.06-.023.136-.033.194-.033.195 0 .375.104.495.254l2.462 3.33V8.108c0-.345.282-.63.63-.63.345 0 .63.285.63.63v4.771zm-5.741 0c0 .344-.282.629-.631.629-.345 0-.627-.285-.627-.629V8.108c0-.345.282-.63.627-.63.349 0 .631.285.631.63v4.771zm-2.466.629H4.917c-.345 0-.63-.285-.63-.629V8.108c0-.345.285-.63.63-.63.348 0 .63.285.63.63v4.141h1.756c.348 0 .629.283.629.63 0 .344-.281.629-.629.629M24 10.314C24 4.943 18.615.572 12 .572S0 4.943 0 10.314c0 4.811 4.27 8.842 10.035 9.608.391.082.923.258 1.058.59.12.301.079.766.038 1.08l-.164 1.02c-.045.301-.24 1.186 1.049.645 1.291-.539 6.916-4.078 9.436-6.975C23.176 14.393 24 12.458 24 10.314" />
        </svg>
        {t('lineButton')}
      </a>

      <div className="flex items-center gap-3">
        <Separator className="flex-1" />
        <span className="text-xs text-gray-400 shrink-0">または</span>
        <Separator className="flex-1" />
      </div>

      <form onSubmit={handleSubmit} className="space-y-3" noValidate>
        <div className="space-y-1">
          <label htmlFor="email" className="block text-sm font-medium text-gray-700">
            {t('emailLabel')}
          </label>
          <Input
            id="email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            autoComplete="email"
            placeholder="example@email.com"
          />
        </div>

        <div className="space-y-1">
          <label htmlFor="password" className="block text-sm font-medium text-gray-700">
            {t('passwordLabel')}
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

        <Button type="submit" disabled={isLoading} className="w-full" size="lg">
          {isLoading ? tc('processing') : t('submitButton')}
        </Button>
      </form>
    </div>
  )
}
