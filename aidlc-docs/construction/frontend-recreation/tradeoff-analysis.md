# Frontend Unit 1 Refactor — Trade-off Analysis (All Questions)

**Ngày tạo**: 2026-03-22
**Mục đích**: Phân tích trade-off cho tất cả architectural decisions trong frontend refactor
**Recommendation Priority**: FSD Compliance > Maintainability > Concurrent Development > Future Scalability

---

## Evaluation Criteria

Mỗi option được đánh giá theo 5 tiêu chí:

| Tiêu chí | Trọng số | Mô tả |
|---|---|---|
| **FSD Compliance** | ⭐⭐⭐⭐⭐ | Tuân thủ FSD methodology và dependency rules |
| **Maintainability** | ⭐⭐⭐⭐⭐ | Dễ maintain, debug, và modify code |
| **Concurrent Dev** | ⭐⭐⭐⭐ | Hỗ trợ nhiều developers làm parallel |
| **Scalability** | ⭐⭐⭐⭐ | Scale tốt khi thêm features (Unit 2-5) |
| **Simplicity** | ⭐⭐⭐ | Đơn giản, ít boilerplate, dễ học |

**Scoring**: 1-5 stars, 5 = best

---

## Question 1: FSD Layer — Composition Layer

**Context**: Unit 1 screens đơn giản (auth forms + profile). Có cần `src/views/` composition layer riêng không?

### Option A: Tạo `src/views/` layer riêng

**Structure**:
```
app/[locale]/login/page.tsx          → import LoginPage from '@/views/login'
src/views/login/ui/LoginPage.tsx     → composition logic
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Đúng FSD — tách routing và composition |
| Maintainability | ⭐⭐⭐⭐ | Clear separation, dễ test composition logic |
| Concurrent Dev | ⭐⭐⭐⭐ | Developers edit pages/ không conflict với app/ |
| Scalability | ⭐⭐⭐⭐⭐ | Chuẩn bị sẵn cho complex screens (Unit 2+) |
| Simplicity | ⭐⭐ | Thêm 1 layer, nhiều files hơn |

**Ưu điểm**:
- Tách biệt routing (Next.js concern) và composition (business concern)
- Composition logic có thể reuse cho different routes
- Test composition logic độc lập với Next.js routing

**Nhược điểm**:
- Unit 1 screens đơn giản, chưa cần complexity này
- Thêm indirection: `app/page.tsx` → `pages/ui/Page.tsx`

### Option B: Composition trong `app/**/page.tsx`

**Structure**:
```
app/[locale]/login/page.tsx     → composition logic trực tiếp
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐ | Acceptable — app/ layer có thể làm composition |
| Maintainability | ⭐⭐⭐ | Đơn giản nhưng mix routing và composition |
| Concurrent Dev | ⭐⭐⭐ | OK cho Unit 1, có thể conflict khi screens phức tạp |
| Scalability | ⭐⭐ | Khó scale khi screens phức tạp (Unit 2+) |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản nhất, ít files |

**Ưu điểm**:
- Đơn giản, ít boilerplate
- Phù hợp với simple screens của Unit 1

**Nhược điểm**:
- Mix concerns (routing + composition)
- Khó reuse composition logic
- Sẽ cần refactor lại khi screens phức tạp hơn

### 🏆 Recommendation: **Option A**

**Lý do**:
- **FSD Compliance**: Đúng methodology, chuẩn bị architecture đúng từ đầu
- **Future-proof**: Unit 2+ sẽ có complex screens (LivestreamViewer, ChatRoom) — cần pages/ layer
- **Consistency**: Tất cả units follow cùng pattern
- **Trade-off chấp nhận**: Phức tạp hơn một chút cho Unit 1, nhưng tránh refactor lại sau

---

## Question 2: FSD Layer — Widgets Layer

**Context**: Unit 1 không có complex composite UI. Có cần `src/widgets/` ngay không?

### Option A: Tạo `src/widgets/` ngay

**Structure**:
```
src/widgets/
├── auth-card/          # Wrapper cho AuthForm với branding
└── profile-header/     # Profile avatar + display name + badges
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Đầy đủ FSD layers |
| Maintainability | ⭐⭐⭐⭐ | Clear layer cho composite components |
| Concurrent Dev | ⭐⭐⭐⭐ | Widgets layer riêng, ít conflict |
| Scalability | ⭐⭐⭐⭐⭐ | Sẵn sàng cho Unit 2+ (LivestreamViewer, GiftPanel) |
| Simplicity | ⭐⭐ | Thêm layer cho Unit 1 chưa cần |

### Option B: Skip widgets cho Unit 1

**Structure**:
```
src/
├── features/      # AuthForm, OtpInput ở đây
└── entities/      # Simple presentational components
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐ | Thiếu widgets layer nhưng acceptable |
| Maintainability | ⭐⭐⭐ | OK cho Unit 1, cần thêm layer sau |
| Concurrent Dev | ⭐⭐⭐ | Đủ cho Unit 1 |
| Scalability | ⭐⭐ | Phải thêm widgets layer khi Unit 2 |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản nhất |

### Option C: Tạo 1-2 simple widgets

