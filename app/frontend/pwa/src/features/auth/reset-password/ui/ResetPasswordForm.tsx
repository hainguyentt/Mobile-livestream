'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter } from '@/shared/lib/navigation'
import { authApi } from '@/entities/user'
import { Button, Input } from '@/shared/ui'

/**
 * Two-step password reset form: request OTP → reset with OTP.
 */
export function ResetPasswordForm() {
  const t = useTranslations('resetPassword')
  const tc = useTranslations('common')
  const [email, setEmail] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [otpCode, setOtpCode] = useState('')
  const [step, setStep] = useState<'request' | 'reset'>('request')
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const router = useRouter()

  const handleRequestOtp = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)
    setError(null)
    try {
      await authApi.sendOtp(email, 'PasswordReset')
      setStep('reset')
    } catch {
      setError(t('sendError'))
    } finally {
      setIsLoading(false)
    }
  }

  const handleReset = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)
    setError(null)
    try {
      await authApi.resetPassword(email, newPassword, otpCode)
      router.push('/login')
    } catch {
      setError(t('resetError'))
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="space-y-4">
      {error && (
        <p role="alert" className="text-rose-500 text-sm text-center bg-rose-50 rounded-lg px-3 py-2">
          {error}
        </p>
      )}

      {step === 'request' ? (
        <form onSubmit={handleRequestOtp} className="space-y-3">
          <div className="space-y-1">
            <label htmlFor="rp-email" className="block text-sm font-medium text-gray-700">
              {t('emailLabel')}
            </label>
            <Input
              id="rp-email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoComplete="email"
              placeholder="example@email.com"
            />
          </div>
          <Button type="submit" disabled={isLoading} className="w-full" size="lg">
            {isLoading ? t('sendingButton') : t('sendCodeButton')}
          </Button>
        </form>
      ) : (
        <form onSubmit={handleReset} className="space-y-3">
          <div className="space-y-1">
            <label htmlFor="rp-otp" className="block text-sm font-medium text-gray-700">
              {t('otpLabel')}
            </label>
            <Input
              id="rp-otp"
              type="text"
              inputMode="numeric"
              value={otpCode}
              onChange={(e) => setOtpCode(e.target.value)}
              maxLength={6}
              required
              placeholder="000000"
            />
          </div>
          <div className="space-y-1">
            <label htmlFor="rp-new-password" className="block text-sm font-medium text-gray-700">
              {t('newPasswordLabel')}
            </label>
            <Input
              id="rp-new-password"
              type="password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
              autoComplete="new-password"
            />
          </div>
          <Button type="submit" disabled={isLoading} className="w-full" size="lg">
            {isLoading ? tc('processing') : t('submitButton')}
          </Button>
        </form>
      )}

      <div className="text-center">
        <a href="/login" className="text-sm text-gray-500 hover:text-pink-600">
          ← {tc('back')}
        </a>
      </div>
    </div>
  )
}
