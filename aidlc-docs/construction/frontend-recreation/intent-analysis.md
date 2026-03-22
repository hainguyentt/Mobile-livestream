# Intent Analysis — Frontend Unit 1 Recreate

**Timestamp**: 2026-03-22T10:50:00Z
**Updated**: 2026-03-22T11:25:00Z
**Request Type**: Complete Recreation + Architecture Migration
**Scope**: Frontend PWA + Admin (Unit 1 screens only)
**Complexity**: Moderate to Complex
**Approach**: Recreate from scratch (không phải refactor/migrate)

---

## User Request Summary

**Original Request**:
> "Tôi muốn thực hiện lại toàn bộ phần Front-end theo design ui-ux thống nhất nên cho phép refactor/recreate lại toàn bộ source code của Unit 1. Lý do: execution-plan đã thiếu phần UI/UX specs & design. Mục tiêu **QUAN TRỌNG NHẤT**: cấu trúc source code hiện đại tối ưu, hiện đại; đồng nhất giữa các chức năng, màn hình."

**Interpreted Intent**:
- **Recreate toàn bộ** frontend Unit 1 từ đầu (không phải refactor/migrate)
- Áp dụng Feature-Sliced Design (FSD) architecture từ đầu
- Áp dụng UI/UX design system thống nhất (đã có docs)
- Đảm bảo consistency tuyệt đối giữa các screens và components
- Modernize code structure để dễ maintain và scale
- Reuse proven assets (shadcn/ui, translations, utilities) nhưng recreate business logic

**Key Decision**: 
- ✅ **Recreate from scratch** — Generate clean FSD code
- ❌ **NOT refactor** — Không migrate/transform existing code
- ✅ **Reuse assets** — shadcn/ui, translations, test scenarios, utilities
- ✅ **Backup old code** — Git branch để preserve

---

## Current State Analysis

### Existing Frontend Structure (Flat — Will be replaced)

**PWA** (`app/frontend/pwa/src/`):
```
├── app/[locale]/          # Next.js App Router pages (8 screens)
├── components/            # Flat components (AuthForm, OtpInput, PhotoUploader, LanguageSwitcher)
├── components/ui/         # shadcn/ui components (REUSE)
├── store/                 # Zustand stores (authStore, profileStore) — RECREATE
├── lib/api/               # API clients (auth.ts, profiles.ts) — RECREATE
├── i18n/locales/          # Translation files (REUSE với namespace organization)
└── __tests__/             # Tests (RECREATE với scenarios từ old tests)
```

**Admin** (`app/frontend/admin/src/`):
```
├── app/                   # 1 screen: login
├── components/ui/         # shadcn/ui components (REUSE)
└── lib/api/               # API client (RECREATE)
```

### Target FSD Structure (Clean recreation)

**PWA** (`app/frontend/pwa/src/`):
```
├── app/                   # Next.js App Router — ROUTING ONLY (minimal changes)
│   └── [locale]/
│       ├── login/page.tsx            → import from @/pages/login
│       ├── register/page.tsx         → import from @/pages/register
│       └── ...
│
├── pages/                 # 🆕 Composition layer (GENERATE)
│   ├── login/ui/LoginPage.tsx
│   ├── register/ui/RegisterPage.tsx
│   ├── verify-email/ui/VerifyEmailPage.tsx
│   ├── verify-phone/ui/VerifyPhonePage.tsx
│   ├── reset-password/ui/ResetPasswordPage.tsx
│   ├── profile-view/ui/ProfileViewPage.tsx
│   ├── profile-edit/ui/ProfileEditPage.tsx
│   └── profile-photos/ui/ProfilePhotosPage.tsx
│
├── widgets/               # 🆕 Composite UI blocks (GENERATE)
│   ├── auth-card/         # Logo + card wrapper
│   └── profile-header/    # Avatar + display name + badges
│
├── features/              # 🆕 User interactions (GENERATE)
│   ├── auth/
│   │   ├── login-email/
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
├── entities/              # 🆕 Domain models (GENERATE)
│   ├── user/
│   │   ├── model/         # types.ts, userStore.ts
│   │   ├── api/           # user.queries.ts
│   │   ├── ui/            # UserAvatar.tsx
│   │   └── index.ts
│   └── profile/
│       ├── model/         # types.ts, profileStore.ts
│       ├── api/           # profile.queries.ts
│       ├── ui/            # ProfileCard.tsx
│       └── index.ts
│
├── shared/                # ♻️ REUSE + organize
│   ├── ui/                # shadcn/ui (REUSE từ components/ui/)
│   ├── lib/
│   │   ├── api/client.ts  # REUSE Axios instance
│   │   ├── validation/    # GENERATE (extract từ old code)
│   │   ├── format/        # GENERATE
│   │   └── utils.ts       # REUSE utilities
│   ├── config/            # GENERATE (env.ts, constants.ts, routes.ts)
│   ├── hooks/             # GENERATE (useMediaQuery, useDebounce)
│   └── types/             # GENERATE (api.ts, common.ts)
│
├── i18n/locales/          # ♻️ REUSE với namespace organization
│   ├── ja.json            # Organize by domain: auth, profile, common
│   └── en.json
│
└── __tests__/             # 🆕 RECREATE với test scenarios từ old tests
    ├── features/
    ├── entities/
    └── shared/
```

