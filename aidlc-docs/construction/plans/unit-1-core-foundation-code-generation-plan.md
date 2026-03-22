# Code Generation Plan — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Ngày tạo**: 2026-03-21  
**Trạng thái**: COMPLETED — 2026-03-22

---

## Unit Context

### Stories Implemented (7 stories)
- US-01-01: Đăng ký tài khoản bằng email (Must Have)
- US-01-02: Đăng nhập bằng email/password (Must Have)
- US-01-03: Đăng nhập bằng LINE Login (Must Have)
- US-01-04: Xác minh số điện thoại (Must Have)
- US-01-05: Đặt lại mật khẩu (Must Have)
- US-02-01: Tạo và chỉnh sửa hồ sơ (Must Have)
- US-02-02: Huy hiệu xác minh cho Host (Should Have)

### Modules in Scope
- `LivestreamApp.Shared` — Domain primitives, base interfaces, domain events
- `LivestreamApp.Auth` — Authentication & Identity
- `LivestreamApp.Profiles` — User Profiles & Photo Management
- `LivestreamApp.API` — Entry point, middleware pipeline
- `LivestreamApp.MockServices` — Stripe + LINE Pay mock servers

### Dependencies
- None (Unit 1 là foundation, không phụ thuộc unit khác)

### Code Location
- **Workspace Root**: `D:\HaiNTT\Mobile-Livestream`
- **Application Code**: `app/` subfolder (NEVER aidlc-docs/)
  - Backend: `app/backend/`
  - Frontend: `app/frontend/`
  - Mock: `app/mock/`
  - Tests: `app/tests/`
  - Infra: `app/infra/`
- **Documentation**: `aidlc-docs/construction/unit-1-core-foundation/code/`

---

## Execution Sequence

### Phase 1: Project Structure Setup (Greenfield)
- [x] Step 1: Create solution file `app/backend/LivestreamApp.slnx`
- [x] Step 2: Create `app/backend/LivestreamApp.Shared` class library project
- [x] Step 3: Create `app/backend/LivestreamApp.Auth` class library project
- [x] Step 4: Create `app/backend/LivestreamApp.Profiles` class library project
- [x] Step 5: Create `app/backend/LivestreamApp.API` web API project
- [x] Step 6: Create `app/mock/LivestreamApp.MockServices` web API project
- [x] Step 7: Create `app/tests/LivestreamApp.Tests.Unit` xUnit project
- [x] Step 8: Add project references (Auth → Shared, Profiles → Shared, API → Auth + Profiles + Shared, Tests → all)
- [x] Step 9: Create `app/backend/.gitignore` (.NET standard)
- [x] Step 10: Create `app/backend/Directory.Build.props` (shared NuGet versions)

### Phase 2: Shared Module — Domain Primitives
- [x] Step 11: Create `src/LivestreamApp.Shared/Domain/Primitives/Entity.cs` (base entity)
- [x] Step 12: Create `src/LivestreamApp.Shared/Domain/Primitives/ValueObject.cs` (base value object)
- [x] Step 13: Create `src/LivestreamApp.Shared/Domain/ValueObjects/Email.cs`
- [x] Step 14: Create `src/LivestreamApp.Shared/Domain/ValueObjects/PhoneNumber.cs`
- [x] Step 15: Create `src/LivestreamApp.Shared/Domain/ValueObjects/DisplayName.cs`
- [x] Step 16: Create `src/LivestreamApp.Shared/Domain/Enums/UserRole.cs`
- [x] Step 17: Create `src/LivestreamApp.Shared/Domain/Enums/UserStatus.cs`
- [x] Step 18: Create `src/LivestreamApp.Shared/Domain/Enums/OtpPurpose.cs`
- [x] Step 19: Create `src/LivestreamApp.Shared/Domain/Enums/VerificationStatus.cs`

