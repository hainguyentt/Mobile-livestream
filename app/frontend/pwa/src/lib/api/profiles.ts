import { apiClient } from './client'

export interface UserProfile {
  userId: string
  displayName: string
  bio: string | null
  interests: string[]
  preferredLanguage: string
  isVerifiedHost: boolean
  photos: UserPhoto[]
}

export interface UserPhoto {
  id: string
  displayIndex: number
  s3Url: string
  mimeType: string
}

export interface PresignResponse {
  uploadUrl: string
  photoId: string
  s3Key: string
}

/**
 * Creates a new profile for the authenticated user.
 */
export async function createProfile(displayName: string, dateOfBirth: string): Promise<UserProfile> {
  const { data } = await apiClient.post<UserProfile>('/api/v1/profiles/me', { displayName, dateOfBirth })
  return data
}

/**
 * Returns the current user's profile with photos.
 */
export async function getMyProfile(): Promise<UserProfile> {
  const { data } = await apiClient.get<UserProfile>('/api/v1/profiles/me')
  return data
}

/**
 * Updates bio, interests, and preferred language.
 */
export async function updateProfile(
  bio: string | null,
  interests: string[],
  preferredLanguage: string
): Promise<UserProfile> {
  const { data } = await apiClient.put<UserProfile>('/api/v1/profiles/me', { bio, interests, preferredLanguage })
  return data
}

/**
 * Generates a presigned S3 upload URL for a user photo.
 * @param index - Display order index (0-5)
 */
export async function presignPhotoUpload(
  displayIndex: number,
  contentType: string,
  fileSizeBytes: number
): Promise<PresignResponse> {
  const { data } = await apiClient.post<PresignResponse>('/api/v1/profiles/photos/presign', {
    displayIndex,
    contentType,
    fileSizeBytes,
  })
  return data
}

/**
 * Confirms a photo upload after the client has PUT the file to S3.
 * @param displayIndex - Slot index (0-5) for the photo
 */
export async function confirmPhotoUpload(
  photoId: string,
  displayIndex: number,
  s3Key: string,
  s3Url: string,
  fileSizeBytes: number,
  mimeType: string
): Promise<UserPhoto> {
  const { data } = await apiClient.post<UserPhoto>('/api/v1/profiles/photos/confirm', {
    photoId,
    displayIndex,
    s3Key,
    s3Url,
    fileSizeBytes,
    mimeType,
  })
  return data
}

/**
 * Deletes a photo.
 */
export async function deletePhoto(photoId: string): Promise<void> {
  await apiClient.delete(`/api/v1/profiles/photos/${photoId}`)
}

/**
 * Reorders photos by providing new ordered array of photo IDs.
 */
export async function reorderPhotos(orderedPhotoIds: string[]): Promise<void> {
  await apiClient.put('/api/v1/profiles/photos/reorder', { orderedPhotoIds })
}