**Structure**:
```
src/widgets/
└── auth-card/      # Chỉ tạo 1-2 widgets đơn giản
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐ | Có widgets layer nhưng minimal |
| Maintainability | ⭐⭐⭐⭐ | Balance giữa structure và simplicity |
| Concurrent Dev | ⭐⭐⭐⭐ | Có layer nhưng không overhead |
| Scalability | ⭐⭐⭐⭐ | Dễ expand khi cần |
| Simplicity | ⭐⭐⭐⭐ | Reasonable complexity |

### 🏆 Recommendation: **Option A**

**Lý do**:
- **Consistency**: Tạo đầy đủ FSD layers từ đầu, tránh inconsistency
- **Future-proof**: Unit 2 sẽ cần nhiều widgets (LivestreamViewer, ChatRoom, GiftPanel)
- **Learning curve**: Team học FSD đầy đủ ngay từ Unit 1
- **Trade-off**: Có thể tạo 1-2 simple widgets cho Unit 1 (AuthCard, ProfileHeader)

---

## Question 3: Recreate Strategy

**Context**: Approach là "recreate from scratch" — không phải refactor/migrate. Cách thức recreate như thế nào?

### Option A: Big Bang — Recreate toàn bộ cùng lúc

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Clean FSD structure ngay |
| Maintainability | ⭐⭐⭐⭐ | Clean code, nhưng cần review kỹ |
| Concurrent Dev | ⭐⭐ | Cần coordinate timing, review toàn bộ |
| Scalability | ⭐⭐⭐⭐⭐ | Clean slate, no legacy baggage |
| Simplicity | ⭐⭐⭐⭐⭐ | Straightforward — generate all at once |

**Ưu điểm**: 
- Fastest approach — AI generate toàn bộ
- Consistent code style across all features
- No hybrid state

**Nhược điểm**: 
- Cần review kỹ toàn bộ generated code
- Có thể miss business logic details
- Testing effort lớn

### Option B: Incremental — Recreate từng feature một

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Dần dần build FSD structure |
| Maintainability | ⭐⭐⭐⭐⭐ | Review và test thoroughly từng feature |
| Concurrent Dev | ⭐⭐⭐⭐⭐ | Team có thể làm parallel trên features khác nhau |
| Scalability | ⭐⭐⭐⭐⭐ | Build incrementally, catch issues early |
| Simplicity | ⭐⭐⭐ | Cần manage sequence |

**Ưu điểm**: 
- Low risk — test thoroughly từng feature
- Catch bugs và logic issues sớm
- Có thể adjust approach dựa trên lessons learned
- Parallel development friendly

**Nhược điểm**: 
- Slower than big bang
- Cần coordinate sequence

### 🏆 Recommendation: **Option B — Incremental**

**Lý do**:
- **Quality control**: Review và test từng feature thoroughly
- **Risk mitigation**: Catch issues early, adjust approach nếu cần
- **Learning**: Lessons từ feature đầu tiên apply cho features sau
- **Concurrent dev**: Team có thể làm parallel
- **Sequence**: Login → Register → Verify Email → Verify Phone → Reset Password → Profile Edit → Photo Upload

---

## Question 4: Component Classification — AuthForm

**Context**: `AuthForm` dùng cho cả login và register. Nên đặt ở layer nào?

### Option A: Duplicate thành LoginForm và RegisterForm

**Structure**:
```
features/auth/login-email/ui/LoginForm.tsx
features/auth/register/ui/RegisterForm.tsx
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Mỗi feature self-contained |
| Maintainability | ⭐⭐⭐ | Duplicate code, changes cần sync |
| Concurrent Dev | ⭐⭐⭐⭐⭐ | Zero coupling giữa login và register |
| Scalability | ⭐⭐⭐⭐ | Mỗi feature evolve độc lập |
| Simplicity | ⭐⭐ | Duplicate logic |

**Ưu điểm**: Feature isolation tuyệt đối, login và register có thể diverge về UI  
**Nhược điểm**: Duplicate validation logic, form handling

### Option B: Shared component trong `shared/ui/`

**Structure**:
```
shared/ui/AuthForm.tsx     → reusable component
features/auth/login-email/ui/LoginEmailFeature.tsx → use AuthForm
features/auth/register/ui/RegisterFeature.tsx → use AuthForm
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐ | Shared UI acceptable nhưng không ideal |
| Maintainability | ⭐⭐⭐⭐⭐ | DRY, single source of truth |
| Concurrent Dev | ⭐⭐⭐ | Changes ảnh hưởng cả login và register |
| Scalability | ⭐⭐⭐ | Shared component có thể bloat khi thêm modes |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản nhất, giống hiện tại |

**Ưu điểm**: No duplication, dễ maintain  
**Nhược điểm**: Coupling giữa login và register

### Option C: Widget composite

**Structure**:
```
widgets/auth-card/ui/AuthCard.tsx     → composite widget
features/auth/login-email/ → use AuthCard
features/auth/register/ → use AuthCard
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐ | Widgets layer cho composite UI |
| Maintainability | ⭐⭐⭐⭐ | Clear layer, reusable |
| Concurrent Dev | ⭐⭐⭐⭐ | Widget layer riêng |
| Scalability | ⭐⭐⭐⭐ | Widget có thể compose nhiều features |
| Simplicity | ⭐⭐⭐ | Thêm widgets layer |

**Ưu điểm**: Balance giữa reusability và FSD compliance  
**Nhược điểm**: AuthCard có thể không đủ "composite" để justify widgets layer

### 🏆 Recommendation: **Option A — Duplicate**

**Lý do**:
- **FSD Compliance**: Mỗi feature self-contained
- **Future divergence**: Login và Register sẽ diverge (Login có LINE button, Register có terms checkbox)
- **Maintainability**: Duplicate ít (chỉ form structure), business logic khác nhau
- **Implementation**: Extract shared validation logic vào `shared/lib/validation/`

---

## Question 5: Component Classification — OtpInput

**Context**: `OtpInput` dùng cho verify-email và verify-phone. Nên đặt ở đâu?

### Option A: Duplicate cho mỗi feature

**Structure**:
```
features/auth/verify-email/ui/OtpInput.tsx
features/auth/verify-phone/ui/OtpInput.tsx
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Feature self-contained |
| Maintainability | ⭐⭐ | Duplicate code, bug fix cần sync |
| Concurrent Dev | ⭐⭐⭐⭐⭐ | Zero coupling |
| Scalability | ⭐⭐ | Duplicate tăng khi thêm OTP use cases |
| Simplicity | ⭐⭐ | Unnecessary duplication |

**Ưu điểm**: Feature isolation  
**Nhược điểm**: OtpInput logic hoàn toàn giống nhau — pure duplication

### Option B: Shared UI primitive

**Structure**:
```
shared/ui/otp-input.tsx     → reusable primitive
features/auth/verify-email/ → import from shared
features/auth/verify-phone/ → import from shared
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Shared UI primitives là valid FSD pattern |
| Maintainability | ⭐⭐⭐⭐⭐ | DRY, single source of truth |
| Concurrent Dev | ⭐⭐⭐⭐ | Shared component, nhưng ít thay đổi |
| Scalability | ⭐⭐⭐⭐⭐ | Reuse cho tất cả OTP use cases |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản, no duplication |