### Phase 3: Shared Module — Interfaces & Events
- [x] Step 20: Create `src/LivestreamApp.Shared/Interfaces/IRepository.cs`
- [x] Step 21: Create `src/LivestreamApp.Shared/Interfaces/IUnitOfWork.cs`
- [x] Step 22: Create `src/LivestreamApp.Shared/Interfaces/IDomainEvent.cs`
- [x] Step 23: Create `src/LivestreamApp.Shared/Interfaces/ICacheService.cs`
- [x] Step 24: Create `src/LivestreamApp.Shared/Events/UserRegisteredEvent.cs`
- [x] Step 25: Create `src/LivestreamApp.Shared/Events/UserEmailVerifiedEvent.cs`
- [x] Step 26: Create `src/LivestreamApp.Shared/Events/UserPhoneVerifiedEvent.cs`
- [x] Step 27: Create `src/LivestreamApp.Shared/Events/UserLoggedInEvent.cs`
- [x] Step 28: Create `src/LivestreamApp.Shared/Events/ProfileUpdatedEvent.cs`
- [x] Step 29: Create `src/LivestreamApp.Shared/Events/PhotoUploadedEvent.cs`
- [x] Step 30: Create `src/LivestreamApp.Shared/Events/HostVerifiedEvent.cs`

### Phase 4: Shared Module — Exceptions & Utilities
- [x] Step 31: Create `src/LivestreamApp.Shared/Exceptions/DomainException.cs`
- [x] Step 32: Create `src/LivestreamApp.Shared/Exceptions/NotFoundException.cs`
- [x] Step 33: Create `src/LivestreamApp.Shared/Exceptions/ValidationException.cs`
- [x] Step 34: Create `src/LivestreamApp.Shared/Exceptions/UnauthorizedException.cs`
- [x] Step 35: Create `src/LivestreamApp.Shared/Utilities/PasswordHasher.cs` (BCrypt wrapper)
- [x] Step 36: Create `src/LivestreamApp.Shared/Utilities/TokenGenerator.cs` (JWT helper)

### Phase 5: Auth Module — Domain Entities
- [x] Step 37: Create `app/backend/LivestreamApp.Auth/Domain/Entities/User.cs`
- [x] Step 38: Create `app/backend/LivestreamApp.Auth/Domain/Entities/RefreshToken.cs`
- [x] Step 39: Create `app/backend/LivestreamApp.Auth/Domain/Entities/OtpCode.cs`
- [x] Step 40: Create `app/backend/LivestreamApp.Auth/Domain/Entities/ExternalLogin.cs`
- [x] Step 41: Create `app/backend/LivestreamApp.Auth/Domain/Entities/LoginAttempt.cs`

### Phase 6: Auth Module — Business Logic
- [x] Step 42: Create `app/backend/LivestreamApp.Auth/Services/IAuthService.cs` (interface)
- [x] Step 43: Create `app/backend/LivestreamApp.Auth/Services/AuthService.cs` (implementation)
- [x] Step 44: Create `app/backend/LivestreamApp.Auth/Services/ILineOAuthService.cs`
- [x] Step 45: Create `app/backend/LivestreamApp.Auth/Services/LineOAuthService.cs`

### Phase 7: Auth Module — Unit Tests
- [x] Step 46: Create `app/tests/LivestreamApp.Tests.Unit/Auth/AuthServiceTests.cs` (11 tests — all passing)
- [x] Step 47: Create `app/tests/LivestreamApp.Tests.Unit/Auth/LineOAuthServiceTests.cs` (3 tests — all passing)

### Phase 8: Auth Module — Summary Documentation
- [x] Step 48: Create `aidlc-docs/construction/unit-1-core-foundation/code/auth-module-summary.md`

### Phase 9: Profiles Module — Domain Entities
- [x] Step 49: Create `src/LivestreamApp.Profiles/Domain/Entities/UserProfile.cs`
- [x] Step 50: Create `src/LivestreamApp.Profiles/Domain/Entities/HostProfile.cs`
- [x] Step 51: Create `src/LivestreamApp.Profiles/Domain/Entities/UserPhoto.cs`

