'use client'

import { useSearchParams } from 'next/navigation'
import { useRouter } from '@/shared/lib/navigation'
import { useTranslations } from 'next-intl'
import { VerifyOtpForm } from '@/features/auth/verify-otp'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/ui'

/**
 * Phone verification view.
 */
export function VerifyPhoneView() {
  const searchParams = useSearchParams()
  const router = useRouter()
  const t = useTranslations('otp')
  const phone = searchParams.get('phone') ?? ''

  return (
    <main className="min-h-screen flex items-center justify-center bg-brand-gradient px-4 py-8">
      <div className="w-full max-w-sm animate-fade-in">
        <Card className="shadow-xl border-0">
          <CardHeader className="text-center">
            <div className="text-4xl mb-2" aria-hidden="true">📱</div>
            <CardTitle>{t('title')}</CardTitle>
            <CardDescription>
              {t('description')}
              {phone && (
                <span className="block font-medium text-gray-700 mt-1">{phone}</span>
              )}
            </CardDescription>
          </CardHeader>
          <CardContent>
            <VerifyOtpForm
              purpose="PhoneVerification"
              target={phone}
              onSuccess={() => router.push('/profile')}
            />
          </CardContent>
        </Card>
      </div>
    </main>
  )
}
