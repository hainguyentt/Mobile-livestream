'use client'

import { useEffect } from 'react'
import Link from 'next/link'
import { useTranslations } from 'next-intl'
import { useProfileStore } from '@/entities/profile'

/**
 * Profile page view — shows user profile info with edit/photo links.
 */
export function ProfileView() {
  const t = useTranslations('profile')
  const tc = useTranslations('common')
  const { profile, isLoading, error, fetchProfile } = useProfileStore()

  useEffect(() => {
    fetchProfile()
  }, [fetchProfile])

  return (
    <main className="min-h-screen bg-gray-50 p-4">
      <div className="max-w-lg mx-auto bg-white rounded-lg shadow p-6">
        <div className="flex items-center justify-between mb-4">
          <h1 className="text-xl font-semibold">{t('title')}</h1>
          {profile && (
            <Link href="/profile/edit" className="text-sm text-pink-600 hover:underline">
              {t('editButton')}
            </Link>
          )}
        </div>

        {isLoading && <p className="text-gray-500 text-sm">{tc('loading')}</p>}
        {error && <p role="alert" className="text-rose-500 text-sm">{error}</p>}

        {profile && (
          <div className="space-y-3" aria-live="polite">
            <div>
              <p className="text-xs text-gray-400">{t('displayNameLabel')}</p>
              <p className="font-medium">{profile.displayName}</p>
            </div>
            {profile.bio && (
              <div>
                <p className="text-xs text-gray-400">{t('bioLabel')}</p>
                <p className="text-sm text-gray-700">{profile.bio}</p>
              </div>
            )}
            {profile.interests.length > 0 && (
              <div>
                <p className="text-xs text-gray-400">{t('interestsLabel')}</p>
                <div className="flex flex-wrap gap-1 mt-1">
                  {profile.interests.map((interest) => (
                    <span key={interest} className="bg-pink-100 text-pink-700 text-xs px-2 py-0.5 rounded-full">
                      {interest}
                    </span>
                  ))}
                </div>
              </div>
            )}
            <div>
              <Link href="/profile/photos" className="text-sm text-pink-600 hover:underline">
                {t('managePhotos')}
              </Link>
            </div>
          </div>
        )}

        {!isLoading && !profile && !error && (
          <div className="text-center py-6">
            <p className="text-gray-500 text-sm mb-3">{t('noProfile')}</p>
            <Link
              href="/profile/edit"
              className="bg-pink-500 text-white px-4 py-2 rounded-lg text-sm hover:bg-pink-600 transition-colors"
            >
              {t('createButton')}
            </Link>
          </div>
        )}
      </div>
    </main>
  )
}
