# Frontend Unit 1 Recreation — FSD Design

**Ngày tạo**: 2026-03-22
**Phiên bản**: 1.0
**Tham chiếu**: requirements.md, frontend-component-architecture.md, frontend-uiux-design.md

---

## 1. FSD Layer Architecture

### Nguyên tắc cốt lõi

```
app/ → pages/ → widgets/ → features/ → entities/ → shared/
```

- Layer trên chỉ import từ layer dưới — KHÔNG BAO GIỜ ngược lại
- Mỗi slice có `index.ts` làm public API duy nhất
- Cross-feature imports bị cấm (features không import lẫn nhau)

---

## 2. Admin FSD Structure

```
app/frontend/admin/src/
├── app/                                    # Next.js App Router (routing only)
│   └── login/
│       └── page.tsx                        # → import { AdminLoginPage } from '@/pages/login'
│
├── pages/                                  # Composition layer
│   └── login/
│       ├── ui/
│       │   └── AdminLoginPage.tsx          # Compose features + layout
│       └── index.ts
│
├── features/                               # User interactions
│   └── auth/
│       └── login-admin/
│           ├── ui/
│           │   └── AdminLoginForm.tsx      # Form UI + submit handler
│           ├── model/
│           │   └── loginAdmin.schema.ts    # Zod validation schema
│           └── index.ts
│
├── entities/                               # Domain models
│   └── admin/
│       ├── model/
│       │   └── adminStore.ts               # Zustand: { admin, isLoading, error }
│       ├── api/
│       │   └── admin.queries.ts            # login(), logout()
│       └── index.ts
│
└── shared/                                 # Primitives (no business logic)
    ├── ui/                                 # shadcn/ui components
    │   ├── button.tsx
    │   ├── input.tsx
    │   ├── card.tsx
    │   ├── form.tsx
    │   └── label.tsx
    ├── lib/
    │   ├── api/
    │   │   └── client.ts                   # Axios instance + interceptors
    │   └── utils.ts                        # cn(), formatters
    └── config/
        └── routes.ts                       # Route constants
```

**Widgets layer**: Skip cho Admin (không có composite UI blocks)

---

## 3. PWA FSD Structure

