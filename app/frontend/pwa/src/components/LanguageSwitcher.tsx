'use client'

import { useRouter, usePathname } from '@/lib/navigation'

const LOCALES = [
  { code: 'ja', label: '日本語' },
  { code: 'en', label: 'English' },
]

/**
 * Language switcher component for JP/EN toggle.
 * Uses next-intl navigation to preserve locale-aware routing.
 */
export function LanguageSwitcher() {
  const router = useRouter()
  const pathname = usePathname()

  const switchLocale = (locale: string) => {
    // usePathname from next-intl returns path without locale prefix
    // useRouter.push with locale option switches the locale
    router.push(pathname, { locale })
  }

  const currentLocale = usePathname().split('/')[0] || 'ja'

  return (
    <nav aria-label="Language switcher">
      <ul className="flex gap-2">
        {LOCALES.map(({ code, label }) => (
          <li key={code}>
            <button
              onClick={() => switchLocale(code)}
              aria-current={currentLocale === code ? 'true' : undefined}
              className={`text-sm px-2 py-1 rounded ${
                currentLocale === code
                  ? 'bg-blue-600 text-white'
                  : 'text-gray-600 hover:text-gray-900'
              }`}
            >
              {label}
            </button>
          </li>
        ))}
      </ul>
    </nav>
  )
}