**Admin** (`app/frontend/admin/src/`):
```
├── app/                   # Next.js App Router (minimal changes)
│   └── login/page.tsx     → import from @/pages/login
│
├── pages/                 # 🆕 Composition layer
│   └── login/ui/AdminLoginPage.tsx
│
├── features/              # 🆕 User interactions
│   └── auth/
│       └── admin-login/
│
├── entities/              # 🆕 Domain models
│   └── admin/
│       ├── model/
│       ├── api/
│       └── ui/
│
└── shared/                # ♻️ REUSE
    ├── ui/                # shadcn/ui (REUSE)
    └── lib/
```

---

## Scope Boundaries

### In Scope — PWA (Priority 1)
- **8 screens**: Login, Register, Verify Email, Verify Phone, Reset Password, Profile View, Profile Edit, Photo Upload
- **Components**: Recreate AuthForm → LoginForm + RegisterForm, recreate PhotoUploader, move OtpInput to shared/ui
- **Stores**: Recreate authStore → entities/user/model/userStore.ts, profileStore → entities/profile/model/profileStore.ts
- **API clients**: Recreate auth.ts → entities/user/api/user.queries.ts, profiles.ts → entities/profile/api/profile.queries.ts
- **Tests**: Recreate với test scenarios từ old tests
- **Assets to reuse**: shadcn/ui components, translations (organize namespaces), Axios client, utilities

### In Scope — Admin (Priority 2)
- **1 screen**: Admin Login
- **Structure**: Apply FSD architecture (pages, features, entities, shared)
- **Assets to reuse**: shadcn/ui components

### Out of Scope
- Backend code (no changes)
- New features (chỉ recreate existing functionality)
- i18n translation content (reuse, chỉ organize namespaces)
- Dependencies (selective updates only)

---

## Key Requirements (Updated for Recreation)

### FR-01: Clean FSD Architecture
- Generate clean FSD structure từ đầu: app/ → pages/ → widgets/ → features/ → entities/ → shared/
- Enforce dependency rules với eslint-plugin-boundaries
- Mỗi slice có `index.ts` public API
- No legacy patterns, no hybrid state

### FR-02: Code Consistency
- Tất cả screens follow identical pattern
- Tất cả features follow identical structure
- Naming conventions thống nhất 100%
- Code style consistent (AI-generated)

### FR-03: Preserve Functionality
- Tất cả existing features work như cũ
- API integration không thay đổi
- User experience không thay đổi
- Test coverage maintained (reuse scenarios)

### FR-04: Asset Reuse Strategy
- Reuse shadcn/ui components (đã FSD-compliant)
- Reuse translations (organize namespaces)
- Reuse Axios client instance
- Reuse utilities (validation, formatting)
- Reuse test scenarios (recreate test code)

### NFR-01: Maintainability
- Clear separation of concerns
- Easy to locate code by feature
- Minimal coupling between features
- Self-documenting code structure

### NFR-02: Scalability
- Dễ thêm features mới (Unit 2-5)
- Parallel development friendly
- Clear ownership boundaries
- Architecture ready cho 84+ endpoints

### NFR-03: Quality Assurance
- Test coverage ≥80%
- No TypeScript errors
- ESLint boundaries rules enforced
- Thorough review process

### NFR-04: Safety
- Backup old code trong Git branch
- Incremental recreation (test từng feature)
- Rollback capability
- Preserve Git history

---

## Ambiguities & Questions (14 Questions)

