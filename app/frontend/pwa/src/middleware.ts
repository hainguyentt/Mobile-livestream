import createMiddleware from 'next-intl/middleware'

export default createMiddleware({
  locales: ['ja', 'en'],
  defaultLocale: 'ja',
  // Redirect / → /ja, /profile → /ja/profile, etc.
  localePrefix: 'always',
})

export const config = {
  // Match all paths except Next.js internals and static files
  matcher: ['/((?!_next|_vercel|.*\\..*).*)'],
}
