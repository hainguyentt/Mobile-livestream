import type { Metadata } from 'next'
import './globals.css'

export const metadata: Metadata = {
  title: 'Livestream App',
  description: 'Live streaming platform',
}

export default function RootLayout({
  children,
  params,
}: {
  children: React.ReactNode
  params?: { locale?: string }
}) {
  // Default to 'ja' — actual locale is set by next-intl middleware and [locale] routing
  const lang = params?.locale ?? 'ja'

  return (
    <html lang={lang}>
      <body>{children}</body>
    </html>
  )
}
