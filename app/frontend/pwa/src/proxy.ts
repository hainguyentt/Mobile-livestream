import createMiddleware from 'next-intl/middleware'
import { NextRequest, NextResponse } from 'next/server'

const intlMiddleware = createMiddleware({
  locales: ['ja', 'en'],
  defaultLocale: 'ja',
  localePrefix: 'always',
})

export default function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl

  // Pass through API and SignalR hub requests — handled by next.config.js rewrites
  if (pathname.startsWith('/api/') || pathname.startsWith('/hubs/')) {
    return NextResponse.next()
  }

  return intlMiddleware(request)
}

export const config = {
  // Match all paths except Next.js internals and static files
  matcher: ['/((?!_next|_vercel|.*\\..*).*)'],
}