**Ưu điểm**: OtpInput là UI primitive (như Input, Button) — nên ở shared/ui/  
**Nhược điểm**: None significant

### Option C: Entity component

**Structure**:
```
entities/otp/ui/OtpInput.tsx
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐ | OTP không phải domain entity |
| Maintainability | ⭐⭐⭐ | Conceptual mismatch |
| Concurrent Dev | ⭐⭐⭐ | OK nhưng không ideal |
| Scalability | ⭐⭐⭐ | Works nhưng không semantic |
| Simplicity | ⭐⭐⭐ | Thêm entity không cần thiết |

**Ưu điểm**: Có namespace riêng  
**Nhược điểm**: OTP không phải entity — là UI primitive

### 🏆 Recommendation: **Option B — Shared UI Primitive**

**Lý do**:
- **Semantic correctness**: OtpInput là UI primitive (6-digit input với auto-focus) — giống Button, Input
- **Reusability**: Sẽ dùng cho nhiều OTP scenarios (email, phone, 2FA future)
- **FSD Compliance**: Shared UI primitives là valid pattern trong FSD
- **No duplication**: Logic hoàn toàn giống nhau, không có lý do duplicate

---

## Question 6: API Client Location

**Context**: API clients (`auth.ts`, `profiles.ts`) — 14 endpoints hiện tại, ~84 endpoints future

### Option A: `entities/{domain}/api/` — Domain Entity

**Structure**:
```
entities/
├── user/api/user.queries.ts       # login, register, logout, sendOtp, verifyOtp
└── profile/api/profile.queries.ts # CRUD profile, photos
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Entities own data fetching |
| Maintainability | ⭐⭐⭐⭐ | Domain-driven, clear ownership |
| Concurrent Dev | ⭐⭐⭐⭐ | Developers work on different entities |
| Scalability | ⭐⭐⭐⭐⭐ | 84 endpoints → 6-7 entity files (manageable) |
| Simplicity | ⭐⭐⭐ | Cần hiểu entity boundaries |

**Ưu điểm**: 
- Domain clarity: User entity owns user operations
- Scalability: 84 endpoints split thành ~10 entity files
- TanStack Query integration: Query keys + functions cùng entity

**Nhược điểm**: 
- Auth actions (login, logout) không phải "entity data"
- Shared client ở `shared/lib/api/client.ts`

### Option B: `features/{feature}/api/` — Per Feature

**Structure**:
```
features/auth/login-email/api/login.action.ts
features/auth/register/api/register.action.ts
features/auth/verify-email/api/verify.action.ts
...
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Feature self-contained |
| Maintainability | ⭐⭐⭐ | Fragmentation, khó overview API surface |
| Concurrent Dev | ⭐⭐⭐⭐⭐ | Zero coupling giữa features |
| Scalability | ⭐⭐⭐⭐ | Works nhưng nhiều files |
| Simplicity | ⭐⭐ | Overhead cao, duplication risk |

**Ưu điểm**: Feature isolation tuyệt đối  
**Nhược điểm**: 84 endpoints → 40+ files, sendOtp() duplicate

### Option C: `shared/lib/api/` — Centralized

**Structure**:
```
shared/lib/api/
├── client.ts
├── auth.ts
└── profiles.ts
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐ | Violate FSD — không gắn domain |
| Maintainability | ⭐⭐⭐ | Simple nhưng files sẽ rất lớn |
| Concurrent Dev | ⭐⭐ | Merge conflicts khi team lớn |
| Scalability | ⭐⭐ | 84 endpoints → 5-6 huge files |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản nhất |

**Ưu điểm**: Đơn giản, familiar pattern  
**Nhược điểm**: Không scale (auth.ts sẽ có 30+ functions)

### 🏆 Recommendation: **Option A — entities/{domain}/api/**

**Lý do**:
- **Scalability**: 84 endpoints → 6-7 entity files thay vì 5-6 huge centralized files
- **Domain clarity**: User, Profile, Livestream, Gift, Coin, Message — boundaries rõ ràng
- **FSD Compliance**: Đúng methodology
- **Future-proof**: Chuẩn bị cho Unit 2-5

**Implementation**:
```typescript
// entities/user/api/user.queries.ts
export const userQueries = {
  login: (email: string, password: string) => apiClient.post('/auth/login', ...),
  register: (email: string, password: string) => apiClient.post('/auth/register', ...),
  ...
}

// entities/profile/api/profile.queries.ts
export const profileQueries = {
  getMyProfile: () => apiClient.get('/profiles/me'),
  updateProfile: (...) => apiClient.put('/profiles/me', ...),
  ...
}
```

---

## Question 7: Zustand Store Location

**Context**: `authStore`, `profileStore` — client state management

### Option A: Per-feature stores

**Structure**:
```
features/auth/login-email/model/loginStore.ts
features/auth/register/model/registerStore.ts
features/profile/edit-profile/model/profileStore.ts
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Feature owns state |
| Maintainability | ⭐⭐ | Fragmentation, state sharing khó |
| Concurrent Dev | ⭐⭐⭐⭐⭐ | Zero coupling |
| Scalability | ⭐⭐ | State sharing giữa features phức tạp |
| Simplicity | ⭐⭐ | Overhead cao |

**Ưu điểm**: Feature isolation  
**Nhược điểm**: `authStore` cần share giữa nhiều features (login, register, logout, profile)

### Option B: Entity-level stores

**Structure**:
```
entities/user/model/userStore.ts       # auth state (user, tokens, isLoading)
entities/profile/model/profileStore.ts # profile state
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Entities own domain state |
| Maintainability | ⭐⭐⭐⭐⭐ | Domain-driven, clear ownership |
| Concurrent Dev | ⭐⭐⭐⭐ | Entity boundaries clear |
| Scalability | ⭐⭐⭐⭐⭐ | State gắn với entity, dễ scale |
| Simplicity | ⭐⭐⭐⭐ | Reasonable complexity |

