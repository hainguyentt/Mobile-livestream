# Frontend Components — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Apps**: PWA (`pwa/`) + Admin (`admin/`)  
**Ngày tạo**: 2026-03-21

---

## 1. PWA — Component Hierarchy

```
App (Next.js App Router)
├── (auth) — Public routes (no JWT required)
│   ├── /register          → RegisterPage
│   ├── /register/verify   → OtpVerifyPage
│   ├── /login             → LoginPage
│   ├── /login/line/callback → LineCallbackPage
│   ├── /forgot-password   → ForgotPasswordPage
│   └── /reset-password    → ResetPasswordPage
│
├── (onboarding) — New user flow (JWT required, profile incomplete)
│   ├── /onboarding        → OnboardingPage
│   └── /onboarding/phone  → PhoneVerifyPage (optional)
│
└── (app) — Protected routes (JWT required, profile complete)
    └── /profile
        ├── /profile/me    → MyProfilePage
        └── /profile/edit  → EditProfilePage
```

---

## 2. PWA — Auth Components

### 2.1 RegisterPage

**Route**: `/register`  
**State**:
```typescript
{
  email: string
  password: string
  confirmPassword: string
  isLoading: boolean
  errors: { email?: string; password?: string; confirmPassword?: string }
}
```

**Props**: none (page component)

**User Interactions**:
1. Fill email + password + confirmPassword → Submit
2. Click "Đăng nhập bằng LINE" → Redirect LINE OAuth
3. Click "Đã có tài khoản? Đăng nhập" → Navigate `/login`

**Form Validation** (client-side):
- Email: required, valid format
- Password: min 8 chars, 1 uppercase, 1 lowercase, 1 digit
- ConfirmPassword: must match password

**API Integration**:
- `POST /api/auth/register` → on success: navigate `/register/verify?email={email}`

**LINE Login Button**: Hiển thị trên trang này (Q-E3: C)

---

### 2.2 OtpVerifyPage

**Route**: `/register/verify`  
**State**:
```typescript
{
  otp: string          // 6 digits
  isLoading: boolean
  error: string | null
  resendCooldown: number  // seconds countdown
  email: string           // from query param
}
```

**User Interactions**:
1. Nhập 6 ô OTP (auto-focus next) → Auto-submit khi đủ 6 số
2. Click "Gửi lại OTP" (sau 60s cooldown)

**API Integration**:
- `POST /api/auth/verify-email` → on success: navigate `/onboarding` (isNewUser=true)
- `POST /api/auth/resend-otp`

---

### 2.3 LoginPage

**Route**: `/login`  
**State**:
```typescript
{
  email: string
  password: string
  captchaToken: string | null
  showCaptcha: boolean      // true khi server trả về requiresCaptcha
  isLoading: boolean
  error: string | null
}
```

**User Interactions**:
1. Fill email + password → Submit
2. [IF showCaptcha] Solve CAPTCHA → Submit
3. Click "Đăng nhập bằng LINE" → Redirect LINE OAuth
4. Click "Quên mật khẩu?" → Navigate `/forgot-password`
5. Click "Chưa có tài khoản? Đăng ký" → Navigate `/register`

**API Integration**:
- `POST /api/auth/login`
  - 200: navigate based on `isNewUser` flag (Q-E1: D)
    - `isNewUser = true` → `/onboarding`
    - `isNewUser = false` → `/` (Home/Discovery)
  - 403 `requiresCaptcha: true` → `showCaptcha = true`
  - 423 `accountLocked` → hiển thị lockout message + thời gian unlock

**LINE Login Button**: Hiển thị trên trang này (Q-E3: C)

---

### 2.4 LineCallbackPage

**Route**: `/login/line/callback`  
**State**: loading spinner only

**Logic**:
1. Extract `code` + `state` từ URL query params
2. `POST /api/auth/line/callback` với `{code, state}`
3. On success:
   - `isNewUser = true` → navigate `/onboarding`
   - `isNewUser = false` → navigate `/` (Home)
4. On error → navigate `/login?error=line_failed`

---

### 2.5 ForgotPasswordPage

**Route**: `/forgot-password`  
**State**:
```typescript
{
  email: string
  isSubmitted: boolean
  isLoading: boolean
  error: string | null
}
```

**User Interactions**:
1. Nhập email → Submit
2. [After submit] Hiển thị success message (không reveal nếu email tồn tại hay không)

