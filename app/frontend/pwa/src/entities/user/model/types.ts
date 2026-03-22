/** Auth response from API */
export interface AuthResponse {
  message: string
}

export interface RegisterResponse extends AuthResponse {
  userId: string
}
