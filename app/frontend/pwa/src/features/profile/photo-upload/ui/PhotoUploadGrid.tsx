'use client'

import { useState } from 'react'
import { useTranslations } from 'next-intl'
import { profilesApi } from '@/entities/profile'
import type { UserPhoto } from '@/entities/profile'

const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024 // 10MB
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']

/**
 * 6-slot photo upload grid using presigned S3 URLs.
 * Flow: presign → PUT to S3 → confirm upload
 */
export function PhotoUploadGrid() {
  const t = useTranslations('profile')
  const [isUploading, setIsUploading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [uploadedPhotos, setUploadedPhotos] = useState<UserPhoto[]>([])

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>, displayIndex: number) => {
    const file = e.target.files?.[0]
    if (!file) return

    if (!ALLOWED_TYPES.includes(file.type)) {
      setError(t('photoInvalidType'))
      return
    }

    if (file.size > MAX_FILE_SIZE_BYTES) {
      setError(t('photoTooLarge'))
      return
    }

    setIsUploading(true)
    setError(null)

    try {
      const { uploadUrl, photoId, s3Key } = await profilesApi.presignPhotoUpload(
        displayIndex,
        file.type,
        file.size
      )

      await fetch(uploadUrl, {
        method: 'PUT',
        body: file,
        headers: { 'Content-Type': file.type },
      })

      const s3Url = uploadUrl.split('?')[0]
      const photo = await profilesApi.confirmPhotoUpload(
        photoId,
        displayIndex,
        s3Key,
        s3Url,
        file.size,
        file.type
      )

      setUploadedPhotos((prev) => [...prev.filter((p) => p.displayIndex !== displayIndex), photo])
    } catch {
      setError(t('photoUploadError'))
    } finally {
      setIsUploading(false)
    }
  }

  return (
    <div>
      {error && <p role="alert" className="text-rose-500 text-sm mb-3">{error}</p>}

      <div className="grid grid-cols-3 gap-3">
        {Array.from({ length: 6 }, (_, i) => {
          const photo = uploadedPhotos.find((p) => p.displayIndex === i)
          return (
            <div key={i} className="relative aspect-square bg-gray-100 rounded-lg overflow-hidden">
              {photo ? (
                <img
                  src={photo.s3Url}
                  alt={t('photoAlt', { index: i + 1 })}
                  className="w-full h-full object-cover"
                />
              ) : (
                <label
                  htmlFor={`photo-upload-${i}`}
                  className="flex items-center justify-center w-full h-full cursor-pointer hover:bg-gray-200"
                  aria-label={t('photoUploadLabel', { index: i + 1 })}
                >
                  <span className="text-2xl text-gray-400">+</span>
                  <input
                    id={`photo-upload-${i}`}
                    type="file"
                    accept={ALLOWED_TYPES.join(',')}
                    onChange={(e) => handleFileChange(e, i)}
                    disabled={isUploading}
                    className="sr-only"
                  />
                </label>
              )}
            </div>
          )
        })}
      </div>

      {isUploading && (
        <p aria-live="polite" className="text-sm text-gray-500 mt-2">{t('photoUploading')}</p>
      )}
    </div>
  )
}
