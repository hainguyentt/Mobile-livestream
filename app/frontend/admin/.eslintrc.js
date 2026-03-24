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
    'boundaries/dependencies': [
      'error',
      {
        default: 'disallow',
        rules: [
          { from: { type: 'app' },      allow: [{ to: { type: 'views' } }, { to: { type: 'widgets' } }, { to: { type: 'features' } }, { to: { type: 'entities' } }, { to: { type: 'shared' } }] },
          { from: { type: 'views' },    allow: [{ to: { type: 'widgets' } }, { to: { type: 'features' } }, { to: { type: 'entities' } }, { to: { type: 'shared' } }] },
          { from: { type: 'widgets' },  allow: [{ to: { type: 'features' } }, { to: { type: 'entities' } }, { to: { type: 'shared' } }] },
          { from: { type: 'features' }, allow: [{ to: { type: 'entities' } }, { to: { type: 'shared' } }] },
          { from: { type: 'entities' }, allow: [{ to: { type: 'shared' } }] },
          { from: { type: 'shared' },   allow: [] },
        ],
      },
    ],
  },
}
