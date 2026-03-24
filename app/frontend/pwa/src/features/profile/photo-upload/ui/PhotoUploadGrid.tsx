'use client'

import { useState, useEffect, useCallback } from 'react'
import { useTranslations } from 'next-intl'
import { motion, AnimatePresence } from 'framer-motion'
import {
  DndContext,
  closestCenter,
  PointerSensor,
  TouchSensor,
  useSensor,
  useSensors,
  DragOverlay,
  type DragStartEvent,
  type DragEndEvent,
} from '@dnd-kit/core'
import { SortableContext, rectSortingStrategy, useSortable } from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import { profilesApi } from '@/entities/profile'
import type { UserPhoto } from '@/entities/profile'

const MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']
const TOTAL_SLOTS = 6

// ─── Sortable photo slot ──────────────────────────────────────────────────────

interface PhotoSlotProps {
  slotIndex: number
  photo: UserPhoto | undefined
  isUploading: boolean
  isDeleting: boolean
  busy: boolean
  onFileChange: (e: React.ChangeEvent<HTMLInputElement>, index: number) => void
  onDelete: (e: React.MouseEvent, photo: UserPhoto) => void
  t: ReturnType<typeof useTranslations>
}

function PhotoSlot({ slotIndex, photo, isUploading, isDeleting, busy, onFileChange, onDelete, t }: PhotoSlotProps) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id: photo ? photo.id : `empty-${slotIndex}`,
    disabled: !photo || busy,
  })

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0 : 1,
  }

  return (
    <div ref={setNodeRef} style={style} className="aspect-square">
      {photo ? (
        <motion.div
          className="relative w-full h-full rounded-xl overflow-hidden group cursor-grab active:cursor-grabbing select-none"
          whileHover={{ scale: 1.04 }}
          transition={{ type: 'spring', stiffness: 300, damping: 22 }}
          {...attributes}
          {...listeners}
        >
          <img
            src={photo.s3Url}
            alt={t('photoAlt', { index: slotIndex + 1 })}
            className="w-full h-full object-cover pointer-events-none"
            onError={(e) => { e.currentTarget.style.opacity = '0.3' }}
            draggable={false}
          />

          {/* Hover overlay */}
          <div className="absolute inset-0 bg-black/0 group-hover:bg-black/45 transition-colors duration-200 flex items-center justify-center gap-3">
            {/* Replace */}
            <motion.label
              htmlFor={`photo-replace-${slotIndex}`}
              className="opacity-0 group-hover:opacity-100 transition-opacity duration-150 cursor-pointer bg-white/90 rounded-full p-2 hover:bg-white shadow"
              whileHover={{ scale: 1.15 }}
              whileTap={{ scale: 0.9 }}
              aria-label={t('photoReplaceLabel', { index: slotIndex + 1 })}
              onClick={(e) => e.stopPropagation()}
              onPointerDown={(e) => e.stopPropagation()}
            >
              <svg className="w-4 h-4 text-gray-700" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M15.232 5.232l3.536 3.536M9 13l6.586-6.586a2 2 0 112.828 2.828L11.828 15.828a4 4 0 01-1.414.828l-3 1 1-3a4 4 0 01.828-1.414z" />
              </svg>
              <input
                id={`photo-replace-${slotIndex}`}
                type="file"
                accept={ALLOWED_TYPES.join(',')}
                onChange={(e) => onFileChange(e, slotIndex)}
                disabled={busy}
                className="sr-only"
              />
            </motion.label>

            {/* Delete */}
            <motion.button
              type="button"
              onClick={(e) => onDelete(e, photo)}
              onPointerDown={(e) => e.stopPropagation()}
              disabled={busy}
              aria-label={t('photoDeleteLabel', { index: slotIndex + 1 })}
              className="opacity-0 group-hover:opacity-100 transition-opacity duration-150 bg-white/90 rounded-full p-2 hover:bg-white shadow"
              whileHover={{ scale: 1.15 }}
              whileTap={{ scale: 0.9 }}
            >
              {isDeleting ? (
                <span className="w-4 h-4 block text-center text-xs leading-4">...</span>
              ) : (
                <svg className="w-4 h-4 text-rose-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6M9 7h6m-7 0a1 1 0 011-1h4a1 1 0 011 1m-7 0h8" />
                </svg>
              )}
            </motion.button>
          </div>

          <AnimatePresence>
            {isUploading && (
              <motion.div
                className="absolute inset-0 bg-black/50 flex items-center justify-center"
                initial={{ opacity: 0 }} animate={{ opacity: 1 }} exit={{ opacity: 0 }}
              >
                <span className="text-white text-xs">{t('photoUploading')}</span>
              </motion.div>
            )}
          </AnimatePresence>
        </motion.div>
      ) : (
        <motion.label
          htmlFor={`photo-upload-${slotIndex}`}
          className="flex items-center justify-center w-full h-full rounded-xl bg-gray-100 cursor-pointer border-2 border-dashed border-gray-200"
          whileHover={{ scale: 1.04, backgroundColor: '#fdf2f8', borderColor: '#ec4899' }}
          whileTap={{ scale: 0.97 }}
          transition={{ type: 'spring', stiffness: 300, damping: 22 }}
          aria-label={t('photoUploadLabel', { index: slotIndex + 1 })}
        >
          {isUploading ? (
            <span className="text-sm text-gray-500">{t('photoUploading')}</span>
          ) : (
            <motion.span
              className="text-3xl text-gray-300"
              whileHover={{ color: '#ec4899', scale: 1.2 }}
              transition={{ type: 'spring', stiffness: 400 }}
            >
              +
            </motion.span>
          )}
          <input
            id={`photo-upload-${slotIndex}`}
            type="file"
            accept={ALLOWED_TYPES.join(',')}
            onChange={(e) => onFileChange(e, slotIndex)}
            disabled={busy}
            className="sr-only"
          />
        </motion.label>
      )}
    </div>
  )
}

