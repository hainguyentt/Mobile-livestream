import { redirect } from 'next/navigation'

// Root locale page — redirect to login
// Auth guard in login page will redirect to /livestream if already authenticated
export default function LocaleIndexPage() {
  redirect('/login')
}
