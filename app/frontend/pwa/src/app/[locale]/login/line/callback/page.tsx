'use client'

import { useEffect } from 'react'
import { useRouter, useSearchParams } from 'next/navigation'
import { useAuthStore } from '@/entities/user'

/**
 * LINE OAuth callback page.
 * Receives the authorization code from LINE and exchanges it for tokens.
 */
export default function LineCallbackPage() {
  const router = useRouter()
  const searchParams = useSearchParams()
  const loginWithLine = useAuthStore((s) => s.loginWithLine)
  const error = useAuthStore((s) => s.error)

  useEffect(() => {
    const code = searchParams.get('code')
    if (!code) {
      router.replace('/login')
      return
    }

    loginWithLine(code).then(() => {
      router.replace('/profile')
    })
  }, [searchParams, loginWithLine, router])

  if (error) {
    return (
      <main className="min-h-screen flex items-center justify-center">
        <p role="alert" className="text-red-600">{error}</p>
      </main>
    )
  }

  return (
    <main className="min-h-screen flex items-center justify-center">
      <p aria-live="polite">LINEログイン処理中...</p>
    </main>
  )
}