// ─── Drag overlay thumbnail ───────────────────────────────────────────────────

function DragThumbnail({ photo }: { photo: UserPhoto }) {
  return (
    <div className="w-24 h-24 rounded-xl overflow-hidden shadow-2xl rotate-3 opacity-90 pointer-events-none">
      <img src={photo.s3Url} alt="" className="w-full h-full object-cover" draggable={false} />
    </div>
  )
}

// ─── Main grid ────────────────────────────────────────────────────────────────

/**
 * 6-slot photo grid.
 * - Hover: scale + overlay with replace/delete buttons
 * - Drag-and-drop reorder (dnd-kit), persists to backend
 * - Upload flow: presign -> PUT to S3 -> confirm
 */
export function PhotoUploadGrid() {
  const t = useTranslations('profile')
  const [photos, setPhotos] = useState<UserPhoto[]>([])
  const [isUploading, setIsUploading] = useState<number | null>(null)
  const [isDeleting, setIsDeleting] = useState<number | null>(null)
  const [isSavingOrder, setIsSavingOrder] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [activePhoto, setActivePhoto] = useState<UserPhoto | null>(null)

  useEffect(() => {
    profilesApi.getMyProfile()
      .then((p) => setPhotos(p.photos ?? []))
      .catch(() => {})
  }, [])

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 8 } }),
    useSensor(TouchSensor, { activationConstraint: { delay: 200, tolerance: 5 } })
  )

  const busy = isUploading !== null || isDeleting !== null || isSavingOrder

  const handleFileChange = useCallback(async (
    e: React.ChangeEvent<HTMLInputElement>,
    displayIndex: number
  ) => {
    const file = e.target.files?.[0]
    if (!file) return
    e.target.value = ''
    if (!ALLOWED_TYPES.includes(file.type)) { setError(t('photoInvalidType')); return }
    if (file.size > MAX_FILE_SIZE_BYTES) { setError(t('photoTooLarge')); return }
    setIsUploading(displayIndex)
    setError(null)
    try {
      const { uploadUrl, photoId, s3Key } = await profilesApi.presignPhotoUpload(displayIndex, file.type, file.size)
      await fetch(uploadUrl, { method: 'PUT', body: file, headers: { 'Content-Type': file.type } })
      const photo = await profilesApi.confirmPhotoUpload(photoId, displayIndex, s3Key, uploadUrl.split('?')[0], file.size, file.type)
      setPhotos((prev) => [...prev.filter((p) => p.displayIndex !== displayIndex), photo])
    } catch {
      setError(t('photoUploadError'))
    } finally {
      setIsUploading(null)
    }
  }, [t])

  const handleDelete = useCallback(async (e: React.MouseEvent, photo: UserPhoto) => {
    e.stopPropagation()
    setIsDeleting(photo.displayIndex)
    setError(null)
    try {
      await profilesApi.deletePhoto(photo.id)
      setPhotos((prev) => prev.filter((p) => p.id !== photo.id))
    } catch {
      setError(t('photoUploadError'))
    } finally {
      setIsDeleting(null)
    }
  }, [t])

  const handleDragStart = (event: DragStartEvent) => {
    setActivePhoto(photos.find((p) => p.id === event.active.id) ?? null)
  }

  const handleDragEnd = async (event: DragEndEvent) => {
    setActivePhoto(null)
    const { active, over } = event
    if (!over || active.id === over.id) return
    const from = photos.find((p) => p.id === active.id)
    const to = photos.find((p) => p.id === over.id)
    if (!from || !to) return
    const reordered = photos.map((p) => {
      if (p.id === from.id) return { ...p, displayIndex: to.displayIndex }
      if (p.id === to.id) return { ...p, displayIndex: from.displayIndex }
      return p
    })
    setPhotos(reordered)
    setIsSavingOrder(true)
    try {
      const sorted = [...reordered].sort((a, b) => a.displayIndex - b.displayIndex)
      await profilesApi.reorderPhotos(sorted.map((p) => p.id))
    } catch {
      setError(t('photoUploadError'))
      setPhotos(photos)
    } finally {
      setIsSavingOrder(false)
    }
  }

  return (
    <div>
      <AnimatePresence>
        {error && (
          <motion.p
            role="alert"
            className="text-rose-500 text-sm mb-3 bg-rose-50 rounded-lg px-3 py-2"
            initial={{ opacity: 0, y: -4 }}
            animate={{ opacity: 1, y: 0 }}
            exit={{ opacity: 0, y: -4 }}
          >
            {error}
          </motion.p>
        )}
      </AnimatePresence>

      <DndContext
        sensors={sensors}
        collisionDetection={closestCenter}
        onDragStart={handleDragStart}
        onDragEnd={handleDragEnd}
      >
        <SortableContext items={photos.map((p) => p.id)} strategy={rectSortingStrategy}>
          <div className="grid grid-cols-3 gap-3">
            {Array.from({ length: TOTAL_SLOTS }, (_, i) => {
              const photo = photos.find((p) => p.displayIndex === i)
              return (
                <PhotoSlot
                  key={photo ? photo.id : `empty-${i}`}
                  slotIndex={i}
                  photo={photo}
                  isUploading={isUploading === i}
                  isDeleting={isDeleting === i}
                  busy={busy}
                  onFileChange={handleFileChange}
                  onDelete={handleDelete}
                  t={t}
                />
              )
            })}
          </div>
        </SortableContext>

        <DragOverlay dropAnimation={{ duration: 180, easing: 'ease' }}>
          {activePhoto && <DragThumbnail photo={activePhoto} />}
        </DragOverlay>
      </DndContext>

      {isSavingOrder && (
        <p className="text-xs text-gray-400 mt-2 text-center animate-pulse">
          {t('photoSavingOrder')}
        </p>
      )}
    </div>
  )
}
