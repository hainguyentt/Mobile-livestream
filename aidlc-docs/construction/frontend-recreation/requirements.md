# Change Request — Frontend Unit 1 Recreation với FSD Architecture

**Loại**: Architecture Migration + Complete Recreation
**Ngày tạo**: 2026-03-22
**Phạm vi**: Frontend Admin (Priority 1) → PWA (Priority 2)
**Độ phức tạp**: Moderate to Complex
**Phương pháp**: Recreate from scratch với FSD architecture
**Liên quan**: Requirements chính (requirements.md) - Unit 1 Core Foundation

---

## Intent Analysis Summary

### User Request
Recreate toàn bộ frontend Unit 1 với:
- Feature-Sliced Design (FSD) architecture từ đầu
- UI/UX design system thống nhất (đã có docs)
- Consistency tuyệt đối giữa các screens và components
- Modern code structure để dễ maintain và scale
- Reuse proven assets nhưng recreate business logic

### Request Type
**Complete Recreation** - Không phải refactor/migrate, mà generate clean FSD code từ đầu

### Scope Estimate
**Admin**: 1 screen (login)
**PWA**: 8 screens (login, register, verify-email, verify-phone, reset-password, profile-view, profile-edit, photo-upload)

### Complexity Estimate
**Moderate to Complex** - Architecture migration với multiple screens, cần preserve functionality

### Execution Order (CRITICAL)
**Phase 1**: Admin Recreation (Priority 1) - Simple, test ground cho FSD
**Phase 2**: PWA Recreation (Priority 2) - Apply lessons learned từ Admin

---

## Functional Requirements

### FRR-01: Clean FSD Architecture

**Mô tả**: Generate clean FSD structure từ đầu với đầy đủ layers

**Acceptance Criteria**:
- Đầy đủ FSD layers: `app/` → `pages/` → `widgets/` → `features/` → `entities/` → `shared/`
- Dependency rules enforced với `eslint-plugin-boundaries`
- Mỗi slice có `index.ts` làm public API
- Import paths follow FSD rules: upper layers chỉ import từ lower layers
- No legacy patterns, no hybrid state

**Rationale**: Chuẩn bị architecture đúng từ đầu cho Unit 2-5

---

### FRR-02: Execution Order - Admin First, PWA Second

**Mô tả**: Recreate Admin trước (1 screen), sau đó PWA (8 screens)

**Acceptance Criteria**:
- **Phase 1 - Admin Recreation**:
  - Complete Admin với FSD structure
  - Test thoroughly Admin login flow
  - Document lessons learned
  - Approve Admin trước khi proceed to PWA
- **Phase 2 - PWA Recreation**:
  - Apply lessons learned từ Admin
  - Recreate 8 PWA screens với FSD structure
  - Test thoroughly từng feature

**Rationale**: Admin đơn giản - test ground cho FSD, lower risk

---

### FRR-03: Incremental Recreation Strategy

**Mô tả**: Recreate từng feature một, test ngay sau mỗi feature

**Admin Sequence**:
1. Admin Login feature

**PWA Sequence** (sau khi Admin complete):
1. Login Email
2. Register
3. Verify Email
4. Verify Phone
5. Reset Password
6. Profile View
7. Profile Edit
8. Photo Upload

**Rationale**: Low risk, quality control, parallel development friendly

---

### FRR-04: Component Architecture - Composition Layer

**Mô tả**: Tạo `src/views/` layer riêng, tách biệt routing và composition

**Structure**:
```
app/[locale]/login/page.tsx          → import from @/views/login
src/views/login/ui/LoginPage.tsx     → composition logic
```

**Acceptance Criteria**:
- `app/[locale]/**/page.tsx` chỉ import và export từ `@/pages/*`
- Composition logic trong `pages/*/ui/*Page.tsx`
- Test composition logic độc lập với routing

**Rationale**: FSD compliance, future-proof cho complex screens Unit 2+

---

### FRR-05: Component Architecture - Widgets Layer

**Mô tả**: Tạo `src/widgets/` layer cho composite UI blocks

**Admin Widgets**: Optional (skip nếu không cần)

**PWA Widgets**:
- `widgets/auth-card/` - Logo + card wrapper cho auth screens
- `widgets/profile-header/` - Avatar + display name + badges

**Acceptance Criteria**:
- Widgets compose multiple features/entities
- Reusable across pages
- Follow FSD dependency rules

**Rationale**: Chuẩn bị cho Unit 2+ (LivestreamViewer, ChatRoom, GiftPanel)

---

### FRR-06: Component Classification - AuthForm

