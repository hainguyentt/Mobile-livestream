import { NextIntlClientProvider } from 'next-intl'
import { getMessages } from 'next-intl/server'
import { notFound } from 'next/navigation'

const SUPPORTED_LOCALES = ['ja', 'en']

export default async function LocaleLayout({
  children,
  params,
}: {
  children: React.ReactNode
  params: { locale: string }
}) {
  const { locale } = params

  if (!SUPPORTED_LOCALES.includes(locale)) {
    notFound()
  }

  const messages = await getMessages()

  return (
    <NextIntlClientProvider locale={locale} messages={messages}>
      {children}
    </NextIntlClientProvider>
  )
}
