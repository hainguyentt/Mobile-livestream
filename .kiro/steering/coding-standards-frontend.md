---
inclusion: fileMatch
fileMatchPattern: "app/frontend/**"
---

# Frontend Coding Standards — Next.js 14 / TypeScript

## Language & Framework
- Next.js 14+ with App Router
- TypeScript strict mode (`"strict": true`)
- Tailwind CSS for styling
- Zustand for state management
- Axios for HTTP client

## Project Structure — Feature-Sliced Design (FSD)

```
app/frontend/
├── pwa/src/
│   ├── app/[locale]/          # Next.js route entry points — chỉ import từ views/
│   ├── views/                 # FSD: composition layer (page-level)
│   ├── widgets/               # FSD: complex UI blocks
│   ├── features/              # FSD: user interactions / use cases
│   ├── entities/              # FSD: domain models + API queries
│   └── shared/
│       ├── ui/                # shadcn/ui components
│       ├── lib/api/           # Axios client + interceptors
│       └── i18n/messages/     # ja.json, en.json
└── admin/src/
    ├── app/
    ├── components/
    └── lib/api/
```

> ⚠️ Dùng `src/views/` — KHÔNG dùng `src/pages/` (conflict với Next.js App Router)

## Naming Conventions

| Element | Convention | Example |
|---|---|---|
| Component file | PascalCase | `AuthForm.tsx`, `OtpInput.tsx` |
| Page file | lowercase | `page.tsx`, `layout.tsx` |
| Hook | `use` prefix, camelCase | `useAuthStore`, `useProfile` |
| Store file | camelCase + `Store` | `authStore.ts`, `profileStore.ts` |
| API client file | camelCase | `auth.ts`, `profiles.ts` |
| Type / Interface | PascalCase | `UserProfile`, `AuthTokens` |
| Enum | PascalCase | `UserRole` |
| CSS class | Tailwind utility classes only — no custom CSS unless necessary |

## Component Rules

- One component per file
- Export as named export (not default) for non-page components
- Pages use default export (Next.js requirement)
- Props interface defined above the component, named `{ComponentName}Props`
- Use `React.FC` only when children prop is needed — otherwise plain function

```tsx
// CORRECT
interface AuthFormProps {
  onSuccess: () => void;
  mode: 'login' | 'register';
}

export function AuthForm({ onSuccess, mode }: AuthFormProps) { ... }

// WRONG
export default function AuthForm(props: any) { ... }
```

## TypeScript Rules

- No `any` — use `unknown` and narrow with type guards
- No non-null assertion `!` unless absolutely unavoidable (add comment explaining why)
- Always type async function return values explicitly: `async function foo(): Promise<User>`
- Use `type` for unions/intersections, `interface` for object shapes
- Use `readonly` for props and state that should not be mutated

## State Management

| Loại state | Tool |
|---|---|
| Server state (API data, caching) | TanStack Query (`useQuery`, `useMutation`) |
| Client/UI state (auth session, UI flags) | Zustand |
| Real-time state (SignalR events) | SignalR → Zustand bridge qua `useSignalRSubscription` |

KHÔNG dùng Redux, Context API cho global state.

### Zustand store pattern
- One store per domain (`authStore`, `profileStore`)
- Interface tách `State` và `Actions`
- Never mutate state directly — always use `set()`

### HTTP Error Handling trong store
Phân biệt rõ các loại lỗi — không treat tất cả exceptions như nhau:

```ts
import axios from 'axios'

fetchProfile: async () => {
  set({ isLoading: true, error: null })
  try {
    const profile = await profilesApi.getMyProfile()
    set({ profile, isLoading: false })
  } catch (err) {
    // 404 = resource chưa tồn tại — không phải lỗi, là empty state
    if (axios.isAxiosError(err) && err.response?.status === 404) {
      set({ profile: null, isLoading: false })
    } else {
      set({ error: getErrorMessage(err), isLoading: false })
    }
  }
},
```

Các status cần xử lý riêng:
- `404` — resource không tồn tại → set về `null`, không set `error`
- `401` — unauthorized → redirect về login (xử lý ở Axios interceptor)
- `409` — conflict (duplicate) → set `error` với message cụ thể
- `422` / `400` — validation error → set `error` với message từ response body
- Các lỗi khác → set `error` với generic message

Store error messages — KHÔNG dùng i18n trong store (store không biết locale); trả về error code hoặc để component tự translate.

## API Client Rules

- Axios instance in `lib/api/client.ts` — shared across all API files
- Request interceptor: attach JWT from cookie/store
- Response interceptor: handle 401 (refresh token), normalize errors
- Each domain has its own API file (`auth.ts`, `profiles.ts`)
- API functions return typed responses, never raw `AxiosResponse`

```ts
// CORRECT
export async function login(email: string, password: string): Promise<AuthTokens> {
  const { data } = await client.post<AuthTokens>('/api/v1/auth/login', { email, password });
  return data;
}
```

## Zod Schema

- Mỗi feature có file `{feature}.schema.ts` trong `features/{domain}/{feature}/model/`
- Dùng Zod cho tất cả form validation — không validate thủ công
- Export cả schema lẫn inferred type