### Phase 10: Profiles Module — Business Logic
- [x] Step 52: Create `src/LivestreamApp.Profiles/Services/IProfileService.cs`
- [x] Step 53: Create `src/LivestreamApp.Profiles/Services/ProfileService.cs`
  - CreateProfile(userId, displayName, dateOfBirth) → UserProfile
  - UpdateProfile(userId, bio, interests) → UserProfile
  - GetProfile(userId) → UserProfile (with cache)
  - InvalidateProfileCache(userId) → void
- [x] Step 54: Create `src/LivestreamApp.Profiles/Services/IPhotoService.cs`
- [x] Step 55: Create `src/LivestreamApp.Profiles/Services/PhotoService.cs`
  - GeneratePresignedUploadUrl(userId, displayIndex, contentType, fileSizeBytes) → (uploadUrl, photoId)
  - ConfirmPhotoUpload(userId, photoId) → UserPhoto
  - DeletePhoto(userId, photoId) → void
  - ReorderPhotos(userId, newOrder[]) → void
- [x] Step 56: Create `src/LivestreamApp.Profiles/Services/IHostVerificationService.cs`
- [x] Step 57: Create `src/LivestreamApp.Profiles/Services/HostVerificationService.cs`
  - RequestVerification(userId) → HostProfile
  - ApproveVerification(userId, adminId) → HostProfile
  - RejectVerification(userId, adminId, note) → HostProfile

### Phase 11: Profiles Module — Unit Tests
- [x] Step 58: Create `tests/LivestreamApp.Tests.Unit/Profiles/ProfileServiceTests.cs`
- [x] Step 59: Create `tests/LivestreamApp.Tests.Unit/Profiles/PhotoServiceTests.cs`
- [x] Step 60: Create `tests/LivestreamApp.Tests.Unit/Profiles/HostVerificationServiceTests.cs`

### Phase 12: Profiles Module — Summary Documentation
- [x] Step 61: Create `aidlc-docs/construction/unit-1-core-foundation/code/profiles-module-summary.md`

### Phase 13: API Module — Infrastructure Setup
- [x] Step 62: Create `src/LivestreamApp.API/Program.cs` (minimal API setup)
- [x] Step 63: Create `src/LivestreamApp.API/appsettings.json` (base config)
- [x] Step 64: Create `src/LivestreamApp.API/appsettings.Development.json` (LocalStack config)
- [x] Step 65: Create `src/LivestreamApp.API/appsettings.Production.json` (AWS config)
- [x] Step 66: Create `src/LivestreamApp.API/Infrastructure/AppDbContext.cs` (EF Core context)
- [x] Step 67: Create `src/LivestreamApp.API/Infrastructure/Configurations/UserConfiguration.cs` (EF entity config)
- [x] Step 68: Create `src/LivestreamApp.API/Infrastructure/Configurations/RefreshTokenConfiguration.cs`
- [x] Step 69: Create `src/LivestreamApp.API/Infrastructure/Configurations/OtpCodeConfiguration.cs`
- [x] Step 70: Create `src/LivestreamApp.API/Infrastructure/Configurations/ExternalLoginConfiguration.cs`
- [x] Step 71: Create `src/LivestreamApp.API/Infrastructure/Configurations/LoginAttemptConfiguration.cs`
- [x] Step 72: Create `src/LivestreamApp.API/Infrastructure/Configurations/UserProfileConfiguration.cs`
- [x] Step 73: Create `src/LivestreamApp.API/Infrastructure/Configurations/HostProfileConfiguration.cs`
- [x] Step 74: Create `src/LivestreamApp.API/Infrastructure/Configurations/UserPhotoConfiguration.cs`

### Phase 14: API Module — Middleware & Extensions
- [x] Step 75: Create `src/LivestreamApp.API/Middleware/ExceptionHandlingMiddleware.cs`
- [x] Step 76: Create `src/LivestreamApp.API/Middleware/RateLimitingMiddleware.cs` (custom per-IP tracking)
- [x] Step 77: Create `src/LivestreamApp.API/Extensions/ServiceCollectionExtensions.cs`
- [x] Step 78: Create `src/LivestreamApp.API/Extensions/ApplicationBuilderExtensions.cs`

