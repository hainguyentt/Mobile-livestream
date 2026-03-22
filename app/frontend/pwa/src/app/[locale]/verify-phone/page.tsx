'use client'

import { Suspense } from 'react'
import { VerifyPhoneView } from '@/views/verify-phone'

export default function VerifyPhonePage() {
  return (
    <Suspense>
      <VerifyPhoneView />
    </Suspense>
  )
}