**Mô tả**: Duplicate thành LoginForm và RegisterForm riêng

**Structure**:
```
features/auth/login-email/ui/LoginForm.tsx
features/auth/register/ui/RegisterForm.tsx
shared/lib/validation/authValidation.ts    # Shared validation logic
```

**Acceptance Criteria**:
- LoginForm và RegisterForm là separate components
- Extract shared validation logic vào `shared/lib/validation/`
- Each feature self-contained
- UI có thể diverge (Login có LINE button, Register có terms checkbox)

**Rationale**: Feature isolation, future divergence expected

---

### FRR-07: Component Classification - OtpInput

**Mô tả**: OtpInput là shared UI primitive trong `shared/ui/`

**Structure**:
```
shared/ui/otp-input.tsx                    # Reusable primitive
features/auth/verify-email/ui/             → import from shared
features/auth/verify-phone/ui/             → import from shared
```

**Acceptance Criteria**:
- OtpInput trong `shared/ui/otp-input.tsx`
- Reusable cho tất cả OTP scenarios
- No duplication

**Rationale**: OtpInput là UI primitive (như Button, Input), logic hoàn toàn giống nhau

---

### FRR-08: API Client Location - Entity-Based

**Mô tả**: API clients theo domain entity

**Structure**:
```
entities/user/api/user.queries.ts          # login, register, logout, sendOtp, verifyOtp
entities/profile/api/profile.queries.ts    # CRUD profile, photos
entities/admin/api/admin.queries.ts        # Admin operations
shared/lib/api/client.ts                   # Shared Axios instance
```

**Acceptance Criteria**:
- API clients trong `entities/*/api/`
- Domain-driven organization
- Shared Axios client trong `shared/lib/api/client.ts`
- Scale to 84+ endpoints future

**Rationale**: Domain clarity, scalability, FSD compliance

---

### FRR-09: Zustand Store Location - Entity-Based

**Mô tả**: Stores theo domain entity

**Structure**:
```
entities/user/model/userStore.ts           # Auth state (user, tokens, isLoading)
entities/profile/model/profileStore.ts     # Profile state
entities/admin/model/adminStore.ts         # Admin state
```

**Acceptance Criteria**:
- Stores trong `entities/*/model/`
- Multiple features share entity state naturally
- Clear ownership

**Rationale**: Domain state gắn với entity, natural sharing

---

### FRR-10: Test Strategy - Hybrid Approach

**Mô tả**: Reuse test scenarios, recreate test code

**Process**:
```
1. Read old test file
2. Extract test scenarios (describe, it blocks)
3. Generate new test code for new component APIs
4. Verify all scenarios covered
5. Run tests
```

**Acceptance Criteria**:
- Test scenarios preserved từ old tests
- Test code recreated cho new structure
- Test coverage maintained ≥80%
- All tests pass

**Rationale**: Preserve coverage, match new APIs, best quality

---

### FRR-11: i18n Structure - Hybrid Namespace

**Mô tả**: Centralized translation files với namespace organization

**Structure**:
```json
{
  "auth": { "login": {...}, "register": {...} },
  "profile": { "edit": {...}, "photos": {...} },
  "common": { "loading": "...", "error": "..." }
}
```

**Acceptance Criteria**:
- Organize keys theo domain namespaces
- Preserve existing translation content
- Both `ja.json` và `en.json` có cùng structure
- next-intl native support

**Rationale**: Balance organization và simplicity, scalable to 500+ keys

---

### FRR-12: Asset Reuse Strategy

**Mô tả**: Reuse proven assets, recreate business logic

**Assets to Reuse**:
- ✅ shadcn/ui components → move to `src/shared/ui/`
- ✅ Translation content → organize namespaces
- ✅ Axios client instance → move to `shared/lib/api/client.ts`
- ✅ Utility functions → move to `shared/lib/utils.ts`
- ✅ Validation schemas → extract to `shared/lib/validation/`
- ✅ Test scenarios → extract và apply to new tests

**Assets to Recreate**:
- ❌ Components (AuthForm, PhotoUploader) → recreate với FSD
- ❌ Stores (authStore, profileStore) → recreate trong entities/
- ❌ API clients (auth.ts, profiles.ts) → recreate trong entities/
- ❌ Page compositions → generate trong pages/

**Rationale**: Focus on architecture, preserve infrastructure

---

### FRR-13: Dependency Management

**Mô tả**: Audit và selective update critical packages

**Priority**:
- 🔴 Security vulnerabilities (must update)
- 🟡 Major features needed (consider update)
- 🟢 Stable packages (keep current)