**API Integration**:
- `POST /api/auth/forgot-password` → always show success message

---

### 2.6 ResetPasswordPage

**Route**: `/reset-password`  
**State**:
```typescript
{
  newPassword: string
  confirmPassword: string
  token: string           // from query param
  isLoading: boolean
  isSuccess: boolean
  error: string | null
}
```

**API Integration**:
- `POST /api/auth/reset-password` → on success: navigate `/login?message=password_reset`

---

## 3. PWA — Onboarding Components

### 3.1 OnboardingPage

**Route**: `/onboarding`  
**Mục đích**: Multi-step flow cho user mới (Q-E1: D)  
**State**:
```typescript
{
  step: 1 | 2 | 3
  displayName: string
  dateOfBirth: string    // YYYY-MM-DD
  bio: string
  interests: string[]
  isLoading: boolean
  errors: Record<string, string>
}
```

**Steps**:
- Step 1: Nhập `DisplayName` + `DateOfBirth` (bắt buộc — BR-PROFILE-01-1, 01-2)
- Step 2: Nhập `Bio` + chọn `Interests` (optional)
- Step 3: Upload avatar (optional, có thể skip)

**Form Validation**:
- Step 1: DisplayName required, DateOfBirth required + must be 18+
- Step 2: Bio max 500 chars
- Step 3: File type JPEG/PNG/WebP, max 10MB

**API Integration**:
- `PUT /api/profiles/me` (Step 1 + 2)
- `POST /api/profiles/photos` với `displayIndex: 0` (Step 3)
- On complete → navigate `/` (Home)

---

### 3.2 PhoneVerifyPage

**Route**: `/onboarding/phone`  
**Mục đích**: Optional phone verification (Q-B3: C)  
**State**:
```typescript
{
  phoneNumber: string
  otp: string
  step: 'input' | 'verify'
  isLoading: boolean
  error: string | null
  resendCooldown: number
}
```

**User Interactions**:
1. Nhập số điện thoại → Request OTP
2. Nhập OTP → Verify
3. Click "Bỏ qua" → Navigate về Home (optional, không blocking)

**API Integration**:
- `POST /api/auth/phone/request-otp`
- `POST /api/auth/phone/verify`

---

## 4. PWA — Profile Components

### 4.1 MyProfilePage

**Route**: `/profile/me`  
**State**: loaded from API, read-only display

**Hiển thị**:
- Avatar (DisplayIndex=0) + ảnh gallery (index 1-5)
- DisplayName + Verified badge (nếu có)
- Phone Verified badge (nếu `IsPhoneVerified = true`)
- DateOfBirth (hiển thị tuổi, không hiển thị ngày cụ thể)
- Bio + Interests
- Nút "Chỉnh sửa hồ sơ" → navigate `/profile/edit`
- [IF Role=Host] Nút "Yêu cầu xác minh" (nếu chưa verified)
- "Connect LINE" button (nếu chưa link LINE) — Q-E3: C

**API Integration**:
- `GET /api/profiles/me`

---

### 4.2 EditProfilePage

**Route**: `/profile/edit`  
**State**:
```typescript
{
  displayName: string
  bio: string
  interests: string[]
  photos: Array<{ index: number; url: string | null }>  // 6 slots
  isLoading: boolean
  errors: Record<string, string>
  isDirty: boolean
}
```

**User Interactions**:
1. Edit text fields → Save
2. Click ảnh slot → Upload ảnh mới (file picker)
3. Drag ảnh để reorder (drag-and-drop, index 0-5)
4. Click X trên ảnh → Xóa ảnh
5. Click "Connect LINE" → Redirect LINE OAuth (Q-E3: C)

**Photo Grid Component**:
```
[0] [1] [2]
[3] [4] [5]
```
- Slot 0: Avatar chính (có label "Avatar")
- Slots 1-5: Ảnh phụ
- Empty slot: Hiển thị "+" icon để upload

**API Integration**:
- `PUT /api/profiles/me`
- `POST /api/profiles/photos` (upload)
- `PUT /api/profiles/photos/reorder` (reorder)
- `DELETE /api/profiles/photos/{index}` (xóa)

---

## 5. PWA — Shared Components

### 5.1 AuthGuard (HOC/Middleware)

**Mục đích**: Protect routes yêu cầu authentication