```
app/frontend/pwa/src/
├── app/                                    # Next.js App Router (routing only)
│   └── [locale]/
│       ├── login/page.tsx                  # → @/pages/login
│       ├── register/page.tsx               # → @/pages/register
│       ├── verify-email/page.tsx           # → @/pages/verify-email
│       ├── verify-phone/page.tsx           # → @/pages/verify-phone
│       ├── reset-password/page.tsx         # → @/pages/reset-password
│       └── profile/
│           ├── page.tsx                    # → @/pages/profile
│           ├── edit/page.tsx               # → @/pages/profile-edit
│           └── photos/page.tsx             # → @/pages/photo-upload
│
├── pages/                                  # Composition layer
│   ├── login/
│   │   ├── ui/LoginPage.tsx
│   │   └── index.ts
│   ├── register/
│   │   ├── ui/RegisterPage.tsx
│   │   └── index.ts
│   ├── verify-email/
│   │   ├── ui/VerifyEmailPage.tsx
│   │   └── index.ts
│   ├── verify-phone/
│   │   ├── ui/VerifyPhonePage.tsx
│   │   └── index.ts
│   ├── reset-password/
│   │   ├── ui/ResetPasswordPage.tsx
│   │   └── index.ts
│   ├── profile/
│   │   ├── ui/ProfileViewPage.tsx
│   │   └── index.ts
│   ├── profile-edit/
│   │   ├── ui/ProfileEditPage.tsx
│   │   └── index.ts
│   └── photo-upload/
│       ├── ui/PhotoUploadPage.tsx
│       └── index.ts
│
├── widgets/                                # Composite UI blocks
│   ├── auth-card/
│   │   ├── ui/
│   │   │   └── AuthCard.tsx               # Logo + card wrapper cho auth screens
│   │   └── index.ts
│   └── profile-header/
│       ├── ui/
│       │   └── ProfileHeader.tsx          # Avatar + display name + badges
│       └── index.ts
│
├── features/                               # User interactions
│   ├── auth/
│   │   ├── login-email/
│   │   │   ├── ui/
│   │   │   │   └── LoginForm.tsx          # Email + password + LINE button
│   │   │   ├── model/
│   │   │   │   ├── loginEmail.schema.ts   # Zod schema
│   │   │   │   └── useLoginEmail.ts       # Hook: submit, loading, error
│   │   │   └── index.ts
│   │   ├── register/
│   │   │   ├── ui/
│   │   │   │   └── RegisterForm.tsx       # Email + password + terms checkbox
│   │   │   ├── model/
│   │   │   │   ├── register.schema.ts
│   │   │   │   └── useRegister.ts
│   │   │   └── index.ts
│   │   ├── verify-email/
│   │   │   ├── ui/
│   │   │   │   └── VerifyEmailForm.tsx    # OTP input + resend button
│   │   │   ├── model/
│   │   │   │   └── useVerifyEmail.ts
│   │   │   └── index.ts
│   │   ├── verify-phone/
│   │   │   ├── ui/
│   │   │   │   └── VerifyPhoneForm.tsx    # OTP input + resend button
│   │   │   ├── model/
│   │   │   │   └── useVerifyPhone.ts
│   │   │   └── index.ts
│   │   └── reset-password/
│   │       ├── ui/
│   │       │   ├── RequestResetForm.tsx   # Email input
│   │       │   └── NewPasswordForm.tsx    # New password + confirm
│   │       ├── model/
│   │       │   ├── resetPassword.schema.ts
│   │       │   └── useResetPassword.ts
│   │       └── index.ts
│   └── profile/
│       ├── view/
│       │   ├── ui/
│       │   │   └── ProfileCard.tsx        # Display profile info
│       │   └── index.ts
│       ├── edit/
│       │   ├── ui/
│       │   │   └── ProfileEditForm.tsx    # Edit display name, bio, etc.
│       │   ├── model/
│       │   │   ├── profileEdit.schema.ts
│       │   │   └── useProfileEdit.ts
│       │   └── index.ts
│       └── photo-upload/
│           ├── ui/
│           │   └── PhotoUploader.tsx      # Upload + reorder photos
│           ├── model/
│           │   └── usePhotoUpload.ts
│           └── index.ts
│
├── entities/                               # Domain models
│   ├── user/
│   │   ├── model/
│   │   │   ├── types.ts                   # User, AuthTokens interfaces
│   │   │   └── userStore.ts               # Zustand: { user, tokens, isLoading, error }
│   │   ├── api/
│   │   │   └── user.queries.ts            # login, register, logout, sendOtp, verifyOtp
│   │   └── index.ts
│   └── profile/
│       ├── model/
│       │   ├── types.ts                   # Profile, Photo interfaces
│       │   └── profileStore.ts            # Zustand: { profile, photos, isLoading, error }
│       ├── api/
│       │   └── profile.queries.ts         # getProfile, updateProfile, uploadPhoto
│       └── index.ts
│
└── shared/                                 # Primitives
    ├── ui/                                 # shadcn/ui + custom primitives
    │   ├── button.tsx
    │   ├── input.tsx
    │   ├── card.tsx
    │   ├── form.tsx
    │   ├── label.tsx
    │   ├── avatar.tsx
    │   ├── skeleton.tsx
    │   ├── toast.tsx
    │   └── otp-input.tsx                  # Custom OTP primitive (reusable)
    ├── lib/
    │   ├── api/
    │   │   └── client.ts                  # Axios instance + interceptors
    │   ├── validation/
    │   │   └── authValidation.ts          # Shared Zod schemas (email, password rules)
    │   ├── navigation.ts                  # next-intl useRouter wrapper
    │   └── utils.ts                       # cn(), getErrorMessage()
    ├── config/
    │   └── routes.ts                      # Route constants
    └── types/
        └── index.ts                       # Global types
```

---

## 4. Component Design

### 4.1 AdminLoginForm (Admin)

```tsx
// features/auth/login-admin/ui/AdminLoginForm.tsx
interface AdminLoginFormProps {
  onSuccess: () => void
}

// Renders: email input, password input, submit button
// Uses: adminStore.login(), loginAdmin.schema.ts
// Design: Neutral slate palette (Admin design tokens)
```

### 4.2 LoginForm (PWA)

```tsx
// features/auth/login-email/ui/LoginForm.tsx
interface LoginFormProps {
  onSuccess: () => void
}

// Renders: email input, password input, LINE login button, submit button
// Uses: userStore.login(), loginEmail.schema.ts
// Design: Pink gradient (PWA design tokens)
```

### 4.3 RegisterForm (PWA)

```tsx
// features/auth/register/ui/RegisterForm.tsx
interface RegisterFormProps {
  onSuccess: () => void
}

// Renders: email input, password input, confirm password, terms checkbox, submit
// Uses: userStore.register(), register.schema.ts
// Note: Diverges từ LoginForm — có terms checkbox, không có LINE button
```

### 4.4 OtpInput (Shared)

```tsx
// shared/ui/otp-input.tsx
interface OtpInputProps {
  length?: number          // default: 6
  onComplete: (otp: string) => void
  disabled?: boolean
  autoFocus?: boolean
}

// Reused by: VerifyEmailForm, VerifyPhoneForm
// Behavior: Auto-advance, paste support, backspace navigation
```