**Ưu điểm**: 
- Domain state gắn với entity
- Multiple features share entity state naturally
- Clear ownership

**Nhược điểm**: None significant

### Option C: Centralized stores

**Structure**:
```
shared/lib/store/authStore.ts
shared/lib/store/profileStore.ts
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐ | Violate FSD |
| Maintainability | ⭐⭐⭐ | Simple nhưng không scale |
| Concurrent Dev | ⭐⭐ | Merge conflicts |
| Scalability | ⭐⭐ | Centralized state không scale |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản nhất |

**Ưu điểm**: Đơn giản  
**Nhược điểm**: Không FSD compliant, không scale

### 🏆 Recommendation: **Option B — entities/{domain}/model/**

**Lý do**:
- **Domain state**: Auth state gắn với User entity, profile state gắn với Profile entity
- **Sharing**: Multiple features naturally share entity state
- **FSD Compliance**: Đúng methodology
- **Scalability**: Mỗi entity có store riêng

**Implementation**:
```typescript
// entities/user/model/userStore.ts
export const useUserStore = create<UserState & UserActions>((set) => ({
  user: null,
  isLoading: false,
  error: null,
  login: async (...) => { ... },
  register: async (...) => { ... },
  logout: () => { ... },
}))

// entities/profile/model/profileStore.ts
export const useProfileStore = create<ProfileState & ProfileActions>(...)
```

---

## Question 8: Test Strategy

**Context**: Approach là "recreate from scratch". Tests cũ (`AuthForm.test.tsx`, `OtpInput.test.tsx`, `authStore.test.ts`) — reuse hay recreate?

### Option A: Reuse existing tests — Adapt to new structure

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Tests follow FSD structure |
| Maintainability | ⭐⭐⭐⭐⭐ | Preserve test coverage, proven test cases |
| Concurrent Dev | ⭐⭐⭐⭐ | Adapt tests parallel với code generation |
| Scalability | ⭐⭐⭐⭐⭐ | Proven test scenarios |
| Simplicity | ⭐⭐⭐ | Cần adapt imports và structure |

**Ưu điểm**: 
- Preserve existing test coverage
- Test cases đã proven (catch real bugs)
- Faster than writing from scratch

**Nhược điểm**: 
- Cần update imports và file paths
- Có thể không match new component APIs

### Option B: Recreate tests from scratch

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Tests designed for new structure |
| Maintainability | ⭐⭐⭐⭐ | Clean tests, match new APIs |
| Concurrent Dev | ⭐⭐⭐⭐⭐ | Generate tests cùng với code |
| Scalability | ⭐⭐⭐⭐ | Fresh start |
| Simplicity | ⭐⭐⭐⭐⭐ | No adaptation needed |

**Ưu điểm**: 
- Tests designed cho new component APIs
- Consistent với recreate approach
- No legacy test patterns

**Nhược điểm**: 
- Có thể miss edge cases từ old tests
- More effort than adapting

### Option C: Hybrid — Reuse test scenarios, recreate test code

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Tests follow FSD |
| Maintainability | ⭐⭐⭐⭐⭐ | Best of both worlds |
| Concurrent Dev | ⭐⭐⭐⭐⭐ | Leverage existing scenarios |
| Scalability | ⭐⭐⭐⭐⭐ | Proven scenarios + clean code |
| Simplicity | ⭐⭐⭐⭐ | Balanced approach |

**Ưu điểm**: 
- Preserve proven test scenarios (what to test)
- Fresh test code (how to test)
- Best quality

**Nhược điểm**: 
- Cần analyze old tests để extract scenarios

### 🏆 Recommendation: **Option C — Hybrid**

**Lý do**:
- **Quality**: Preserve proven test scenarios từ old tests
- **Clean code**: Recreate test implementation cho new structure
- **Coverage**: Không miss edge cases
- **Consistency**: Test code match new component APIs

**Process**:
```
1. Analyze old test → extract test scenarios
2. Generate new test code for new component
3. Ensure all scenarios covered
4. Add new scenarios if needed
```

---

## Question 9: i18n Structure

**Context**: Translation files `i18n/locales/ja.json`, `en.json` — ~50 keys hiện tại

### Option A: Giữ nguyên centralized

**Structure**:
```
i18n/locales/
├── ja.json     # All translations
└── en.json
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐ | Acceptable — i18n có thể centralized |
| Maintainability | ⭐⭐⭐⭐ | Single file, dễ overview |
| Concurrent Dev | ⭐⭐ | Merge conflicts khi team lớn |
| Scalability | ⭐⭐ | File sẽ rất lớn (~500+ keys cho 5 units) |
| Simplicity | ⭐⭐⭐⭐⭐ | Đơn giản nhất, next-intl default |

**Ưu điểm**: Đơn giản, next-intl default pattern  
**Nhược điểm**: File lớn, merge conflicts

### Option B: Per-feature translation files

