import { getTranslations } from 'next-intl/server'
import { LoginEmailForm } from '@/features/auth/login-email'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/shared/ui'

/**
 * Login page view — Japanese market design.
 * Gradient background, centered card, LINE button first (UX-03-1).
 */
export async function LoginView() {
  const t = await getTranslations('login')

  return (
    <main className="min-h-screen flex items-center justify-center bg-brand-gradient px-4 py-8">
      <div className="w-full max-w-sm animate-fade-in">
        {/* App branding */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-white/20 backdrop-blur-sm mb-3">
            <span className="text-3xl" aria-hidden="true">💕</span>
          </div>
          <h1 className="text-2xl font-bold text-white">{t('appName')}</h1>
          <p className="text-white/80 text-sm mt-1">{t('tagline')}</p>
        </div>

        <Card className="shadow-xl border-0">
          <CardHeader className="pb-2">
            <CardTitle className="text-center text-lg">{t('title')}</CardTitle>
            <CardDescription className="text-center">{t('subtitle')}</CardDescription>
          </CardHeader>
          <CardContent>
            <LoginEmailForm />

            <p className="text-center text-sm text-gray-500 mt-4">
              {t('noAccount')}{' '}
              <a href="/register" className="text-pink-600 font-medium hover:underline">
                {t('registerLinkLabel')}
              </a>
            </p>
          </CardContent>
        </Card>
      </div>
    </main>
  )
}
