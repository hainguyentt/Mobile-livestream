const nextJest = require('next/jest')

const createJestConfig = nextJest({ dir: './' })

/** @type {import('jest').Config} */
const config = {
  testEnvironment: 'jest-environment-jsdom',
  setupFilesAfterEnv: ['<rootDir>/jest.setup.ts'],
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/src/$1',
  },
  // Allow Jest to transform ESM packages that Next.js normally handles
  transformIgnorePatterns: [
    '/node_modules/(?!(class-variance-authority|clsx|tailwind-merge)/)',
  ],
}

module.exports = createJestConfig(config)