**Acceptance Criteria**:
- Run `npm audit` check security
- Update packages có security issues
- Document breaking changes
- Test after updates

**Rationale**: Balance security và stability

---

### FRR-14: Old Codebase Handling

**Mô tả**: Archive old code trong `_archive/` folder

**Process**:
```
1. Move to app/frontend/_archive/pwa-old/ và admin-old/
2. Archive trước recreation
3. Reference during recreation if needed
4. Delete archive sau verification complete
```

**Rationale**: Safety net + easy reference

---

### FRR-15: Code Generation Approach

**Mô tả**: AI generate toàn bộ code mới với FSD structure

**Process**:
```
1. AI generates feature slice (structure + code + tests)
2. Review generated code
3. Test feature
4. Adjust if needed
5. Proceed to next feature
```

**Acceptance Criteria**:
- Consistent code style across features
- Follow coding standards
- Thorough review process
- Test sau mỗi feature

**Rationale**: Fastest, consistent, với incremental testing

---

## Non-Functional Requirements

### NFRR-01: FSD Compliance

**Mô tả**: Tuân thủ Feature-Sliced Design methodology

**Acceptance Criteria**:
- Dependency rules: `app/` → `pages/` → `widgets/` → `features/` → `entities/` → `shared/`
- No circular dependencies
- No cross-feature imports
- ESLint boundaries rules enforced
- Each slice exports via `index.ts`

**Verification**: `npm run lint` với eslint-plugin-boundaries

---

### NFRR-02: Maintainability

**Acceptance Criteria**:
- Clear separation of concerns
- Easy to locate code by feature
- Minimal coupling
- Self-documenting structure
- TypeScript strict mode, no errors

---

### NFRR-03: Scalability

**Acceptance Criteria**:
- Entity-based API clients scale to 84+ endpoints
- Entity-based stores scale to multiple domains
- Widget layer ready cho complex composites
- i18n namespaces scale to 500+ keys

---

### NFRR-04: Concurrent Development

**Acceptance Criteria**:
- Feature isolation
- Entity boundaries clear
- Minimal merge conflicts
- Independent testing per feature

---

### NFRR-05: Code Quality

**Acceptance Criteria**:
- Test coverage ≥80%
- No TypeScript errors
- No ESLint errors
- Consistent code style (Prettier)
- Modern React patterns
- Accessibility compliant

---

### NFRR-06: Preserve Functionality

**Acceptance Criteria**:
- All 8 PWA screens work identically
- Admin login screen works identically
- API integration unchanged
- User experience unchanged
- No regressions

---

### NFRR-07: Safety & Rollback

**Acceptance Criteria**:
- Old code archived trước recreation
- Git history preserved
- Can reference old code
- Archive deleted sau verification

---

## Architecture Decisions

### ADR-01: FSD Layer Structure

**Decision**: Implement đầy đủ FSD layers

**Structure**:
```
app/frontend/{admin|pwa}/src/
├── app/              # Routing only
├── pages/            # Composition
├── widgets/          # Composites
├── features/         # Interactions
├── entities/         # Domain models
└── shared/           # Primitives
```

---

### ADR-02: Execution Order

**Decision**: Admin trước (Phase 1), PWA sau (Phase 2)

**Rationale**: Admin simple - test ground, learn lessons, apply to PWA

---

### ADR-03: Recreation Strategy

**Decision**: Incremental - từng feature một, test ngay

**Rationale**: Low risk, quality control, parallel dev friendly

---

### ADR-04: Component Classification

**Decisions**:
- LoginForm/RegisterForm: Separate trong `features/auth/*/ui/`
- OtpInput: Shared primitive trong `shared/ui/`
- API Clients: Entity-based trong `entities/*/api/`
- Stores: Entity-based trong `entities/*/model/`

---

### ADR-05: Asset Reuse

**Decision**: Reuse shadcn/ui, translations, utilities, test scenarios

---

### ADR-06: Test Strategy

**Decision**: Hybrid - reuse scenarios, recreate code

---

### ADR-07: i18n Organization

**Decision**: Centralized với namespace organization

---

### ADR-08: Dependency Management

**Decision**: Audit và selective update

---

### ADR-09: Old Codebase Handling

**Decision**: Archive trong `_archive/` folder

---

### ADR-10: Code Generation

**Decision**: AI generate toàn bộ code mới

---

## Scope Boundaries

### Phase 1: Admin Recreation (Priority 1)

