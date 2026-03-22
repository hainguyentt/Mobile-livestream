import Link from 'next/link'
import { getTranslations } from 'next-intl/server'
import { PhotoUploadGrid } from '@/features/profile/photo-upload'
import { Card, CardContent, CardHeader, CardTitle } from '@/shared/ui'

/**
 * Profile photos management view.
 */
export async function ProfilePhotosView() {
  const t = await getTranslations('profile')
  const tc = await getTranslations('common')

  return (
    <main className="min-h-screen bg-gray-50 p-4">
      <div className="max-w-lg mx-auto animate-fade-in">
        <Card>
          <CardHeader>
            <div className="flex items-center gap-3">
              <Link
                href="/profile"
                className="flex items-center justify-center w-9 h-9 rounded-lg text-gray-500 hover:bg-gray-100 transition-colors"
                aria-label={tc('back')}
              >
                ←
              </Link>
              <CardTitle>{t('photosPageTitle')}</CardTitle>
            </div>
          </CardHeader>
          <CardContent>
            <PhotoUploadGrid />
          </CardContent>
        </Card>
      </div>
    </main>
  )
}