```typescript
// Next.js middleware.ts
// Routes cần JWT: /profile/*, /onboarding/*, /*, (trừ /login, /register, /reset-password)
// Nếu không có cookie → redirect /login
// Nếu profile chưa complete (isProfileComplete=false) → redirect /onboarding
```

### 5.2 TokenRefreshInterceptor

**Mục đích**: Auto-refresh JWT khi expired

```typescript
// Axios interceptor hoặc fetch wrapper
// On 401 response → POST /api/auth/refresh (dùng httpOnly cookie)
// On success → Retry original request
// On refresh fail → Clear state + redirect /login
```

**Token Storage** (Q-E2: B — httpOnly Cookie):
- Access Token: `httpOnly Cookie`, `Secure`, `SameSite=Strict`, max-age=15min
- Refresh Token: `httpOnly Cookie`, `Secure`, `SameSite=Strict`, max-age=30days
- Client-side state (Zustand): chỉ lưu `user` object (không lưu tokens)

### 5.3 LineLoginButton

**Props**:
```typescript
{
  mode: 'login' | 'register' | 'connect'
  className?: string
}
```

**Hiển thị**: LINE brand button với icon + text theo `mode`
- `login`: "LINEでログイン" (Login with LINE)
- `register`: "LINEで登録" (Register with LINE)
- `connect`: "LINEを連携する" (Connect LINE)

---

## 6. Admin App — Components

### 6.1 AdminLoginPage

**Route**: `/` (admin app root)  
**State**:
```typescript
{
  email: string
  password: string
  isLoading: boolean
  error: string | null
}
```

**Đặc điểm**:
- Không có LINE Login (admin chỉ dùng email/password)
- Không có "Đăng ký" link
- Sau login thành công → navigate `/dashboard`

**API Integration**:
- `POST /api/auth/login` với role check (phải là Admin)
- Nếu role không phải Admin → 403 Forbidden

### 6.2 AdminLayout

**Mục đích**: Shared layout cho tất cả admin pages (Unit 5 sẽ implement đầy đủ)

**Skeleton trong Unit 1**:
- Sidebar navigation (placeholder links)
- Header với user info + logout button
- Main content area

---

## 7. State Management (Zustand)

### 7.1 AuthStore

```typescript
interface AuthStore {
  user: {
    id: string
    email: string
    role: 'Viewer' | 'Host' | 'Admin'
    displayName: string | null
    avatarUrl: string | null
    isEmailVerified: boolean
    isPhoneVerified: boolean
    isProfileComplete: boolean
  } | null
  isLoading: boolean

  // Actions
  setUser: (user: AuthStore['user']) => void
  clearUser: () => void
  refreshUser: () => Promise<void>
}
```

> **Lưu ý**: Tokens KHÔNG được lưu trong Zustand store. Chỉ lưu `user` object. Tokens được quản lý hoàn toàn bởi `httpOnly Cookie` (server-side set).

---

## 8. API Integration Points

| Component | Endpoint | Method | Auth Required |
|---|---|---|---|
| RegisterPage | `/api/auth/register` | POST | No |
| OtpVerifyPage | `/api/auth/verify-email` | POST | No |
| OtpVerifyPage | `/api/auth/resend-otp` | POST | No |
| LoginPage | `/api/auth/login` | POST | No |
| LineCallbackPage | `/api/auth/line/callback` | POST | No |
| ForgotPasswordPage | `/api/auth/forgot-password` | POST | No |
| ResetPasswordPage | `/api/auth/reset-password` | POST | No |
| TokenRefreshInterceptor | `/api/auth/refresh` | POST | Cookie |
| PhoneVerifyPage | `/api/auth/phone/request-otp` | POST | Yes |
| PhoneVerifyPage | `/api/auth/phone/verify` | POST | Yes |
| OnboardingPage | `/api/profiles/me` | PUT | Yes |
| OnboardingPage | `/api/profiles/photos` | POST | Yes |
| MyProfilePage | `/api/profiles/me` | GET | Yes |
| EditProfilePage | `/api/profiles/me` | PUT | Yes |
| EditProfilePage | `/api/profiles/photos` | POST | Yes |
| EditProfilePage | `/api/profiles/photos/reorder` | PUT | Yes |
| EditProfilePage | `/api/profiles/photos/{index}` | DELETE | Yes |
| MyProfilePage | `/api/profiles/verify-request` | POST | Yes (Host only) |
| AdminLoginPage | `/api/auth/login` | POST | No |