**In Scope**:
- 1 screen: Admin Login
- FSD structure: app/ → pages/ → features/ → entities/ → shared/
- Widgets: Optional (skip nếu không cần)
- Components: AdminLoginForm
- Store: `entities/admin/model/adminStore.ts`
- API: `entities/admin/api/admin.queries.ts`
- Tests: AdminLoginForm.test.tsx, adminStore.test.ts
- Reuse: shadcn/ui components

**Out of Scope**: PWA screens (Phase 2), new features, backend changes

---

### Phase 2: PWA Recreation (Priority 2)

**In Scope**:
- 8 screens: Login, Register, Verify Email, Verify Phone, Reset Password, Profile View, Profile Edit, Photo Upload
- FSD structure: app/ → pages/ → widgets/ → features/ → entities/ → shared/
- Widgets: AuthCard, ProfileHeader
- Features: 7 auth + 2 profile
- Entities: User, Profile
- Stores: userStore, profileStore
- API: user.queries, profile.queries
- Tests: Feature, entity, shared tests
- Reuse: shadcn/ui, translations, utilities, test scenarios

**Out of Scope**: New features, backend changes, Unit 2-5

---

## Success Criteria

### Must Have
- [ ] Admin login works với FSD structure
- [ ] All 8 PWA screens work như cũ
- [ ] All tests pass
- [ ] No TypeScript errors
- [ ] FSD structure đầy đủ
- [ ] Import paths follow FSD rules
- [ ] ESLint boundaries enforced

### Should Have
- [ ] Test coverage ≥80%
- [ ] Performance maintained
- [ ] Bundle size không tăng
- [ ] Documentation updated
- [ ] Consistent code style

### Nice to Have
- [ ] Improved organization
- [ ] Better type safety
- [ ] Cleaner APIs
- [ ] Improved test quality

---

## Risk Assessment

### High Risk
| Risk | Mitigation |
|---|---|
| Business logic loss | Reference old code, thorough review |
| Type errors | TypeScript strict, diagnostics |
| Runtime errors | Test thoroughly mỗi feature |
| Test coverage loss | Reuse test scenarios |

### Medium Risk
| Risk | Mitigation |
|---|---|
| FSD learning curve | Admin test ground, clear docs |
| Generated code quality | Thorough review, incremental |
| Over-engineering | Keep Unit 1 simple |

### Low Risk
| Risk | Mitigation |
|---|---|
| UI/UX changes | Preserve existing design |
| API contracts | No backend changes |
| Dependencies | Selective updates only |

---

## Estimated Effort

### Phase 1: Admin
**1-2 days** (1 developer)
- Setup & Archive: 0.5 day
- Generate Admin FSD: 0.5 day
- Generate Login feature: 0.5 day
- Test & Review: 0.5 day

### Phase 2: PWA
**4-5 days** (1 dev) hoặc **2-3 days** (2 devs parallel)
- Reuse Assets: 0.5 day
- Generate Entities: 1 day
- Generate Features - Auth: 1.5 days
- Generate Features - Profile: 1 day
- Generate Widgets: 0.5 day
- Generate Pages: 1 day
- Final Review: 0.5 day

**Total**: 5-7 days (1 dev) hoặc 3-4 days (2 devs)

---

## Dependencies

### External
- Next.js 14+, React 18+, TypeScript 5+
- Tailwind CSS 3+, Zustand 4+, Axios 1+
- next-intl 3+, shadcn/ui

### Internal
- UI/UX Design System docs
- Component Architecture docs
- Coding standards
- Backend API contracts (unchanged)

---

## Constraints

### Technical
- Next.js App Router (not Pages Router)
- TypeScript strict mode
- Tailwind CSS (no custom CSS unless necessary)
- Zustand for state (not Redux/Context)
- Axios for HTTP (not fetch)

### Business
- No UX changes
- No API contract changes
- Preserve all functionality
- No new features

### Timeline
- Admin complete trước PWA
- Incremental approach
- Thorough review required

---

## Traceability to Main Requirements

| Main Requirement | Recreation Requirement | Relationship |
|---|---|---|
| FR-01 (Email/LINE Auth) | FRR-06, FRR-07 | Recreate auth components với FSD |
| FR-02 (Profile Management) | FRR-08, FRR-09 | Recreate profile features với FSD |
| NFR-01 (Performance) | NFRR-06 | Preserve performance |
| NFR-02 (Security) | NFRR-06 | Preserve security |
| NFR-05 (Maintainability) | NFRR-02 | Improve maintainability với FSD |

---

## Next Steps

1. ✅ Requirements Analysis - Complete
2. ⏳ Workflow Planning - Determine detailed phases
3. ⏳ Functional Design - FSD structure details
4. ⏳ Code Generation - Phase 1 (Admin) → Phase 2 (PWA)