**Structure**:
```
features/auth/login-email/locales/ja.json
features/auth/register/locales/ja.json
...
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Feature owns translations |
| Maintainability | ⭐⭐ | Fragmentation, khó overview |
| Concurrent Dev | ⭐⭐⭐⭐⭐ | Zero conflicts |
| Scalability | ⭐⭐⭐⭐ | Isolated files |
| Simplicity | ⭐ | Complex setup, next-intl không support native |

**Ưu điểm**: Feature isolation  
**Nhược điểm**: next-intl không support per-feature translations native, cần custom loader

### Option C: Hybrid — Centralized với namespace organization

**Structure**:
```
i18n/locales/ja.json
{
  "auth": { "login": {...}, "register": {...} },
  "profile": { "edit": {...}, "photos": {...} },
  "common": { "loading": "...", "error": "..." }
}
```

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐ | Organized by domain |
| Maintainability | ⭐⭐⭐⭐⭐ | Clear namespaces, dễ tìm |
| Concurrent Dev | ⭐⭐⭐ | Conflicts ít hơn Option A |
| Scalability | ⭐⭐⭐⭐ | Namespaces scale tốt |
| Simplicity | ⭐⭐⭐⭐ | next-intl native support |

**Ưu điểm**: Balance giữa organization và simplicity  
**Nhược điểm**: Vẫn 1 file lớn

### 🏆 Recommendation: **Option C — Hybrid Namespace**

**Lý do**:
- **next-intl native**: Không cần custom loader
- **Organization**: Namespaces theo domain (auth, profile, livestream, chat, coin)
- **Maintainability**: Dễ tìm keys, clear structure
- **Scalability**: 500 keys với namespaces vẫn manageable

---

## Question 10: Reusable Assets from Old Codebase

**Context**: Approach là "recreate from scratch". Assets nào từ old codebase có thể reuse?

### Option A: Reuse tất cả assets có thể

**Assets có thể reuse**:
- shadcn/ui components (`src/components/ui/`) — đã compliant
- Translation files (`i18n/locales/*.json`) — chỉ cần organize namespaces
- Test scenarios (không phải test code) — proven edge cases
- Type definitions (nếu match backend contracts)
- Utility functions (validation, formatting) — nếu business-agnostic

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐ | Assets move vào đúng layers |
| Maintainability | ⭐⭐⭐⭐⭐ | Preserve proven code |
| Concurrent Dev | ⭐⭐⭐⭐ | Less work, faster |
| Scalability | ⭐⭐⭐⭐ | Proven assets |
| Simplicity | ⭐⭐⭐⭐⭐ | Minimize recreation effort |

**Ưu điểm**: 
- Faster — không recreate những gì đã work
- Preserve proven logic
- Less testing effort

**Nhược điểm**: 
- Có thể carry over bad patterns
- Cần review để ensure FSD compliance

### Option B: Recreate toàn bộ — Fresh start

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Everything designed for FSD |
| Maintainability | ⭐⭐⭐⭐ | Clean code, no legacy patterns |
| Concurrent Dev | ⭐⭐⭐ | More work |
| Scalability | ⭐⭐⭐⭐⭐ | Clean slate |
| Simplicity | ⭐⭐ | Most effort |

**Ưu điểm**: 
- No legacy patterns
- Consistent code style
- Opportunity to improve

**Nhược điểm**: 
- More effort
- Risk missing proven logic
- More testing needed

### 🏆 Recommendation: **Option A — Reuse Assets**

**Lý do**:
- **Efficiency**: shadcn/ui components, translations, utilities đã work — không cần recreate
- **Quality**: Test scenarios proven — reuse để ensure coverage
- **Focus**: Recreate architecture và business logic, reuse infrastructure
- **Trade-off**: Review assets để ensure FSD compliance

**Reuse list**:
- ✅ `src/components/ui/*` → move to `src/shared/ui/`
- ✅ `i18n/locales/*.json` → organize namespaces, keep content
- ✅ Test scenarios → extract và apply to new tests
- ✅ Validation schemas → move to `shared/lib/validation/`
- ❌ Components (`AuthForm`, `OtpInput`) → recreate với FSD structure
- ❌ Stores → recreate trong entities/
- ❌ API clients → recreate trong entities/*/api/

---

## Question 11: Package.json Dependencies

**Context**: `package.json` dependencies — keep, update, hay recreate?

### Option A: Keep existing dependencies — Minimal changes

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Dependencies không ảnh hưởng FSD |
| Maintainability | ⭐⭐⭐⭐ | Proven versions, no breaking changes |
| Concurrent Dev | ⭐⭐⭐⭐⭐ | No dependency conflicts |
| Scalability | ⭐⭐⭐⭐ | Works |
| Simplicity | ⭐⭐⭐⭐⭐ | No changes needed |

**Ưu điểm**: 
- No breaking changes
- Proven versions
- Fast

**Nhược điểm**: 
- Có thể miss security updates
- Có thể có outdated packages

### Option B: Update to latest versions

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Dependencies không ảnh hưởng FSD |
| Maintainability | ⭐⭐⭐⭐⭐ | Latest features, security patches |
| Concurrent Dev | ⭐⭐⭐ | Breaking changes có thể cause issues |
| Scalability | ⭐⭐⭐⭐⭐ | Latest versions |
| Simplicity | ⭐⭐ | Cần handle breaking changes |

**Ưu điểm**: 
- Latest features
- Security patches
- Better performance

**Nhược điểm**: 
- Breaking changes risk
- More testing needed
- Migration guides needed

### Option C: Audit và selective update

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Dependencies không ảnh hưởng FSD |
| Maintainability | ⭐⭐⭐⭐⭐ | Balance safety và updates |
| Concurrent Dev | ⭐⭐⭐⭐ | Controlled updates |
| Scalability | ⭐⭐⭐⭐⭐ | Best balance |
| Simplicity | ⭐⭐⭐⭐ | Reasonable effort |

**Ưu điểm**: 
- Update critical packages (security, major features)
- Keep stable packages
- Controlled risk

**Nhược điểm**: 
- Cần audit effort

### 🏆 Recommendation: **Option C — Audit và Selective Update**

**Lý do**:
- **Security**: Update packages có security vulnerabilities
- **Stability**: Keep stable packages (Next.js, React)
- **Features**: Update packages có useful new features
- **Risk control**: Avoid unnecessary breaking changes

**Priority updates**:
- 🔴 Security vulnerabilities (check `npm audit`)
- 🟡 Major features needed (check changelogs)
- 🟢 Keep stable (Next.js, React, TypeScript)

---

## Question 12: Admin Dashboard Scope

**Context**: Admin dashboard (`app/frontend/admin/`) có recreate cùng lúc không?

### Option A: Refactor cả PWA và Admin

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Consistent architecture |
| Maintainability | ⭐⭐⭐⭐⭐ | Uniform structure |
| Concurrent Dev | ⭐⭐ | Large scope, coordination needed |
| Scalability | ⭐⭐⭐⭐⭐ | Both apps follow FSD |
| Simplicity | ⭐⭐ | Large effort |

**Ưu điểm**: Consistent architecture across apps  
**Nhược điểm**: Large scope, high effort