```ts
// features/auth/login/model/login.schema.ts
export const loginSchema = z.object({
  email: z.string().email(),
  password: z.string().min(8),
})
export type LoginFormData = z.infer<typeof loginSchema>
```

## Server Actions

- Đặt trong `features/{domain}/{feature}/api/{feature}.action.ts`
- Dùng cho form submissions thay vì client-side API call khi không cần optimistic update
- Luôn validate input với Zod trước khi gọi backend
- Trả về `{ success: true, data }` hoặc `{ success: false, error: string }`

```ts
// features/auth/login/api/login.action.ts
'use server'
export async function loginAction(formData: LoginFormData) {
  const parsed = loginSchema.safeParse(formData)
  if (!parsed.success) return { success: false, error: 'Invalid input' }
  // ...
}
```

## i18n Rules

### Bắt buộc — không hardcode string
- **TUYỆT ĐỐI KHÔNG** hardcode string hiển thị cho user trong JSX hoặc logic — kể cả error messages, aria-label, alt text, placeholder
- Mọi string phải đi qua `useTranslations` (client) hoặc `getTranslations` (server)
- Translation files: `src/i18n/locales/ja.json` (primary), `en.json` — phải đồng bộ cùng keys

### Cách dùng đúng

**Client component** — dùng `useTranslations`:
```tsx
'use client'
import { useTranslations } from 'next-intl'

export function MyComponent() {
  const t = useTranslations('profile')
  return <h1>{t('title')}</h1>
}
```

**Server component** — dùng `getTranslations`:
```tsx
import { getTranslations } from 'next-intl/server'

export default async function MyPage() {
  const t = await getTranslations('profile')
  return <h1>{t('title')}</h1>
}
```

**Interpolation** — dùng named params, không string concatenation:
```tsx
// CORRECT — key: "photoAlt": "Photo {index}"
t('photoAlt', { index: i + 1 })

// WRONG
`Photo ${i + 1}`
```

### Key format
- Format: `{namespace}.{key}` — ví dụ `profile.title`, `common.loading`
- Namespace theo domain/page: `common`, `login`, `register`, `profile`, `verifyEmail`
- Dùng `common` cho strings tái sử dụng: `loading`, `error`, `save`, `back`, `cancel`
- Khi thêm key mới: **phải thêm vào cả `ja.json` và `en.json` cùng lúc**

### Scope áp dụng
- Pages (`page.tsx`) — bắt buộc
- Reusable components (`components/`) — bắt buộc, kể cả error messages và aria-label
- Store error messages — **không** dùng i18n trong store (store không biết locale); trả về error code hoặc để component tự translate
- API error messages từ backend — hiển thị qua translation key, không hiển thị raw message

### Navigation với locale
- **LUÔN** dùng `useRouter` từ `@/lib/navigation` (wrapper của `next-intl/navigation`) — tự động thêm locale prefix
- **KHÔNG** dùng `useRouter` từ `next/navigation` — sẽ mất locale prefix khi navigate

```tsx
// CORRECT
import { useRouter } from '@/lib/navigation'
router.push('/profile') // → /ja/profile hoặc /en/profile

// WRONG
import { useRouter } from 'next/navigation'
router.push('/profile') // → /profile (404)
```

## Accessibility
- All interactive elements must have accessible labels (`aria-label` or visible text)
- Form inputs must have associated `<label>` elements
- Images must have `alt` attributes
- Use semantic HTML elements (`<button>`, `<nav>`, `<main>`, `<section>`)

## Code Style
- No semicolons (enforced by Prettier)
- Single quotes for strings
- 2-space indentation
- Trailing commas in multi-line structures
- Arrow functions for callbacks, named functions for components and hooks

## Comment Standards

### When to comment
- **Do** comment *why*, not *what*
- **Do** use JSDoc `/** */` on exported functions, hooks, and complex types
- **Do** comment non-obvious business logic or workarounds
- **Do** use `// TODO: {description} — Refs: {ticket}` for known gaps
- **Don't** comment obvious code
- **Don't** leave commented-out code blocks — delete them

### JSDoc — required on exported functions and hooks
```ts
/**
 * Zustand store for authentication state.
 * Handles login, logout, and token refresh.
 */
export const useAuthStore = create<AuthState & AuthActions>(...)

/**
 * Generates a presigned S3 upload URL for a user photo.
 * @param index - Display order index (0-5)
 */
export async function presignPhotoUpload(index: number): Promise<PresignResponse> { ... }
```

### Inline comments — explain *why*
```ts
// CORRECT
// Intercept 401 to attempt silent token refresh before failing
client.interceptors.response.use(null, async (error) => { ... })

// Store tokens in httpOnly cookie — not localStorage, to prevent XSS
document.cookie = `...`

// WRONG
// call login API
const result = await authApi.login(email, password)
```

### TODO format
```ts
// TODO: add request deduplication for concurrent refresh calls — Refs: US-01-02
// TODO: implement offline queue for failed requests — Refs: NFR-05
```
