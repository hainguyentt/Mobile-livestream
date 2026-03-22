const createNextIntlPlugin = require('next-intl/plugin')

const withNextIntl = createNextIntlPlugin('./src/i18n/request.ts')

/** @type {import('next').NextConfig} */
const nextConfig = {
  images: {
    remotePatterns: [
      {
        protocol: 'https',
        hostname: '*.amazonaws.com',
      },
      {
        protocol: 'http',
        hostname: 'localhost',
      },
    ],
  },
}

module.exports = withNextIntl(nextConfig)
