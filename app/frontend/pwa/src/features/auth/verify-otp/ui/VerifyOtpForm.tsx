'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { authApi } from '@/entities/user'
import { Button, OtpInputField } from '@/shared/ui'
import type { OtpPurpose } from '@/shared/types'

export interface VerifyOtpFormProps {
  purpose: OtpPurpose
  target?: string
  onSuccess?: () => void
}

/**
 * OTP verification form — used for email verification, phone verification, and password reset.
 */
export function VerifyOtpForm({ purpose, target, onSuccess }: VerifyOtpFormProps) {
  const t = useTranslations('otp')
  const [digits, setDigits] = useState<string[]>(Array(6).fill(''))
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const handleChange = (index: number, value: string) => {
    const newDigits = [...digits]
    newDigits[index] = value
    setDigits(newDigits)
  }

  const handleKeyDown = (_index: number, _e: React.KeyboardEvent) => {
    // handled inside OtpInputField
  }

  const handlePaste = (e: React.ClipboardEvent) => {
    const pasted = e.clipboardData.getData('text').replace(/\D/g, '').slice(0, 6)
    if (pasted.length > 0) {
      e.preventDefault()
      const newDigits = Array(6).fill('')
      pasted.split('').forEach((char, i) => { newDigits[i] = char })
      setDigits(newDigits)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    const code = digits.join('')
    if (code.length !== 6) return

    setIsLoading(true)
    setError(null)
    try {
      await authApi.verifyOtp(target ?? '', code, purpose)
      onSuccess?.()
    } catch {
      setError(t('invalidError'))
      setDigits(Array(6).fill(''))
    } finally {
      setIsLoading(false)
    }
  }

  const isComplete = digits.join('').length === 6

  return (
    <div className="w-full space-y-4">
      {error && (
        <p role="alert" className="text-rose-500 text-sm text-center bg-rose-50 rounded-lg px-3 py-2">
          {error}
        </p>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <OtpInputField
          digits={digits}
          onChange={handleChange}
          onKeyDown={handleKeyDown}
          onPaste={handlePaste}
          groupLabel={t('groupLabel')}
          digitLabel={(i) => t('digitLabel', { index: i + 1 })}
        />

        <Button type="submit" disabled={isLoading || !isComplete} className="w-full" size="lg">
          {isLoading ? t('verifyingButton') : t('submitButton')}
        </Button>
      </form>
    </div>
  )
}
