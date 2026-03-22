import { getTranslations } from 'next-intl/server'
import { ResetPasswordForm } from '@/features/auth/reset-password'
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/ui'

/**
 * Reset password page view.
 */
export async function ResetPasswordView() {
  const t = await getTranslations('resetPassword')

  return (
    <main className="min-h-screen flex items-center justify-center bg-brand-gradient px-4 py-8">
      <div className="w-full max-w-sm animate-fade-in">
        <Card className="shadow-xl border-0">
          <CardHeader>
            <CardTitle className="text-center">{t('title')}</CardTitle>
          </CardHeader>
          <CardContent>
            <ResetPasswordForm />
          </CardContent>
        </Card>
      </div>
    </main>
  )
}
