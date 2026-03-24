// Manual mock for next-intl — used in Jest tests to avoid ESM parsing issues
import React, { createContext, useContext } from 'react'

type Messages = Record<string, unknown>

const IntlContext = createContext<{ messages: Messages; locale: string }>({
  messages: {},
  locale: 'en',
})

export function NextIntlClientProvider({
  children,
  locale = 'en',
  messages = {},
}: {
  children: React.ReactNode
  locale?: string
  messages?: Messages
}) {
  return (
    <IntlContext.Provider value={{ messages, locale }}>
      {children}
    </IntlContext.Provider>
  )
}

export function useTranslations(namespace?: string) {
  const { messages } = useContext(IntlContext)

  return (key: string, params?: Record<string, unknown>) => {
    // Resolve namespace.key from messages object
    const section = namespace ? (messages[namespace] as Messages | undefined) : messages
    const value = section ? (section[key] as string | undefined) : undefined
    const str = value ?? key

    if (params) {
      return Object.entries(params).reduce(
        (s, [k, v]) => s.replace(`{${k}}`, String(v)),
        str
      )
    }
    return str
  }
}

export function useLocale() {
  const { locale } = useContext(IntlContext)
  return locale
}
