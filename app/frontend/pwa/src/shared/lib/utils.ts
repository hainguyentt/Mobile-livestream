import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

/** Merges Tailwind CSS class names, resolving conflicts. */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

/** Extracts a user-friendly error message from unknown error types. */
export function getErrorMessage(err: unknown): string {
  if (err instanceof Error) return err.message
  return 'An unexpected error occurred'
}