### 4.5 AuthCard Widget (PWA)

```tsx
// widgets/auth-card/ui/AuthCard.tsx
interface AuthCardProps {
  children: React.ReactNode
  title?: string
}

// Renders: App logo + card wrapper
// Used by: LoginPage, RegisterPage, VerifyEmailPage, VerifyPhonePage, ResetPasswordPage
```

### 4.6 ProfileHeader Widget (PWA)

```tsx
// widgets/profile-header/ui/ProfileHeader.tsx
// Renders: Avatar + display name + verification badges
// Used by: ProfileViewPage, ProfileEditPage
```

---

## 5. State Design

### 5.1 Admin Store

```typescript
// entities/admin/model/adminStore.ts
interface AdminState {
  admin: AdminUser | null
  isLoading: boolean
  error: string | null
}

interface AdminActions {
  login: (email: string, password: string) => Promise<void>
  logout: () => void
  clearError: () => void
}
```

### 5.2 User Store (PWA)

```typescript
// entities/user/model/userStore.ts
interface UserState {
  user: User | null
  isLoading: boolean
  error: string | null
}

interface UserActions {
  login: (email: string, password: string) => Promise<void>
  loginWithLine: () => Promise<void>
  register: (email: string, password: string) => Promise<void>
  logout: () => void
  sendEmailOtp: (email: string) => Promise<void>
  verifyEmailOtp: (email: string, code: string) => Promise<void>
  sendPhoneOtp: (phone: string) => Promise<void>
  verifyPhoneOtp: (phone: string, code: string) => Promise<void>
  requestPasswordReset: (email: string) => Promise<void>
  resetPassword: (token: string, newPassword: string) => Promise<void>
  clearError: () => void
}
```

### 5.3 Profile Store (PWA)

```typescript
// entities/profile/model/profileStore.ts
interface ProfileState {
  profile: UserProfile | null
  photos: Photo[]
  isLoading: boolean
  error: string | null
}

interface ProfileActions {
  fetchProfile: () => Promise<void>
  updateProfile: (data: UpdateProfileDto) => Promise<void>
  uploadPhoto: (file: File, index: number) => Promise<void>
  deletePhoto: (photoId: string) => Promise<void>
  clearError: () => void
}
```

---

## 6. API Design

### 6.1 Admin API

```typescript
// entities/admin/api/admin.queries.ts
export async function adminLogin(email: string, password: string): Promise<AdminTokens>
export async function adminLogout(): Promise<void>
```

### 6.2 User API (PWA)

```typescript
// entities/user/api/user.queries.ts
export async function loginWithEmail(email: string, password: string): Promise<AuthTokens>
export async function loginWithLine(code: string): Promise<AuthTokens>
export async function register(email: string, password: string): Promise<User>
export async function logout(): Promise<void>
export async function sendEmailOtp(email: string): Promise<void>
export async function verifyEmailOtp(email: string, code: string): Promise<boolean>
export async function sendPhoneOtp(phone: string): Promise<void>
export async function verifyPhoneOtp(phone: string, code: string): Promise<boolean>
export async function requestPasswordReset(email: string): Promise<void>
export async function resetPassword(token: string, newPassword: string): Promise<void>
```

### 6.3 Profile API (PWA)

```typescript
// entities/profile/api/profile.queries.ts
export async function getMyProfile(): Promise<UserProfile>
export async function updateProfile(data: UpdateProfileDto): Promise<UserProfile>
export async function getMyPhotos(): Promise<Photo[]>
export async function presignPhotoUpload(index: number): Promise<PresignResponse>
export async function confirmPhotoUpload(photoId: string): Promise<Photo>
export async function deletePhoto(photoId: string): Promise<void>
```

---

## 7. i18n Structure

### Namespace Organization

