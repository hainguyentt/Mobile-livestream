'use client'

import { useEffect, useState } from 'react'
import { useTranslations } from 'next-intl'
import { useRouter } from '@/shared/lib/navigation'
import { useProfileStore, profilesApi } from '@/entities/profile'
import { Button, Input, Skeleton, Card, CardContent, CardHeader, CardTitle } from '@/shared/ui'
import Link from 'next/link'

/**
 * Edit/Create profile form.
 * Shows create form when no profile exists, update form otherwise.
 */
export function EditProfileForm() {
  const t = useTranslations('profile')
  const tc = useTranslations('common')
  const router = useRouter()
  const { profile, fetchProfile, isLoading, error } = useProfileStore()

  const [bio, setBio] = useState('')
  const [interests, setInterests] = useState('')
  const [preferredLanguage, setPreferredLanguage] = useState('ja')
  const [displayName, setDisplayName] = useState('')
  const [dateOfBirth, setDateOfBirth] = useState('')
  const [isSaving, setIsSaving] = useState(false)
  const [saveError, setSaveError] = useState<string | null>(null)

  useEffect(() => {
    fetchProfile()
  }, [fetchProfile])

  useEffect(() => {
    if (profile) {
      setBio(profile.bio ?? '')
      setInterests(profile.interests.join(', '))
      setPreferredLanguage(profile.preferredLanguage)
    }
  }, [profile])

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsSaving(true)
    setSaveError(null)
    try {
      await profilesApi.createProfile(displayName, dateOfBirth)
      await fetchProfile()
      router.push('/profile')
    } catch {
      setSaveError(t('createErrorMessage'))
    } finally {
      setIsSaving(false)
    }
  }

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsSaving(true)
    setSaveError(null)
    try {
      const interestList = interests.split(',').map((s) => s.trim()).filter(Boolean)
      await profilesApi.updateProfile(bio || null, interestList, preferredLanguage)
      router.push('/profile')
    } catch {
      setSaveError(t('updateErrorMessage'))
    } finally {
      setIsSaving(false)
    }
  }

  if (isLoading) {
    return (
      <main className="min-h-screen bg-gray-50 p-4">
        <div className="max-w-lg mx-auto">
          <Card>
            <CardContent className="p-6 space-y-4">
              <Skeleton className="h-6 w-32" />
              <Skeleton className="h-11 w-full" />
              <Skeleton className="h-11 w-full" />
              <Skeleton className="h-11 w-full" />
              <Skeleton className="h-11 w-full" />
            </CardContent>
          </Card>
        </div>
      </main>
    )
  }

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
              <CardTitle>{profile ? t('editTitle') : t('createTitle')}</CardTitle>
            </div>
          </CardHeader>

          <CardContent className="space-y-4">
            {(error || saveError) && (
              <p role="alert" className="text-rose-500 text-sm bg-rose-50 rounded-lg px-3 py-2">
                {saveError ?? error}
              </p>
            )}

            {!profile && (
              <form onSubmit={handleCreate} className="space-y-4" aria-live="polite">
                <div className="space-y-1">
                  <label htmlFor="displayName" className="block text-sm font-medium text-gray-700">
                    {t('displayNameLabel')} <span className="text-rose-500" aria-hidden="true">*</span>
                  </label>
                  <Input
                    id="displayName"
                    type="text"
                    value={displayName}
                    onChange={(e) => setDisplayName(e.target.value)}
                    required
                    maxLength={100}
                  />
                </div>
                <div className="space-y-1">
                  <label htmlFor="dateOfBirth" className="block text-sm font-medium text-gray-700">
                    {t('dateOfBirthLabel')} <span className="text-rose-500" aria-hidden="true">*</span>
                  </label>
                  <Input
                    id="dateOfBirth"
                    type="date"
                    value={dateOfBirth}
                    onChange={(e) => setDateOfBirth(e.target.value)}
                    required
                  />
                </div>
                <Button type="submit" disabled={isSaving} className="w-full" size="lg">
                  {isSaving ? t('creatingButton') : t('createButton')}
                </Button>
              </form>
            )}

            {profile && (
              <form onSubmit={handleUpdate} className="space-y-4" aria-live="polite">
                <div className="space-y-1">
                  <label htmlFor="bio" className="block text-sm font-medium text-gray-700">
                    {t('bioLabel')}
                  </label>
                  <textarea
                    id="bio"
                    value={bio}
                    onChange={(e) => setBio(e.target.value)}
                    maxLength={500}
                    rows={3}
                    className="flex w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm placeholder:text-gray-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-pink-500 focus-visible:border-pink-500 transition-colors duration-150 resize-none"
                  />
                </div>
                <div className="space-y-1">
                  <label htmlFor="interests" className="block text-sm font-medium text-gray-700">
                    {t('interestsHint')}
                  </label>
                  <Input
                    id="interests"
                    type="text"
                    value={interests}
                    onChange={(e) => setInterests(e.target.value)}
                    placeholder={t('interestsPlaceholder')}
                  />
                </div>
                <div className="space-y-1">
                  <label htmlFor="preferredLanguage" className="block text-sm font-medium text-gray-700">
                    {t('languageLabel')}
                  </label>
                  <select
                    id="preferredLanguage"
                    value={preferredLanguage}
                    onChange={(e) => setPreferredLanguage(e.target.value)}
                    className="flex h-11 w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-pink-500 focus-visible:border-pink-500 transition-colors duration-150"
                  >
                    <option value="ja">日本語</option>
                    <option value="en">English</option>
                  </select>
                </div>
                <Button type="submit" disabled={isSaving} className="w-full" size="lg">
                  {isSaving ? t('savingButton') : t('saveButton')}
                </Button>
              </form>
            )}
          </CardContent>
        </Card>
      </div>
    </main>
  )
}