1. **Composition Layer**: Có cần tạo `src/pages/` layer riêng hay để logic composition trong `app/**/page.tsx`?
2. **Widgets Layer**: Unit 1 có cần widgets layer không? (screens hiện tại khá đơn giản)
3. **Recreate Strategy**: Big bang hay incremental recreation?
4. **AuthForm Classification**: Duplicate thành LoginForm/RegisterForm hay giữ shared?
5. **OtpInput Classification**: Shared UI primitive hay duplicate per feature?
6. **API Client Location**: `entities/*/api/` hay `features/*/api/` hay `shared/lib/api/`?
7. **Zustand Store Location**: Per-feature, per-entity, hay centralized?
8. **Test Strategy**: Reuse tests, recreate tests, hay hybrid (reuse scenarios)?
9. **i18n Structure**: Centralized, per-feature, hay hybrid namespace?
10. **Reusable Assets**: Reuse tất cả, recreate tất cả, hay selective reuse?
11. **Package.json**: Keep, update all, hay selective update?
12. **Admin Scope**: Recreate cùng PWA hay riêng sau?
13. **Old Codebase Handling**: Delete ngay, backup branch, hay archive folder?
14. **Code Generation**: AI generate toàn bộ, migration scripts, manual, hay hybrid?

---

## Recreation Approach (Not Refactoring)

### What "Recreate from Scratch" Means

**Generate new code**:
- AI generates clean FSD structure
- No transformation/migration of existing code
- Fresh implementation based on requirements
- Consistent patterns across all features

**Reuse proven assets**:
- shadcn/ui components (already FSD-compliant)
- Translation content (organize namespaces)
- Axios client configuration
- Utility functions (validation, formatting)
- Test scenarios (recreate test implementation)

**Preserve old code**:
- Create backup branch before recreation
- Old code available for reference
- Can cherry-pick logic if needed
- Git history preserved

### Why Recreation Instead of Refactoring

**Pros**:
- ✅ Clean FSD architecture từ đầu
- ✅ Consistent code style (AI-generated)
- ✅ No legacy patterns or technical debt
- ✅ Faster than incremental refactoring
- ✅ Opportunity to improve implementation

**Cons**:
- ⚠️ Risk missing business logic details
- ⚠️ Cần thorough review process
- ⚠️ More testing effort
- ⚠️ Cần understand existing logic trước khi recreate

**Mitigation**:
- Incremental recreation (test từng feature)
- Thorough review của generated code
- Reference old code khi cần
- Reuse test scenarios để ensure coverage

---

## Expected Outcomes

### Architecture
- Clean FSD structure với đầy đủ layers (pages, widgets, features, entities, shared)
- Dependency rules enforced (eslint-plugin-boundaries)
- Consistent patterns across all features
- Ready for Unit 2-5 expansion

### Code Quality
- TypeScript strict mode, no errors
- Consistent naming conventions
- Self-documenting structure
- Modern React patterns (hooks, Server Components)

### Maintainability
- Easy to locate code by feature
- Clear ownership boundaries
- Minimal coupling
- Team-friendly structure

### Testing
- Test coverage ≥80%
- Tests follow FSD structure
- Proven test scenarios preserved
- Fresh test implementation

### Safety
- Old code preserved trong backup branch
- Incremental recreation (low risk)
- Rollback capability
- Git history intact

---

## Success Criteria

### Must Have
- [ ] Tất cả 8 PWA screens work như cũ
- [ ] Admin login screen work như cũ
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
- [ ] Consistent code style

### Nice to Have
- [ ] Improved code organization
- [ ] Better type safety
- [ ] Cleaner component APIs
- [ ] Improved test quality
- [ ] Better error handling

---

## Risk Assessment

### High Risk Areas
- **Business logic loss**: Thorough review, reference old code
- **Type errors**: TypeScript strict mode, check diagnostics
- **Runtime errors**: Test thoroughly mỗi feature
- **Test coverage loss**: Reuse test scenarios

### Medium Risk Areas
- **FSD learning curve**: Clear documentation, examples
- **Generated code quality**: Thorough review process
- **Over-engineering**: Keep Unit 1 simple

### Low Risk Areas
- **UI/UX changes**: Preserve existing design
- **API contracts**: No backend changes
- **Dependencies**: Selective updates only

---

## Estimated Effort

**PWA Recreation**: 5 days (1 developer) hoặc 2-3 days (2 developers parallel)
**Admin Recreation**: 1 day (1 developer)
**Total**: 6 days (1 developer) hoặc 3-4 days (2 developers)

**Phases**:
1. Backup & Setup (0.5 day)
2. Reuse Assets (0.5 day)
3. Generate Entities (1 day)
4. Generate Features — Auth (1.5 days)
5. Generate Features — Profile (1 day)
6. Generate Widgets (0.5 day)
7. Generate Pages (1 day)
8. Admin Recreation (1 day)
9. Cleanup & Final Review (0.5 day)

---

## Next Steps

1. ✅ Intent Analysis — Complete
2. ✅ Trade-off Analysis — Complete (14 questions)
3. ⏳ **Verification Questions** — Waiting for user answers
4. ⏳ Requirements Document — After answers received
5. ⏳ Functional Design — FSD structure details
6. ⏳ Code Generation — Incremental recreation

