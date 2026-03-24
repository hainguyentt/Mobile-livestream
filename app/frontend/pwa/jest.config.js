const nextJest = require('next/jest')

const createJestConfig = nextJest({ dir: './' })

/** @type {import('jest').Config} */
const customConfig = {
  testEnvironment: 'jest-environment-jsdom',
  setupFilesAfterEnv: ['<rootDir>/jest.setup.ts'],
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/src/$1',
    // Redirect next-intl to manual mock to avoid ESM parsing issues in Jest
    '^next-intl$': '<rootDir>/src/__mocks__/next-intl.tsx',
    '^next-intl/(.*)$': '<rootDir>/src/__mocks__/next-intl.tsx',
  },
}

module.exports = createJestConfig(customConfig)
