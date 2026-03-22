import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

/**
 * Merges Tailwind CSS class names, resolving conflicts intelligently.
 * Uses clsx for conditional classes + tailwind-merge for deduplication.
 */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