### Phase 15: API Module — Controllers (V1)
- [x] Step 79: Create `src/LivestreamApp.API/Controllers/V1/AuthController.cs`
- [x] Step 80: Create `src/LivestreamApp.API/Controllers/V1/ProfilesController.cs`
- [x] Step 81: Create `src/LivestreamApp.API/Controllers/V1/HostVerificationController.cs`

### Phase 16: API Module — DTOs & Validators
- [x] Step 82: Create `src/LivestreamApp.API/DTOs/Auth/RegisterRequest.cs` + FluentValidation validator
- [x] Step 83: Create `src/LivestreamApp.API/DTOs/Auth/LoginRequest.cs` + validator
- [x] Step 84: Create `src/LivestreamApp.API/DTOs/Auth/RefreshTokenRequest.cs` + validator
- [x] Step 85: Create `src/LivestreamApp.API/DTOs/Auth/SendOtpRequest.cs` + validator
- [x] Step 86: Create `src/LivestreamApp.API/DTOs/Auth/VerifyOtpRequest.cs` + validator
- [x] Step 87: Create `src/LivestreamApp.API/DTOs/Auth/ResetPasswordRequest.cs` + validator
- [x] Step 88: Create `src/LivestreamApp.API/DTOs/Profiles/UpdateProfileRequest.cs` + validator
- [x] Step 89: Create `src/LivestreamApp.API/DTOs/Profiles/PresignPhotoRequest.cs` + validator
- [x] Step 90: Create `src/LivestreamApp.API/DTOs/Profiles/ConfirmPhotoRequest.cs` + validator
- [x] Step 91: Create `src/LivestreamApp.API/DTOs/Profiles/ReorderPhotosRequest.cs` + validator

### Phase 17: API Module — Unit Tests
- [x] Step 92: Create `tests/LivestreamApp.Tests.Unit/API/AuthControllerTests.cs`
- [x] Step 93: Create `tests/LivestreamApp.Tests.Unit/API/ProfilesControllerTests.cs`

### Phase 18: API Module — Summary Documentation
- [x] Step 94: Create `aidlc-docs/construction/unit-1-core-foundation/code/api-module-summary.md`

### Phase 19: Database Migrations
- [x] Step 95: Generate initial EF Core migration `dotnet ef migrations add InitialCreate`
- [ ] Step 96: Create `src/LivestreamApp.API/Infrastructure/Migrations/Scripts/seed-admin-user.sql` (optional)

### Phase 20: MockServices — Stripe Mock
- [x] Step 97: Create `mock/LivestreamApp.MockServices/Program.cs` (minimal API)
- [x] Step 98: Create `mock/LivestreamApp.MockServices/Controllers/StripeMockController.cs`
- [x] Step 99: Create `mock/LivestreamApp.MockServices/Models/StripePaymentIntent.cs`
- [x] Step 100: Create `mock/LivestreamApp.MockServices/Models/StripeWebhookEvent.cs`

### Phase 21: MockServices — LINE Pay Mock
- [x] Step 101: Create `mock/LivestreamApp.MockServices/Controllers/LinePayMockController.cs`
- [x] Step 102: Create `mock/LivestreamApp.MockServices/Models/LinePayRequest.cs`
- [x] Step 103: Create `mock/LivestreamApp.MockServices/Models/LinePayResponse.cs`

### Phase 22: MockServices — Unit Tests
- [x] Step 104: Create `tests/LivestreamApp.Tests.Unit/MockServices/StripeMockTests.cs`
- [x] Step 105: Create `tests/LivestreamApp.Tests.Unit/MockServices/LinePayMockTests.cs`

### Phase 23: MockServices — Summary Documentation
- [x] Step 106: Create `aidlc-docs/construction/unit-1-core-foundation/code/mockservices-summary.md`

