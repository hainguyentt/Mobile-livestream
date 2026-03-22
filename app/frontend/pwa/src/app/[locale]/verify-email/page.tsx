'use client'

import { Suspense } from 'react'
import { VerifyEmailView } from '@/views/verify-email'

export default function VerifyEmailPage() {
  return (
    <Suspense>
      <VerifyEmailView />
    </Suspense>
  )
}