### Option B: Chỉ PWA, Admin recreate sau

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐ | PWA compliant, Admin later |
| Maintainability | ⭐⭐⭐⭐ | Focus on PWA first |
| Concurrent Dev | ⭐⭐⭐⭐⭐ | Smaller scope, easier coordination |
| Scalability | ⭐⭐⭐⭐ | PWA ready, Admin migrate later |
| Simplicity | ⭐⭐⭐⭐⭐ | Focused scope |

**Ưu điểm**: Focused scope, manageable effort  
**Nhược điểm**: Inconsistent architecture temporarily

### Option C: Chỉ shared components và design system

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐ | Partial compliance |
| Maintainability | ⭐⭐⭐ | Shared components consistent |
| Concurrent Dev | ⭐⭐⭐⭐ | Minimal scope |
| Scalability | ⭐⭐⭐ | Partial solution |
| Simplicity | ⭐⭐⭐⭐ | Minimal effort |

**Ưu điểm**: Minimal effort  
**Nhược điểm**: Incomplete solution

### 🏆 Recommendation: **Option B — Chỉ PWA**

**Lý do**:
- **Focus**: PWA là user-facing, priority cao hơn Admin
- **Scope management**: Admin có 1 screen (login) — recreate riêng sau dễ hơn
- **Risk reduction**: Smaller scope = lower risk
- **Future**: Admin recreate có thể reuse patterns từ PWA

---

## Question 13: Old Codebase Handling

**Context**: Sau khi recreate xong, xử lý old codebase như thế nào?

### Option A: Delete old code ngay

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Clean FSD structure |
| Maintainability | ⭐⭐⭐⭐⭐ | No confusion, single source of truth |
| Concurrent Dev | ⭐⭐⭐ | Cần coordinate timing |
| Scalability | ⭐⭐⭐⭐⭐ | Clean slate |
| Simplicity | ⭐⭐⭐⭐⭐ | Straightforward |

**Ưu điểm**: 
- Clean workspace
- No confusion về code nào đang active
- Force complete migration

**Nhược điểm**: 
- Không có fallback nếu new code có issues
- Cần confident về new code quality

### Option B: Keep old code trong backup branch

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Main branch clean |
| Maintainability | ⭐⭐⭐⭐⭐ | Safety net, có thể reference |
| Concurrent Dev | ⭐⭐⭐⭐⭐ | No disruption |
| Scalability | ⭐⭐⭐⭐⭐ | Best practice |
| Simplicity | ⭐⭐⭐⭐⭐ | Git handles it |

**Ưu điểm**: 
- Safety net — có thể reference old code
- Git history preserved
- Can cherry-pick logic nếu cần

**Nhược điểm**: 
- None significant

### Option C: Archive old code trong `_archive/` folder

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐ | Old code vẫn trong workspace |
| Maintainability | ⭐⭐ | Confusing, 2 versions trong workspace |
| Concurrent Dev | ⭐⭐ | Clutter |
| Scalability | ⭐⭐ | Waste space |
| Simplicity | ⭐⭐ | Extra folder |

**Ưu điểm**: Easy reference  
**Nhược điểm**: Clutter workspace, confusing

### 🏆 Recommendation: **Option B — Backup Branch**

**Lý do**:
- **Safety**: Git branch là proper way để preserve old code
- **Clean workspace**: Main branch chỉ có new FSD code
- **Reference**: Có thể reference old code nếu cần
- **Best practice**: Standard Git workflow

**Process**:
```bash
# Trước khi recreate
git checkout -b backup/pre-fsd-refactor
git push origin backup/pre-fsd-refactor

# Recreate trên main branch
git checkout main
# ... recreate code ...

# Old code preserved trong backup branch
```

---

## Question 14: Code Generation Approach

**Context**: Sau khi có design, generate code như thế nào?

### Option A: AI generate toàn bộ code mới

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | Clean FSD từ đầu |
| Maintainability | ⭐⭐⭐⭐⭐ | Consistent code style |
| Concurrent Dev | ⭐⭐⭐ | Cần review toàn bộ |
| Scalability | ⭐⭐⭐⭐⭐ | Clean architecture |
| Simplicity | ⭐⭐⭐⭐⭐ | Fastest approach |

**Ưu điểm**: 
- Clean code, consistent style
- Fastest approach
- No legacy patterns

**Nhược điểm**: 
- Cần review kỹ generated code
- Có thể miss business logic details

### Option B: AI generate migration scripts

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐ | Depends on script quality |
| Maintainability | ⭐⭐⭐ | Scripts có thể có bugs |
| Concurrent Dev | ⭐⭐ | Scripts phức tạp |
| Scalability | ⭐⭐⭐ | Works nhưng không ideal |
| Simplicity | ⭐⭐ | Complex scripts |

**Ưu điểm**: Automated transformation  
**Nhược điểm**: Scripts phức tạp, khó debug

### Option C: Manual migration với AI guidance

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐ | Depends on manual work |
| Maintainability | ⭐⭐⭐⭐ | Full control |
| Concurrent Dev | ⭐⭐ | Slow, manual work |
| Scalability | ⭐⭐⭐ | Time-consuming |
| Simplicity | ⭐⭐ | Most effort |

**Ưu điểm**: Full control, understand every change  
**Nhược điểm**: Slow, labor-intensive

### Option D: Hybrid — AI structure, manual logic

| Tiêu chí | Score | Rationale |
|---|---|---|
| FSD Compliance | ⭐⭐⭐⭐⭐ | AI ensures structure |
| Maintainability | ⭐⭐⭐⭐⭐ | Best of both worlds |
| Concurrent Dev | ⭐⭐⭐⭐ | AI generates, team reviews |
| Scalability | ⭐⭐⭐⭐⭐ | Balanced approach |
| Simplicity | ⭐⭐⭐⭐ | Reasonable effort |

**Ưu điểm**: 
- AI generates FSD structure + boilerplate
- Manual migrate business logic
- Review và adjust

**Nhược điểm**: Cần coordinate

### 🏆 Recommendation: **Option A — AI Generate Toàn Bộ**

