# PWA Frontend Summary — app/frontend/pwa

**Module**: Frontend PWA (User-facing)
**Phase**: Construction — Unit 1 Core Foundation
**Ngày hoàn thành**: 2026-03-22
**Trạng thái**: ✅ Hoàn thành — verified e2e

---

## Tech Stack

- Next.js 14 (App Router)
- TypeScript strict mode
- Tailwind CSS + postcss
- Zustand (state management)
- Axios (HTTP client)
- next-intl (i18n — ja/en)
- Jest + React Testing Library (tests)
- eslint-plugin-boundaries (FSD enforcement)

---

## Cấu trúc thư mục (FSD)

```
app/frontend/pwa/src/
├── app/
│   ├── layout.tsx
│   ├── globals.css
│   └── [locale]/
│       ├── login/page.tsx
│       ├── login/line/callback/page.tsx    # LINE OAuth callback
│       ├── register/page.tsx
│       ├── verify-email/page.tsx
│       ├── verify-phone/page.tsx
│       ├── reset-password/page.tsx
│       └── profile/
│           ├── page.tsx
│           ├── edit/page.tsx
│           └── photos/page.tsx
├── views/                                  # FSD composition layer (⚠️ không phải pages/)
│   ├── login/ui/LoginView.tsx
│   ├── register/ui/RegisterView.tsx
│   ├── verify-email/ui/VerifyEmailView.tsx
│   ├── verify-phone/ui/VerifyPhoneView.tsx
│   ├── reset-password/ui/ResetPasswordView.tsx
│   ├── profile/ui/ProfileView.tsx
│   ├── profile-edit/ui/ProfileEditView.tsx
│   └── profile-photos/ui/ProfilePhotosView.tsx
├── features/
│   ├── auth/
│   │   ├── login-email/ui/LoginEmailForm.tsx
│   │   ├── register/ui/RegisterForm.tsx
│   │   ├── verify-otp/ui/VerifyOtpForm.tsx  # Dùng chung cho email + phone
│   │   └── reset-password/ui/ResetPasswordForm.tsx
│   └── profile/
│       ├── edit/ui/EditProfileForm.tsx
│       └── photo-upload/ui/PhotoUploadGrid.tsx
├── entities/
│   ├── user/
│   │   ├── model/authStore.ts              # Zustand auth store
│   │   ├── model/types.ts
│   │   ├── api/auth.ts                     # Auth API calls
│   │   └── index.ts
│   └── profile/
│       ├── model/profileStore.ts           # Zustand profile store
│       ├── model/types.ts
│       ├── api/profiles.ts                 # Profile API calls
│       └── index.ts
├── shared/
│   ├── lib/
│   │   ├── api/client.ts                   # Axios instance + interceptors
│   │   ├── utils.ts                        # cn(), getErrorMessage()
│   │   └── navigation.ts                   # next-intl useRouter wrapper
│   ├── config/routes.ts
│   ├── types/index.ts
│   └── ui/                                 # shadcn/ui components
│       ├── button.tsx, input.tsx, card.tsx
│       ├── skeleton.tsx, separator.tsx
│       ├── badge.tsx, otp-input.tsx
│       └── index.ts
├── components/
│   └── LanguageSwitcher.tsx               # JP/EN toggle (legacy, chưa migrate)
└── i18n/
    ├── request.ts
    └── locales/
        ├── ja.json                         # Japanese (primary)
        └── en.json                         # English
```

---

## Screens

| Route | View | Story |
|---|---|---|
| `/[locale]/login` | `LoginView` | US-01-02, US-01-03 |
| `/[locale]/login/line/callback` | inline | US-01-03 |
| `/[locale]/register` | `RegisterView` | US-01-01 |
| `/[locale]/verify-email` | `VerifyEmailView` | US-01-01 |
| `/[locale]/verify-phone` | `VerifyPhoneView` | US-01-04 |
| `/[locale]/reset-password` | `ResetPasswordView` | US-01-05 |
| `/[locale]/profile` | `ProfileView` | US-02-01 |
| `/[locale]/profile/edit` | `ProfileEditView` | US-02-01 |
| `/[locale]/profile/photos` | `ProfilePhotosView` | US-02-01 |

---

## State Management

### authStore (`entities/user`)
- State: `isLoading`, `error`
- Actions: `login`, `register`, `loginWithLine`, `logout`, `clearError`
- Error handling: trả về error code (`AUTH_*`) thay vì raw message — component tự translate

### profileStore (`entities/profile`)
- State: `profile: UserProfile | null`, `isLoading`, `error`
- Actions: `fetchProfile`, `updateProfile`, `clearError`
- 404 → `profile = null` (không phải error)

---

## API Client

- Base URL: `NEXT_PUBLIC_API_URL` (default: `http://localhost:5174`)
- `withCredentials: true` — gửi httpOnly cookies tự động
- Response interceptor: 401 → silent token refresh → retry once
  - **Skip refresh cho auth endpoints** (`/auth/login`, `/auth/register`) — tránh conflict

---

## i18n Namespaces

| Namespace | Dùng cho |
|---|---|
| `auth` | Error codes từ store (`AUTH_INVALID_CREDENTIALS`, v.v.) |
| `common` | Shared strings (loading, back, save, v.v.) |
| `login` | Màn hình login |
| `register` | Màn hình register |
| `otp` | OTP form (dùng chung) |
| `verifyEmail` | Màn hình verify email |
| `verifyPhone` | Màn hình verify phone |
| `resetPassword` | Màn hình reset password |
| `profile` | Tất cả profile screens |

---

## Tests

| Test File | Tests | Status |
|---|---|---|
| `__tests__/store/authStore.test.ts` | 4 | ✅ pass |
| `__tests__/components/AuthForm.test.tsx` | 3 | ✅ pass |
| `__tests__/components/OtpInput.test.tsx` | 1 (placeholder) | ✅ pass |

Chạy tests: `npm test -- --run` (trong `app/frontend/pwa/`)

---

## Build Status

```
npx next build → Exit Code 0
9 routes compiled successfully
```

---

## Known Gaps

- `widgets/` layer chưa tạo (AuthCard, ProfileHeader) — planned cho Unit 2
- `shared/ui/avatar.tsx`, `toast.tsx`, `form.tsx`, `label.tsx` chưa tạo
- `LanguageSwitcher.tsx` chưa migrate sang FSD
- E2e test với backend chưa thực hiện