```json
// messages/ja.json
{
  "common": {
    "loading": "読み込み中...",
    "error": "エラーが発生しました",
    "save": "保存",
    "cancel": "キャンセル",
    "back": "戻る",
    "next": "次へ",
    "submit": "送信"
  },
  "auth": {
    "login": {
      "title": "ログイン",
      "emailLabel": "メールアドレス",
      "passwordLabel": "パスワード",
      "submitButton": "ログイン",
      "lineButton": "LINEでログイン",
      "forgotPassword": "パスワードを忘れた方",
      "noAccount": "アカウントをお持ちでない方"
    },
    "register": {
      "title": "新規登録",
      "emailLabel": "メールアドレス",
      "passwordLabel": "パスワード",
      "confirmPasswordLabel": "パスワード（確認）",
      "termsLabel": "利用規約に同意する",
      "submitButton": "登録する"
    },
    "verifyEmail": {
      "title": "メール認証",
      "description": "認証コードをメールに送信しました",
      "otpLabel": "認証コード",
      "resendButton": "再送信",
      "submitButton": "認証する"
    },
    "verifyPhone": {
      "title": "電話番号認証",
      "description": "認証コードをSMSに送信しました",
      "otpLabel": "認証コード",
      "resendButton": "再送信",
      "submitButton": "認証する"
    },
    "resetPassword": {
      "title": "パスワードリセット",
      "emailLabel": "メールアドレス",
      "submitButton": "リセットメールを送信",
      "newPasswordLabel": "新しいパスワード",
      "confirmNewPasswordLabel": "新しいパスワード（確認）",
      "updateButton": "パスワードを更新"
    }
  },
  "profile": {
    "view": {
      "title": "プロフィール",
      "editButton": "編集"
    },
    "edit": {
      "title": "プロフィール編集",
      "displayNameLabel": "表示名",
      "bioLabel": "自己紹介",
      "saveButton": "保存する"
    },
    "photos": {
      "title": "写真",
      "uploadButton": "写真を追加",
      "deleteButton": "削除",
      "photoAlt": "写真 {index}"
    }
  }
}
```

---

## 8. ESLint Boundaries Configuration

```javascript
// .eslintrc.js (Admin & PWA)
{
  "plugins": ["boundaries"],
  "rules": {
    "boundaries/element-types": [2, {
      "default": "disallow",
      "rules": [
        { "from": "app",      "allow": ["pages"] },
        { "from": "pages",    "allow": ["widgets", "features", "entities", "shared"] },
        { "from": "widgets",  "allow": ["features", "entities", "shared"] },
        { "from": "features", "allow": ["entities", "shared"] },
        { "from": "entities", "allow": ["shared"] },
        { "from": "shared",   "allow": [] }
      ]
    }]
  }
}
```

---

## 9. Path Aliases (tsconfig.json)

> ⚠️ **QUAN TRỌNG**: Dùng `@/views/*` thay vì `@/pages/*`. Next.js conflict với thư mục tên `pages/`.

```json
{
  "compilerOptions": {
    "paths": {
      "@/app/*":      ["./src/app/*"],
      "@/views/*":    ["./src/views/*"],
      "@/widgets/*":  ["./src/widgets/*"],
      "@/features/*": ["./src/features/*"],
      "@/entities/*": ["./src/entities/*"],
      "@/shared/*":   ["./src/shared/*"]
    }
  }
}
```

---

## 10. Test Structure

### Admin Tests

```
app/frontend/admin/src/
└── __tests__/
    ├── features/
    │   └── auth/
    │       └── AdminLoginForm.test.tsx
    └── entities/
        └── admin/
            └── adminStore.test.ts
```

### PWA Tests

```
app/frontend/pwa/src/
└── __tests__/
    ├── shared/
    │   └── ui/
    │       └── OtpInput.test.tsx
    ├── features/
    │   ├── auth/
    │   │   ├── LoginForm.test.tsx
    │   │   ├── RegisterForm.test.tsx
    │   │   ├── VerifyEmailForm.test.tsx
    │   │   ├── VerifyPhoneForm.test.tsx
    │   │   └── ResetPasswordForm.test.tsx
    │   └── profile/
    │       ├── ProfileEditForm.test.tsx
    │       └── PhotoUploader.test.tsx
    └── entities/
        ├── user/
        │   └── userStore.test.ts
        └── profile/
            └── profileStore.test.ts
```

### Test Strategy (Hybrid)

1. Đọc old test file → extract test scenarios
2. Generate new test code cho new component APIs
3. Verify tất cả scenarios covered
4. Run tests → verify pass

---

## 11. Archive Strategy

```
app/frontend/
├── _archive/
│   ├── admin-old/          # Old admin code (archived trước Phase 1)
│   └── pwa-old/            # Old PWA code (archived trước Phase 2)
├── admin/                  # New FSD admin
└── pwa/                    # New FSD PWA
```

Archive được xóa sau khi verification complete.

---

## 12. Design Tokens Reference

Tuân thủ theo `aidlc-docs/construction/cross-cutting/frontend-uiux-design.md`:

**PWA**:
- Primary: `pink-500` (#ec4899) / gradient `from-pink-400 to-rose-400`
- LINE: `#06C755` (exact)
- Error: `rose-500`
- Font: Noto Sans JP, line-height 1.7
- Touch targets: min `h-11` (44px)

**Admin**:
- Neutral slate palette
- Không dùng pink/gradient
