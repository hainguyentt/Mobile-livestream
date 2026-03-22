/**
 * Re-export next-intl navigation hooks pre-configured with supported locales.
 * Use these instead of next/navigation to get automatic locale-prefixed routing.
 */
import { createNavigation } from 'next-intl/navigation'

export const locales = ['ja', 'en'] as const
export type Locale = (typeof locales)[number]

export const { Link, redirect, useRouter, usePathname } = createNavigation({ locales })
