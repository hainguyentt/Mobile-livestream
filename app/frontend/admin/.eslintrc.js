/** @type {import('eslint').Linter.Config} */
module.exports = {
  extends: ['next/core-web-vitals', 'plugin:boundaries/recommended'],
  plugins: ['boundaries'],
  settings: {
    'boundaries/elements': [
      { type: 'app',      pattern: 'src/app/**' },
      { type: 'views',    pattern: 'src/views/**' },
      { type: 'widgets',  pattern: 'src/widgets/**' },
      { type: 'features', pattern: 'src/features/**' },
      { type: 'entities', pattern: 'src/entities/**' },
      { type: 'shared',   pattern: 'src/shared/**' },
    ],
    'boundaries/ignore': ['**/*.test.*', '**/*.spec.*'],
  },
  rules: {
    // FSD layer hierarchy — upper layers can import from lower layers only
    'boundaries/element-types': [
      'error',
      {
        default: 'disallow',
        rules: [
          { from: 'app',      allow: ['views', 'widgets', 'features', 'entities', 'shared'] },
          { from: 'views',    allow: ['widgets', 'features', 'entities', 'shared'] },
          { from: 'widgets',  allow: ['features', 'entities', 'shared'] },
          { from: 'features', allow: ['entities', 'shared'] },
          { from: 'entities', allow: ['shared'] },
          { from: 'shared',   allow: [] },
        ],
      },
    ],
  },
}
