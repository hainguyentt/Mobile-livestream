export interface UserPhoto {
  id: string
  displayIndex: number
  s3Url: string
  mimeType: string
}

export interface UserProfile {
  userId: string
  displayName: string
  bio: string | null
  interests: string[]
  preferredLanguage: string
  isVerifiedHost: boolean
  photos: UserPhoto[]
}

export interface PresignResponse {
  uploadUrl: string
  photoId: string
  s3Key: string
}
