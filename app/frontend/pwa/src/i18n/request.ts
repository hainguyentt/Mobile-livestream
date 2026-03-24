import { getRequestConfig } from 'next-intl/server'

const SUPPORTED_LOCALES = ['ja', 'en']
const DEFAULT_LOCALE = 'ja'

export default getRequestConfig(async ({ requestLocale }) => {
  const requested = await requestLocale
  // Fall back to default if locale is undefined or unsupported
  const locale = requested && SUPPORTED_LOCALES.includes(requested) ? requested : DEFAULT_LOCALE

  return {
    locale,
    messages: (await import(`./locales/${locale}.json`)).default,
  }
})
