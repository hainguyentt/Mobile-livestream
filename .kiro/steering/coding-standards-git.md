---
inclusion: always
---

# Git Workflow Standards

## Branch Strategy — GitHub Flow

```
main                    # production-ready, always deployable
└── feature/{ticket}-{short-description}
└── fix/{ticket}-{short-description}
└── chore/{short-description}
```

### Branch Naming
```
feature/US-01-01-email-registration
feature/US-02-01-user-profile
fix/US-01-02-login-token-expiry
chore/update-dependencies
chore/add-coding-standards
```

- Always branch from `main`
- Delete branch after merge
- Never commit directly to `main`

## Commit Message Format — Conventional Commits

```
{type}({scope}): {short description}

{optional body}

{optional footer}
```

### Types
| Type | When to use |
|---|---|
| `feat` | New feature |
| `fix` | Bug fix |
| `test` | Adding or updating tests |
| `refactor` | Code change that is not a fix or feature |
| `chore` | Build process, dependencies, tooling |
| `docs` | Documentation only |
| `style` | Formatting, no logic change |
| `perf` | Performance improvement |

### Scope — use module name
`auth`, `profiles`, `api`, `shared`, `frontend-pwa`, `frontend-admin`, `infra`, `mock`

### Examples
```
feat(auth): add email OTP verification flow
fix(auth): prevent login when account is locked
test(profiles): add photo upload service tests
chore(infra): add docker-compose for local dev
docs(api): update auth endpoint reference
refactor(shared): extract token hashing to utility
```

### Rules
- Subject line: max 72 characters, imperative mood ("add" not "added")
- No period at end of subject line
- Body: explain *why*, not *what* (the diff shows what)
- Reference story ID in footer when applicable: `Refs: US-01-01`

## Pull Request Rules

- PR title follows same format as commit message
- PR description must include:
  - What changed and why
  - Story ID reference (`Refs: US-01-01`)
  - How to test locally
- Minimum 1 reviewer approval before merge
- All CI checks must pass before merge
- Squash merge to keep `main` history clean

## What NOT to Commit
- `.env` files with real secrets (use `.env.example` with placeholder values)
- `bin/`, `obj/`, `.vs/`, `node_modules/`, `.next/` (covered by `.gitignore`)
- Generated migration files without review
- Commented-out code blocks — delete or use a TODO with ticket reference