**Lý do**:
- **Speed**: Fastest approach, generate clean FSD code
- **Consistency**: AI ensures consistent patterns across all features
- **Existing logic simple**: Unit 1 logic đơn giản (forms, API calls) — AI có thể handle
- **Review process**: Generated code sẽ được review thoroughly với incremental approach
- **Trade-off**: Cần review kỹ, nhưng với incremental approach (Question 3) thì manageable

**Process**:
1. AI generates feature slice (structure + code + tests)
2. Review generated code thoroughly
3. Test feature
4. Adjust if needed
5. Move to next feature

---

## Summary: Recommended Architecture Decisions

| Question | Recommendation | Rationale |
|---|---|---|
| **Q1: Composition Layer** | **A) Tạo `src/views/` riêng** | FSD compliance, future-proof cho complex screens. ⚠️ Dùng `views/` thay `pages/` — Next.js conflict |
| **Q2: Widgets Layer** | **A) Tạo `src/widgets/` ngay** | Đầy đủ FSD layers, chuẩn bị cho Unit 2+ |
| **Q3: Recreate Strategy** | **B) Incremental** | Low risk, concurrent dev, test từng feature |
| **Q4: AuthForm** | **A) Duplicate LoginForm/RegisterForm** | Feature isolation, UI sẽ diverge |
| **Q5: OtpInput** | **B) Shared UI primitive** | Reusable, no duplication, semantic correct |
| **Q6: API Clients** | **A) entities/{domain}/api/** | Domain-driven, scalability (84 endpoints) |
| **Q7: Zustand Stores** | **B) entities/{domain}/model/** | Domain state, natural sharing |
| **Q8: Test Strategy** | **C) Hybrid — Reuse scenarios, recreate code** | Preserve coverage, clean implementation |
| **Q9: i18n** | **C) Hybrid namespace** | next-intl native, organized, scalable |
| **Q10: Reusable Assets** | **A) Reuse assets** | Efficiency, preserve proven code |
| **Q11: Package.json** | **C) Audit và selective update** | Security + stability balance |
| **Q12: Admin Scope** | **B) Chỉ PWA** | Focus, manageable scope |
| **Q13: Old Codebase** | **B) Backup branch** | Safety net, clean workspace |
| **Q14: Code Generation** | **A) AI generate toàn bộ** | Speed, consistency, với review process |

---

## Target FSD Structure (Final)

```
app/frontend/pwa/src/
├── app/                              # Next.js App Router (KHÔNG THAY ĐỔI)
│   └── [locale]/
│       ├── login/page.tsx            → import from @/pages/login
│       ├── register/page.tsx         → import from @/pages/register
│       └── ...
│
├── pages/                            # 🆕 Composition layer
│   ├── login/ui/LoginPage.tsx
│   ├── register/ui/RegisterPage.tsx
│   ├── verify-email/ui/VerifyEmailPage.tsx
│   ├── verify-phone/ui/VerifyPhonePage.tsx
│   ├── reset-password/ui/ResetPasswordPage.tsx
│   ├── profile-view/ui/ProfileViewPage.tsx
│   ├── profile-edit/ui/ProfileEditPage.tsx
│   └── profile-photos/ui/ProfilePhotosPage.tsx
│
├── widgets/                          # 🆕 Composite UI blocks
│   ├── auth-card/                    # Logo + card wrapper cho auth screens
│   └── profile-header/               # Avatar + display name + badges
│
├── features/                         # 🆕 User interactions
│   ├── auth/
│   │   ├── login-email/
│   │   │   ├── ui/LoginForm.tsx
│   │   │   ├── model/useLogin.ts
│   │   │   └── index.ts
│   │   ├── login-line/
│   │   ├── register/
│   │   ├── verify-email/
│   │   ├── verify-phone/
│   │   ├── reset-password/
│   │   └── logout/
│   └── profile/
│       ├── edit-profile/
│       └── upload-photos/
│
├── entities/                         # 🆕 Domain models
│   ├── user/
│   │   ├── model/
│   │   │   ├── types.ts              # User, AuthTokens types
│   │   │   └── userStore.ts          # Zustand store
│   │   ├── api/
│   │   │   └── user.queries.ts       # API calls
│   │   ├── ui/
│   │   │   └── UserAvatar.tsx        # Presentational
│   │   └── index.ts
│   └── profile/
│       ├── model/
│       │   ├── types.ts
│       │   └── profileStore.ts
│       ├── api/
│       │   └── profile.queries.ts
│       ├── ui/
│       │   └── ProfileCard.tsx
│       └── index.ts
│
├── shared/                           # ♻️ REUSE assets từ old codebase
│   ├── ui/                           # shadcn/ui (REUSE — đã compliant)
│   │   ├── button.tsx
│   │   ├── input.tsx
│   │   ├── otp-input.tsx             # 🆕 Move OtpInput vào đây
│   │   └── ...
│   ├── lib/
│   │   ├── api/
│   │   │   └── client.ts             # REUSE Axios instance
│   │   ├── validation/               # 🆕 Extract từ old code
│   │   ├── format/                   # 🆕 Date, currency formatters
│   │   └── utils.ts                  # REUSE utilities
│   ├── config/
│   │   ├── env.ts                    # 🆕 Type-safe env vars
│   │   ├── constants.ts
│   │   └── routes.ts                 # 🆕 Type-safe routes
│   ├── hooks/
│   │   ├── useMediaQuery.ts
│   │   └── useDebounce.ts
│   └── types/
│       ├── api.ts                    # Generic API types
│       └── common.ts
│
├── i18n/                             # ♻️ REUSE với namespace organization
│   └── locales/
│       ├── ja.json                   # Organize by domain namespaces
│       └── en.json
│
└── __tests__/                        # 🆕 Recreate với test scenarios từ old tests
    ├── features/
    │   └── auth/
    │       └── login-email/
    │           └── LoginForm.test.tsx
    ├── entities/
    │   └── user/
    │       └── userStore.test.ts
    └── shared/
        └── ui/
            └── otp-input.test.tsx
```

---

## Recreate Sequence (Incremental Approach)

### Phase 0: Backup & Setup (Day 1 Morning)
- [ ] Create backup branch: `git checkout -b backup/pre-fsd-refactor`
- [ ] Push backup: `git push origin backup/pre-fsd-refactor`
- [ ] Return to main: `git checkout main`
- [ ] Tạo folders: `src/views/`, `src/widgets/`, `src/features/`, `src/entities/` (⚠️ KHÔNG dùng `src/pages/` — Next.js conflict)
- [ ] Setup path aliases trong `tsconfig.json`
- [ ] Update ESLint với `eslint-plugin-boundaries`

### Phase 1: Reuse Assets (Day 1 Afternoon)
- [ ] Keep `src/components/ui/` → rename to `src/shared/ui/`
- [ ] Keep `i18n/locales/*.json` → organize namespaces
- [ ] Keep `lib/api/client.ts` → move to `shared/lib/api/client.ts`
- [ ] Extract validation schemas → `shared/lib/validation/`
- [ ] Extract utilities → `shared/lib/utils.ts`

### Phase 2: Generate Entities (Day 1-2)
- [ ] Generate `entities/user/` — types, userStore, user.queries, UserAvatar
- [ ] Generate `entities/profile/` — types, profileStore, profile.queries, ProfileCard
- [ ] Generate tests: `userStore.test.ts`, `profileStore.test.ts`
- [ ] Test entities isolation

### Phase 3: Generate Features — Auth (Day 2-3)
- [ ] Generate `features/auth/login-email/` — LoginForm, useLogin hook, tests
- [ ] Generate `features/auth/register/` — RegisterForm, useRegister hook, tests
- [ ] Generate `features/auth/verify-email/` — VerifyEmailForm, useVerifyEmail, tests
- [ ] Generate `features/auth/verify-phone/` — VerifyPhoneForm, useVerifyPhone, tests
- [ ] Generate `features/auth/reset-password/` — ResetPasswordForm, useResetPassword, tests
- [ ] Test all auth features

### Phase 4: Generate Features — Profile (Day 3-4)
- [ ] Generate `features/profile/edit-profile/` — EditProfileForm, useEditProfile, tests
- [ ] Generate `features/profile/upload-photos/` — PhotoUploader, useUploadPhotos, tests
- [ ] Test profile features

### Phase 5: Generate Widgets (Day 4)
- [ ] Generate `widgets/auth-card/` — AuthCard wrapper với branding
- [ ] Generate `widgets/profile-header/` — ProfileHeader composite
- [ ] Test widgets

### Phase 6: Generate Pages (Day 4-5)
- [ ] Generate `pages/login/` — LoginPage composition
- [ ] Generate `pages/register/` — RegisterPage composition
- [ ] Generate `pages/verify-email/` — VerifyEmailPage
- [ ] Generate `pages/verify-phone/` — VerifyPhonePage
- [ ] Generate `pages/reset-password/` — ResetPasswordPage
- [ ] Generate `pages/profile-view/` — ProfileViewPage
- [ ] Generate `pages/profile-edit/` — ProfileEditPage
- [ ] Generate `pages/profile-photos/` — ProfilePhotosPage
- [ ] Update `app/[locale]/**/page.tsx` imports
- [ ] Test all pages

### Phase 7: Cleanup (Day 5)
- [ ] Delete old `src/components/` (except ui/ — đã move)
- [ ] Delete old `src/store/`
- [ ] Delete old `src/lib/api/` (except client.ts — đã move)
- [ ] Run full test suite
- [ ] Update documentation
- [ ] Final review

**Estimated effort**: 5 days (1 developer) hoặc 2-3 days (2 developers parallel)

---

## Risk Assessment

### High Risk Areas

| Risk | Impact | Mitigation |
|---|---|---|
| **Business logic loss** | Features broken | Thorough review, preserve logic từ old code |
| **Type errors** | Build failures | TypeScript strict mode, check diagnostics |
| **Runtime errors** | App crashes | Test thoroughly mỗi feature sau generate |
| **Test coverage loss** | Quality issues | Reuse test scenarios từ old tests |

### Medium Risk Areas

| Risk | Impact | Mitigation |
|---|---|---|
| **FSD learning curve** | Slower development | Clear documentation, examples |
| **Over-engineering** | Unnecessary complexity | Keep Unit 1 simple, expand later |
| **Generated code quality** | Bugs, inconsistencies | Thorough review process |

### Low Risk Areas

| Risk | Impact | Mitigation |
|---|---|---|
| **UI/UX changes** | None | Preserve existing UI design |
| **API contract changes** | None | No backend changes |
| **i18n keys** | None | Reuse existing keys |
| **Dependencies** | Low | Selective updates only |

---

## Success Criteria

### Must Have
- [ ] Tất cả 8 screens work như cũ
- [ ] Tất cả tests pass
- [ ] No TypeScript errors
- [ ] FSD structure đầy đủ (pages, widgets, features, entities, shared)
- [ ] Import paths follow FSD rules
- [ ] ESLint boundaries rules enforced

### Should Have
- [ ] Code coverage maintained (≥80%)
- [ ] Performance không giảm
- [ ] Bundle size không tăng đáng kể
- [ ] Documentation updated

### Nice to Have
- [ ] Improved code organization
- [ ] Better type safety
- [ ] Cleaner component APIs
- [ ] Improved test quality

---

## Conclusion

**Recommended approach**: 
- **FSD-compliant architecture** với đầy đủ layers (pages, widgets, features, entities, shared)
- **Incremental recreation** (7 phases, ~5 days)
- **AI-generated code** với thorough review process
- **Reuse proven assets** (shadcn/ui, translations, utilities, test scenarios)
- **Backup branch** để preserve old code
- **PWA only** (Admin recreate riêng sau)

**Key trade-offs accepted**:
- Phức tạp hơn flat structure → Đổi lại: scalability cho 5 units
- Effort cao hơn quick fix → Đổi lại: architecture đúng từ đầu
- Learning curve FSD → Đổi lại: maintainability và concurrent development
- Recreation effort → Đổi lại: clean code, no legacy patterns

**Expected outcome**: 
- Clean FSD architecture
- Consistent code patterns
- Ready for Unit 2-5 expansion
- Team-friendly structure
- Modern, optimized codebase

