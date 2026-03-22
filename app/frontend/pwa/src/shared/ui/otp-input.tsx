'use client'

import { useRef } from 'react'
import { cn } from '@/shared/lib/utils'

export interface OtpInputFieldProps {
  digits: string[]
  onChange: (index: number, value: string) => void
  onKeyDown: (index: number, e: React.KeyboardEvent) => void
  onPaste: (e: React.ClipboardEvent) => void
  groupLabel: string
  digitLabel: (index: number) => string
}

/**
 * Reusable 6-digit OTP input field group.
 * Each digit is a separate input for better mobile UX.
 * Touch targets are 48x56px — exceeds 44px minimum (UX-05-5).
 */
export function OtpInputField({
  digits,
  onChange,
  onKeyDown,
  onPaste,
  groupLabel,
  digitLabel,
}: OtpInputFieldProps) {
  const inputRefs = useRef<(HTMLInputElement | null)[]>([])

  const handleChange = (index: number, value: string) => {
    if (!/^\d?$/.test(value)) return
    onChange(index, value)
    // Auto-advance to next input
    if (value && index < 5) {
      inputRefs.current[index + 1]?.focus()
    }
  }

  const handleKeyDown = (index: number, e: React.KeyboardEvent) => {
    // Move back on backspace when current input is empty
    if (e.key === 'Backspace' && !digits[index] && index > 0) {
      inputRefs.current[index - 1]?.focus()
    }
    onKeyDown(index, e)
  }

  return (
    <div
      className="flex gap-2 justify-center"
      role="group"
      aria-label={groupLabel}
      onPaste={onPaste}
    >
      {digits.map((digit, i) => (
        <input
          key={i}
          ref={(el) => { inputRefs.current[i] = el }}
          type="text"
          inputMode="numeric"
          maxLength={1}
          value={digit}
          onChange={(e) => handleChange(i, e.target.value)}
          onKeyDown={(e) => handleKeyDown(i, e)}
          aria-label={digitLabel(i)}
          className={cn(
            'w-12 h-14 text-center text-xl font-semibold rounded-lg border-2 transition-all duration-150',
            'focus:outline-none focus:ring-2 focus:ring-pink-500 focus:border-pink-500',
            digit
              ? 'border-pink-400 bg-pink-50 text-pink-700'
              : 'border-gray-300 bg-white text-gray-900'
          )}
        />
      ))}
    </div>
  )
}
