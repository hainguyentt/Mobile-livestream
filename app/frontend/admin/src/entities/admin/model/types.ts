export interface AdminUser {
  id: string
  email: string
  role: 'admin' | 'superadmin'
}

export interface AdminTokens {
  accessToken: string
}