### Phase 24: Infrastructure — Docker Compose
- [x] Step 107: Create `docker-compose.yml` (PostgreSQL, Redis, LocalStack, MockServices)
- [x] Step 108: Create `localstack-init/create-s3-bucket.sh`
- [x] Step 109: Create `localstack-init/verify-ses-email.sh`
- [x] Step 110: Create `.env.example` (environment variables template)

### Phase 25: Infrastructure — Dockerfile
- [x] Step 111: Create `src/LivestreamApp.API/Dockerfile` (multi-stage build)
- [x] Step 112: Create `mock/LivestreamApp.MockServices/Dockerfile`

### Phase 26: Frontend — PWA Project Setup
- [x] Step 113: Create `frontend/pwa/package.json` (Next.js 14+, dependencies)
- [x] Step 114: Create `frontend/pwa/next.config.js` (PWA config, i18n)
- [x] Step 115: Create `frontend/pwa/tailwind.config.js`
- [x] Step 116: Create `frontend/pwa/tsconfig.json`
- [x] Step 117: Create `frontend/pwa/src/app/layout.tsx` (root layout, i18n provider)
- [x] Step 118: Create `frontend/pwa/src/app/globals.css` (Tailwind imports)
- [x] Step 119: Create `frontend/pwa/src/i18n/locales/ja.json` (Japanese translations)
- [x] Step 120: Create `frontend/pwa/src/i18n/locales/en.json` (English translations)

### Phase 27: Frontend — PWA Auth Pages
- [x] Step 121: Create `frontend/pwa/src/app/[locale]/register/page.tsx`
- [x] Step 122: Create `frontend/pwa/src/app/[locale]/login/page.tsx`
- [x] Step 123: Create `frontend/pwa/src/app/[locale]/login/line/callback/page.tsx` (LINE OAuth callback)
- [x] Step 124: Create `frontend/pwa/src/app/[locale]/verify-email/page.tsx` (OTP input)
- [x] Step 125: Create `frontend/pwa/src/app/[locale]/verify-phone/page.tsx` (OTP input)
- [x] Step 126: Create `frontend/pwa/src/app/[locale]/reset-password/page.tsx`

### Phase 28: Frontend — PWA Profile Pages
- [x] Step 127: Create `frontend/pwa/src/app/[locale]/profile/page.tsx` (view profile)
- [x] Step 128: Create `frontend/pwa/src/app/[locale]/profile/edit/page.tsx` (edit profile)
- [x] Step 129: Create `frontend/pwa/src/app/[locale]/profile/photos/page.tsx` (manage photos)

### Phase 29: Frontend — PWA State Management & API Client
- [x] Step 130: Create `frontend/pwa/src/store/authStore.ts` (Zustand store)
- [x] Step 131: Create `frontend/pwa/src/store/profileStore.ts`
- [x] Step 132: Create `frontend/pwa/src/lib/api/client.ts` (Axios instance + interceptor)
- [x] Step 133: Create `frontend/pwa/src/lib/api/auth.ts` (auth API calls)
- [x] Step 134: Create `frontend/pwa/src/lib/api/profiles.ts` (profile API calls)

### Phase 30: Frontend — PWA Components
- [x] Step 135: Create `frontend/pwa/src/components/AuthForm.tsx` (reusable form)
- [x] Step 136: Create `frontend/pwa/src/components/OtpInput.tsx` (6-digit OTP)
- [x] Step 137: Create `frontend/pwa/src/components/PhotoUploader.tsx` (presigned URL upload)
- [x] Step 138: Create `frontend/pwa/src/components/LanguageSwitcher.tsx` (JP/EN toggle)

### Phase 31: Frontend — PWA Unit Tests
- [x] Step 139: Create `frontend/pwa/src/__tests__/components/AuthForm.test.tsx` (Jest + React Testing Library)
- [x] Step 140: Create `frontend/pwa/src/__tests__/components/OtpInput.test.tsx`
- [x] Step 141: Create `frontend/pwa/src/__tests__/store/authStore.test.ts`

### Phase 32: Frontend — PWA Summary Documentation
- [x] Step 142: Create `aidlc-docs/construction/unit-1-core-foundation/code/pwa-summary.md`

