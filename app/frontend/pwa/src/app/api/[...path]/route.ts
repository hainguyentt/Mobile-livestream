import { type NextRequest, NextResponse } from 'next/server'
import { cookies } from 'next/headers'

const BACKEND_URL = process.env.BACKEND_URL ?? 'http://localhost:5174'

/**
 * Catch-all API proxy route.
 * Forwards all /api/* requests to the backend, preserving cookies and headers.
 * This avoids cross-origin cookie issues in development.
 */
async function handler(request: NextRequest): Promise<NextResponse> {
  const { pathname, search } = request.nextUrl
  const targetUrl = `${BACKEND_URL}${pathname}${search}`

  // Forward all headers except host
  const headers = new Headers(request.headers)
  headers.delete('host')

  // Explicitly forward cookies from the incoming request
  const cookieStore = await cookies()
  const cookieHeader = cookieStore.getAll()
    .map(c => `${c.name}=${c.value}`)
    .join('; ')
  if (cookieHeader) {
    headers.set('cookie', cookieHeader)
  }

  const backendResponse = await fetch(targetUrl, {
    method: request.method,
    headers,
    body: request.method !== 'GET' && request.method !== 'HEAD'
      ? await request.arrayBuffer()
      : undefined,
    redirect: 'manual',
  })

  // Forward all response headers including Set-Cookie
  const responseHeaders = new Headers()
  backendResponse.headers.forEach((value, key) => {
    // Next.js strips Set-Cookie if we use new Headers(backendResponse.headers)
    // so we must iterate and set manually
    responseHeaders.append(key, value)
  })

  return new NextResponse(backendResponse.body, {
    status: backendResponse.status,
    statusText: backendResponse.statusText,
    headers: responseHeaders,
  })
}

export const GET = handler
export const POST = handler
export const PUT = handler
export const PATCH = handler
export const DELETE = handler
export const OPTIONS = handler
