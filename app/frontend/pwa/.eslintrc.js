/** @type {import('eslint').Linter.Config} */
module.exports = {
  extends: ['next/core-web-vitals'],
  rules: {
    // Enforce no-barrel-import anti-pattern — import from slice index only
    'no-restricted-imports': [
      'error',
      {
        // Prevent importing from old flat structure
        patterns: [
          {
            group: ['@/components/*', '@/store/*', '@/lib/api/*'],
            message:
              'Import from FSD layers instead: @/shared/*, @/entities/*, @/features/*, @/views/*',
          },
        ],
      },
    ],
  },
}