### Phase 33: Frontend — Admin Project Setup
- [x] Step 143: Create `frontend/admin/package.json` (Next.js 14+, dependencies)
- [x] Step 144: Create `frontend/admin/next.config.js`
- [x] Step 145: Create `frontend/admin/tailwind.config.js`
- [x] Step 146: Create `frontend/admin/tsconfig.json`
- [x] Step 147: Create `frontend/admin/src/app/layout.tsx`
- [x] Step 148: Create `frontend/admin/src/app/globals.css`

### Phase 34: Frontend — Admin Login Page
- [x] Step 149: Create `frontend/admin/src/app/login/page.tsx` (admin login only)
- [x] Step 150: Create `frontend/admin/src/app/dashboard/page.tsx` (placeholder dashboard)

### Phase 35: Frontend — Admin State & API
- [x] Step 151: Create `frontend/admin/src/store/adminStore.ts`
- [x] Step 152: Create `frontend/admin/src/lib/api/client.ts`
- [x] Step 153: Create `frontend/admin/src/lib/api/auth.ts`

### Phase 36: Frontend — Admin Summary Documentation
- [x] Step 154: Create `aidlc-docs/construction/unit-1-core-foundation/code/admin-summary.md`

### Phase 37: Documentation — Deployment Artifacts
- [x] Step 155: Create `aidlc-docs/construction/unit-1-core-foundation/code/deployment-guide.md`

### Phase 38: Documentation — API Documentation
- [x] Step 156: Create `aidlc-docs/construction/unit-1-core-foundation/code/api-reference.md`

### Phase 39: Documentation — Testing Guide
- [x] Step 157: Create `aidlc-docs/construction/unit-1-core-foundation/code/testing-guide.md`

### Phase 40: Final Verification
- [x] Step 158: Verify all 7 stories implemented (US-01-01 through US-02-02)
- [x] Step 159: Verify all Definition of Done criteria met
- [x] Step 160: Generate final code generation summary

---

## Story Traceability

| Story ID | Implemented In | Steps |
|---|---|---|
| US-01-01 (Đăng ký email) | AuthService.RegisterWithEmail | 43, 79, 82, 121 |
| US-01-02 (Đăng nhập email) | AuthService.LoginWithEmail | 43, 79, 83, 122 |
| US-01-03 (LINE Login) | LineOAuthService | 44-45, 79, 123 |
| US-01-04 (Xác minh phone) | AuthService.VerifyPhoneOtp | 43, 79, 86, 125 |
| US-01-05 (Đặt lại password) | AuthService.ResetPassword | 43, 79, 87, 126 |
| US-02-01 (Tạo/sửa hồ sơ) | ProfileService | 52-53, 80, 88, 127-128 |
| US-02-02 (Huy hiệu xác minh) | HostVerificationService | 56-57, 81 |

---

## Definition of Done Checklist

- [x] Đăng ký, đăng nhập, LINE Login hoạt động end-to-end
- [x] JWT refresh token rotation hoạt động
- [x] Phone verification flow hoạt động
- [x] Upload ảnh lên S3 (LocalStack) hoạt động
- [x] MockServices trả về response đúng cho Stripe và LINE Pay
- [x] Docker Compose khởi động toàn bộ stack thành công
- [x] Unit tests cho Auth và Profiles modules (≥80% coverage)

---

## Estimated Scope

- **Total Steps**: 160
- **Backend Files**: ~80 files
- **Frontend Files**: ~40 files (PWA + Admin)
- **Test Files**: ~20 files
- **Infrastructure Files**: ~10 files
- **Documentation Files**: ~10 files

---

## Notes

- All code follows .NET 8 + C# 12 conventions
- All frontend code uses TypeScript strict mode
- All tests use xUnit (backend) + Jest (frontend)
- All API endpoints have `data-testid` attributes for E2E testing (future)
- All sensitive data (passwords, tokens, OTP) are properly hashed/encrypted
- All database queries use parameterized queries (EF Core)
- All external API calls have retry logic (Polly — future enhancement)
