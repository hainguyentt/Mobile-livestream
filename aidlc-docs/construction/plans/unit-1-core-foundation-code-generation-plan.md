# Code Generation Plan — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Ngày tạo**: 2026-03-21  
**Trạng thái**: Planning — Chờ approval

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
- **Workspace Root**: `D:\HaiNTT\Mobile-Livestream` (from aidlc-state.md)
- **Application Code**: Workspace root (NEVER aidlc-docs/)
- **Documentation**: `aidlc-docs/construction/unit-1-core-foundation/code/`

---

## Execution Sequence

### Phase 1: Project Structure Setup (Greenfield)
- [ ] Step 1: Create solution file `LivestreamApp.sln`
- [ ] Step 2: Create `src/LivestreamApp.Shared` class library project
- [ ] Step 3: Create `src/LivestreamApp.Auth` class library project
- [ ] Step 4: Create `src/LivestreamApp.Profiles` class library project
- [ ] Step 5: Create `src/LivestreamApp.API` web API project
- [ ] Step 6: Create `mock/LivestreamApp.MockServices` web API project
- [ ] Step 7: Create `tests/LivestreamApp.Tests.Unit` xUnit project
- [ ] Step 8: Add project references (Auth → Shared, Profiles → Shared, API → Auth + Profiles)
- [ ] Step 9: Create `.gitignore` (standard .NET + Node.js)
- [ ] Step 10: Create `Directory.Build.props` (shared NuGet versions)

### Phase 2: Shared Module — Domain Primitives
- [ ] Step 11: Create `src/LivestreamApp.Shared/Domain/Primitives/Entity.cs` (base entity)
- [ ] Step 12: Create `src/LivestreamApp.Shared/Domain/Primitives/ValueObject.cs` (base value object)
- [ ] Step 13: Create `src/LivestreamApp.Shared/Domain/ValueObjects/Email.cs`
- [ ] Step 14: Create `src/LivestreamApp.Shared/Domain/ValueObjects/PhoneNumber.cs`
- [ ] Step 15: Create `src/LivestreamApp.Shared/Domain/ValueObjects/DisplayName.cs`
- [ ] Step 16: Create `src/LivestreamApp.Shared/Domain/Enums/UserRole.cs`
- [ ] Step 17: Create `src/LivestreamApp.Shared/Domain/Enums/UserStatus.cs`
- [ ] Step 18: Create `src/LivestreamApp.Shared/Domain/Enums/OtpPurpose.cs`
- [ ] Step 19: Create `src/LivestreamApp.Shared/Domain/Enums/VerificationStatus.cs`

### Phase 3: Shared Module — Interfaces & Events
- [ ] Step 20: Create `src/LivestreamApp.Shared/Interfaces/IRepository.cs`
- [ ] Step 21: Create `src/LivestreamApp.Shared/Interfaces/IUnitOfWork.cs`
- [ ] Step 22: Create `src/LivestreamApp.Shared/Interfaces/IDomainEvent.cs`
- [ ] Step 23: Create `src/LivestreamApp.Shared/Interfaces/ICacheService.cs`
- [ ] Step 24: Create `src/LivestreamApp.Shared/Events/UserRegisteredEvent.cs`
- [ ] Step 25: Create `src/LivestreamApp.Shared/Events/UserEmailVerifiedEvent.cs`
- [ ] Step 26: Create `src/LivestreamApp.Shared/Events/UserPhoneVerifiedEvent.cs`
- [ ] Step 27: Create `src/LivestreamApp.Shared/Events/UserLoggedInEvent.cs`
- [ ] Step 28: Create `src/LivestreamApp.Shared/Events/ProfileUpdatedEvent.cs`
- [ ] Step 29: Create `src/LivestreamApp.Shared/Events/PhotoUploadedEvent.cs`
- [ ] Step 30: Create `src/LivestreamApp.Shared/Events/HostVerifiedEvent.cs`

### Phase 4: Shared Module — Exceptions & Utilities
- [ ] Step 31: Create `src/LivestreamApp.Shared/Exceptions/DomainException.cs`
- [ ] Step 32: Create `src/LivestreamApp.Shared/Exceptions/NotFoundException.cs`
- [ ] Step 33: Create `src/LivestreamApp.Shared/Exceptions/ValidationException.cs`
- [ ] Step 34: Create `src/LivestreamApp.Shared/Exceptions/UnauthorizedException.cs`
- [ ] Step 35: Create `src/LivestreamApp.Shared/Utilities/PasswordHasher.cs` (BCrypt wrapper)
- [ ] Step 36: Create `src/LivestreamApp.Shared/Utilities/TokenGenerator.cs` (JWT helper)

### Phase 5: Auth Module — Domain Entities
- [ ] Step 37: Create `src/LivestreamApp.Auth/Domain/Entities/User.cs`
- [ ] Step 38: Create `src/LivestreamApp.Auth/Domain/Entities/RefreshToken.cs`
- [ ] Step 39: Create `src/LivestreamApp.Auth/Domain/Entities/OtpCode.cs`
- [ ] Step 40: Create `src/LivestreamApp.Auth/Domain/Entities/ExternalLogin.cs`
- [ ] Step 41: Create `src/LivestreamApp.Auth/Domain/Entities/LoginAttempt.cs`

### Phase 6: Auth Module — Business Logic
- [ ] Step 42: Create `src/LivestreamApp.Auth/Services/IAuthService.cs` (interface)
- [ ] Step 43: Create `src/LivestreamApp.Auth/Services/AuthService.cs` (implementation)
  - RegisterWithEmail(email, password) → User
  - LoginWithEmail(email, password) → (accessToken, refreshToken)
  - LoginWithLine(lineCode) → (accessToken, refreshToken)
  - RefreshAccessToken(refreshToken) → (newAccessToken, newRefreshToken)
  - SendEmailOtp(email, purpose) → void
  - VerifyEmailOtp(email, code, purpose) → bool
  - SendPhoneOtp(phoneNumber) → void
  - VerifyPhoneOtp(phoneNumber, code) → bool
  - ResetPassword(email, newPassword, otpCode) → void
  - RevokeRefreshToken(tokenHash) → void
- [ ] Step 44: Create `src/LivestreamApp.Auth/Services/ILineOAuthService.cs`
- [ ] Step 45: Create `src/LivestreamApp.Auth/Services/LineOAuthService.cs`
  - GetAuthorizationUrl() → string
  - ExchangeCodeForToken(code) → LineTokenResponse
  - GetUserProfile(accessToken) → LineUserProfile
  - LinkOrMergeAccount(lineUserId, lineEmail) → User

### Phase 7: Auth Module — Unit Tests
- [ ] Step 46: Create `tests/LivestreamApp.Tests.Unit/Auth/AuthServiceTests.cs`
  - Test_RegisterWithEmail_Success
  - Test_RegisterWithEmail_DuplicateEmail_ThrowsException
  - Test_LoginWithEmail_Success
  - Test_LoginWithEmail_InvalidPassword_IncrementFailCount
  - Test_LoginWithEmail_5thFailure_RequiresCaptcha
  - Test_LoginWithEmail_10thFailure_LocksAccount
  - Test_RefreshAccessToken_Success
  - Test_RefreshAccessToken_RevokedToken_ThrowsException
  - Test_VerifyEmailOtp_Success
  - Test_VerifyEmailOtp_Expired_ReturnsFalse
  - Test_VerifyEmailOtp_3rdAttempt_InvalidatesOtp
- [ ] Step 47: Create `tests/LivestreamApp.Tests.Unit/Auth/LineOAuthServiceTests.cs`
  - Test_LinkOrMergeAccount_NewUser_CreatesAccount
  - Test_LinkOrMergeAccount_ExistingEmail_MergesAccount
  - Test_LinkOrMergeAccount_ExistingLineId_ReturnsExistingUser

### Phase 8: Auth Module — Summary Documentation
- [ ] Step 48: Create `aidlc-docs/construction/unit-1-core-foundation/code/auth-module-summary.md`
  - List all generated files with paths
  - Business logic summary
  - Test coverage summary
  - API endpoints summary (will be created in Phase 11)

### Phase 9: Profiles Module — Domain Entities
- [ ] Step 49: Create `src/LivestreamApp.Profiles/Domain/Entities/UserProfile.cs`
- [ ] Step 50: Create `src/LivestreamApp.Profiles/Domain/Entities/HostProfile.cs`
- [ ] Step 51: Create `src/LivestreamApp.Profiles/Domain/Entities/UserPhoto.cs`

### Phase 10: Profiles Module — Business Logic
- [ ] Step 52: Create `src/LivestreamApp.Profiles/Services/IProfileService.cs`
- [ ] Step 53: Create `src/LivestreamApp.Profiles/Services/ProfileService.cs`
  - CreateProfile(userId, displayName, dateOfBirth) → UserProfile
  - UpdateProfile(userId, bio, interests) → UserProfile
  - GetProfile(userId) → UserProfile (with cache)
  - InvalidateProfileCache(userId) → void
- [ ] Step 54: Create `src/LivestreamApp.Profiles/Services/IPhotoService.cs`
- [ ] Step 55: Create `src/LivestreamApp.Profiles/Services/PhotoService.cs`
  - GeneratePresignedUploadUrl(userId, displayIndex, contentType, fileSizeBytes) → (uploadUrl, photoId)
  - ConfirmPhotoUpload(userId, photoId) → UserPhoto
  - DeletePhoto(userId, photoId) → void
  - ReorderPhotos(userId, newOrder[]) → void
- [ ] Step 56: Create `src/LivestreamApp.Profiles/Services/IHostVerificationService.cs`
- [ ] Step 57: Create `src/LivestreamApp.Profiles/Services/HostVerificationService.cs`
  - RequestVerification(userId) → HostProfile
  - ApproveVerification(userId, adminId) → HostProfile
  - RejectVerification(userId, adminId, note) → HostProfile

### Phase 11: Profiles Module — Unit Tests
- [ ] Step 58: Create `tests/LivestreamApp.Tests.Unit/Profiles/ProfileServiceTests.cs`
  - Test_CreateProfile_Success
  - Test_CreateProfile_DuplicateDisplayName_ThrowsException
  - Test_UpdateProfile_Success_InvalidatesCache
  - Test_GetProfile_CacheHit_ReturnsFromCache
  - Test_GetProfile_CacheMiss_LoadsFromDb
- [ ] Step 59: Create `tests/LivestreamApp.Tests.Unit/Profiles/PhotoServiceTests.cs`
  - Test_GeneratePresignedUploadUrl_Success
  - Test_GeneratePresignedUploadUrl_InvalidIndex_ThrowsException
  - Test_ConfirmPhotoUpload_Success_ResizesImage
  - Test_ConfirmPhotoUpload_ObjectNotFound_ThrowsException
  - Test_ReorderPhotos_Success
- [ ] Step 60: Create `tests/LivestreamApp.Tests.Unit/Profiles/HostVerificationServiceTests.cs`
  - Test_RequestVerification_Success
  - Test_ApproveVerification_Success_EmitsEvent
  - Test_RejectVerification_Success

### Phase 12: Profiles Module — Summary Documentation
- [ ] Step 61: Create `aidlc-docs/construction/unit-1-core-foundation/code/profiles-module-summary.md`

### Phase 13: API Module — Infrastructure Setup
- [ ] Step 62: Create `src/LivestreamApp.API/Program.cs` (minimal API setup)
- [ ] Step 63: Create `src/LivestreamApp.API/appsettings.json` (base config)
- [ ] Step 64: Create `src/LivestreamApp.API/appsettings.Development.json` (LocalStack config)
- [ ] Step 65: Create `src/LivestreamApp.API/appsettings.Production.json` (AWS config)
- [ ] Step 66: Create `src/LivestreamApp.API/Infrastructure/AppDbContext.cs` (EF Core context)
- [ ] Step 67: Create `src/LivestreamApp.API/Infrastructure/Configurations/UserConfiguration.cs` (EF entity config)
- [ ] Step 68: Create `src/LivestreamApp.API/Infrastructure/Configurations/RefreshTokenConfiguration.cs`
- [ ] Step 69: Create `src/LivestreamApp.API/Infrastructure/Configurations/OtpCodeConfiguration.cs`
- [ ] Step 70: Create `src/LivestreamApp.API/Infrastructure/Configurations/ExternalLoginConfiguration.cs`
- [ ] Step 71: Create `src/LivestreamApp.API/Infrastructure/Configurations/LoginAttemptConfiguration.cs`
- [ ] Step 72: Create `src/LivestreamApp.API/Infrastructure/Configurations/UserProfileConfiguration.cs`
- [ ] Step 73: Create `src/LivestreamApp.API/Infrastructure/Configurations/HostProfileConfiguration.cs`
- [ ] Step 74: Create `src/LivestreamApp.API/Infrastructure/Configurations/UserPhotoConfiguration.cs`

### Phase 14: API Module — Middleware & Extensions
- [ ] Step 75: Create `src/LivestreamApp.API/Middleware/ExceptionHandlingMiddleware.cs`
- [ ] Step 76: Create `src/LivestreamApp.API/Middleware/RateLimitingMiddleware.cs` (custom per-IP tracking)
- [ ] Step 77: Create `src/LivestreamApp.API/Extensions/ServiceCollectionExtensions.cs`
  - AddAuthenticationServices()
  - AddCachingServices()
  - AddStorageServices()
  - AddEmailServices()
  - AddBackgroundJobs()
- [ ] Step 78: Create `src/LivestreamApp.API/Extensions/ApplicationBuilderExtensions.cs`
  - UseCustomMiddleware()
  - UseHealthChecks()

### Phase 15: API Module — Controllers (V1)
- [ ] Step 79: Create `src/LivestreamApp.API/Controllers/V1/AuthController.cs`
  - POST /api/v1/auth/register
  - POST /api/v1/auth/login
  - POST /api/v1/auth/login/line
  - POST /api/v1/auth/refresh
  - POST /api/v1/auth/logout
  - POST /api/v1/auth/otp/email/send
  - POST /api/v1/auth/otp/email/verify
  - POST /api/v1/auth/otp/phone/send
  - POST /api/v1/auth/otp/phone/verify
  - POST /api/v1/auth/password/reset
- [ ] Step 80: Create `src/LivestreamApp.API/Controllers/V1/ProfilesController.cs`
  - GET /api/v1/profiles/me
  - PUT /api/v1/profiles/me
  - POST /api/v1/profiles/photos/presign
  - POST /api/v1/profiles/photos/confirm
  - DELETE /api/v1/profiles/photos/{photoId}
  - PUT /api/v1/profiles/photos/reorder
- [ ] Step 81: Create `src/LivestreamApp.API/Controllers/V1/HostVerificationController.cs`
  - POST /api/v1/host/verification/request
  - POST /api/v1/host/verification/approve (admin only)
  - POST /api/v1/host/verification/reject (admin only)

### Phase 16: API Module — DTOs & Validators
- [ ] Step 82: Create `src/LivestreamApp.API/DTOs/Auth/RegisterRequest.cs` + FluentValidation validator
- [ ] Step 83: Create `src/LivestreamApp.API/DTOs/Auth/LoginRequest.cs` + validator
- [ ] Step 84: Create `src/LivestreamApp.API/DTOs/Auth/RefreshTokenRequest.cs` + validator
- [ ] Step 85: Create `src/LivestreamApp.API/DTOs/Auth/SendOtpRequest.cs` + validator
- [ ] Step 86: Create `src/LivestreamApp.API/DTOs/Auth/VerifyOtpRequest.cs` + validator
- [ ] Step 87: Create `src/LivestreamApp.API/DTOs/Auth/ResetPasswordRequest.cs` + validator
- [ ] Step 88: Create `src/LivestreamApp.API/DTOs/Profiles/UpdateProfileRequest.cs` + validator
- [ ] Step 89: Create `src/LivestreamApp.API/DTOs/Profiles/PresignPhotoRequest.cs` + validator
- [ ] Step 90: Create `src/LivestreamApp.API/DTOs/Profiles/ConfirmPhotoRequest.cs` + validator
- [ ] Step 91: Create `src/LivestreamApp.API/DTOs/Profiles/ReorderPhotosRequest.cs` + validator

### Phase 17: API Module — Unit Tests
- [ ] Step 92: Create `tests/LivestreamApp.Tests.Unit/API/AuthControllerTests.cs`
  - Test_Register_Success_Returns201
  - Test_Register_DuplicateEmail_Returns409
  - Test_Login_Success_SetsCookie
  - Test_Login_InvalidPassword_Returns401
  - Test_Refresh_Success_RotatesToken
- [ ] Step 93: Create `tests/LivestreamApp.Tests.Unit/API/ProfilesControllerTests.cs`
  - Test_GetProfile_Success_Returns200
  - Test_UpdateProfile_Success_Returns200
  - Test_PresignPhoto_Success_ReturnsUploadUrl
  - Test_ConfirmPhoto_Success_Returns201

### Phase 18: API Module — Summary Documentation
- [ ] Step 94: Create `aidlc-docs/construction/unit-1-core-foundation/code/api-module-summary.md`

### Phase 19: Database Migrations
- [ ] Step 95: Generate initial EF Core migration `dotnet ef migrations add InitialCreate`
- [ ] Step 96: Create `src/LivestreamApp.API/Infrastructure/Migrations/Scripts/seed-admin-user.sql` (optional)

### Phase 20: MockServices — Stripe Mock
- [ ] Step 97: Create `mock/LivestreamApp.MockServices/Program.cs` (minimal API)
- [ ] Step 98: Create `mock/LivestreamApp.MockServices/Controllers/StripeMockController.cs`
  - POST /mock/stripe/v1/payment_intents (create)
  - POST /mock/stripe/v1/payment_intents/{id}/confirm
  - POST /mock/stripe/v1/webhooks (trigger webhook manually)
- [ ] Step 99: Create `mock/LivestreamApp.MockServices/Models/StripePaymentIntent.cs`
- [ ] Step 100: Create `mock/LivestreamApp.MockServices/Models/StripeWebhookEvent.cs`

### Phase 21: MockServices — LINE Pay Mock
- [ ] Step 101: Create `mock/LivestreamApp.MockServices/Controllers/LinePayMockController.cs`
  - POST /mock/linepay/v3/payments/request
  - POST /mock/linepay/v3/payments/{transactionId}/confirm
  - GET /mock/linepay/v3/payments/{transactionId}
- [ ] Step 102: Create `mock/LivestreamApp.MockServices/Models/LinePayRequest.cs`
- [ ] Step 103: Create `mock/LivestreamApp.MockServices/Models/LinePayResponse.cs`

### Phase 22: MockServices — Unit Tests
- [ ] Step 104: Create `tests/LivestreamApp.Tests.Unit/MockServices/StripeMockTests.cs`
  - Test_CreatePaymentIntent_Success
  - Test_ConfirmPaymentIntent_Success
  - Test_TriggerWebhook_Success
- [ ] Step 105: Create `tests/LivestreamApp.Tests.Unit/MockServices/LinePayMockTests.cs`
  - Test_RequestPayment_Success
  - Test_ConfirmPayment_Success

### Phase 23: MockServices — Summary Documentation
- [ ] Step 106: Create `aidlc-docs/construction/unit-1-core-foundation/code/mockservices-summary.md`

### Phase 24: Infrastructure — Docker Compose
- [ ] Step 107: Create `docker-compose.yml` (PostgreSQL, Redis, LocalStack, MockServices)
- [ ] Step 108: Create `localstack-init/create-s3-bucket.sh`
- [ ] Step 109: Create `localstack-init/verify-ses-email.sh`
- [ ] Step 110: Create `.env.example` (environment variables template)

### Phase 25: Infrastructure — Dockerfile
- [ ] Step 111: Create `src/LivestreamApp.API/Dockerfile` (multi-stage build)
- [ ] Step 112: Create `mock/LivestreamApp.MockServices/Dockerfile`

### Phase 26: Frontend — PWA Project Setup
- [ ] Step 113: Create `frontend/pwa/package.json` (Next.js 14+, dependencies)
- [ ] Step 114: Create `frontend/pwa/next.config.js` (PWA config, i18n)
- [ ] Step 115: Create `frontend/pwa/tailwind.config.js`
- [ ] Step 116: Create `frontend/pwa/tsconfig.json`
- [ ] Step 117: Create `frontend/pwa/src/app/layout.tsx` (root layout, i18n provider)
- [ ] Step 118: Create `frontend/pwa/src/app/globals.css` (Tailwind imports)
- [ ] Step 119: Create `frontend/pwa/src/i18n/locales/ja.json` (Japanese translations)
- [ ] Step 120: Create `frontend/pwa/src/i18n/locales/en.json` (English translations)

### Phase 27: Frontend — PWA Auth Pages
- [ ] Step 121: Create `frontend/pwa/src/app/[locale]/register/page.tsx`
- [ ] Step 122: Create `frontend/pwa/src/app/[locale]/login/page.tsx`
- [ ] Step 123: Create `frontend/pwa/src/app/[locale]/login/line/callback/page.tsx` (LINE OAuth callback)
- [ ] Step 124: Create `frontend/pwa/src/app/[locale]/verify-email/page.tsx` (OTP input)
- [ ] Step 125: Create `frontend/pwa/src/app/[locale]/verify-phone/page.tsx` (OTP input)
- [ ] Step 126: Create `frontend/pwa/src/app/[locale]/reset-password/page.tsx`

### Phase 28: Frontend — PWA Profile Pages
- [ ] Step 127: Create `frontend/pwa/src/app/[locale]/profile/page.tsx` (view profile)
- [ ] Step 128: Create `frontend/pwa/src/app/[locale]/profile/edit/page.tsx` (edit profile)
- [ ] Step 129: Create `frontend/pwa/src/app/[locale]/profile/photos/page.tsx` (manage photos)

### Phase 29: Frontend — PWA State Management & API Client
- [ ] Step 130: Create `frontend/pwa/src/store/authStore.ts` (Zustand store)
- [ ] Step 131: Create `frontend/pwa/src/store/profileStore.ts`
- [ ] Step 132: Create `frontend/pwa/src/lib/api/client.ts` (Axios instance + interceptor)
- [ ] Step 133: Create `frontend/pwa/src/lib/api/auth.ts` (auth API calls)
- [ ] Step 134: Create `frontend/pwa/src/lib/api/profiles.ts` (profile API calls)

### Phase 30: Frontend — PWA Components
- [ ] Step 135: Create `frontend/pwa/src/components/AuthForm.tsx` (reusable form)
- [ ] Step 136: Create `frontend/pwa/src/components/OtpInput.tsx` (6-digit OTP)
- [ ] Step 137: Create `frontend/pwa/src/components/PhotoUploader.tsx` (presigned URL upload)
- [ ] Step 138: Create `frontend/pwa/src/components/LanguageSwitcher.tsx` (JP/EN toggle)

### Phase 31: Frontend — PWA Unit Tests
- [ ] Step 139: Create `frontend/pwa/src/__tests__/components/AuthForm.test.tsx` (Jest + React Testing Library)
- [ ] Step 140: Create `frontend/pwa/src/__tests__/components/OtpInput.test.tsx`
- [ ] Step 141: Create `frontend/pwa/src/__tests__/store/authStore.test.ts`

### Phase 32: Frontend — PWA Summary Documentation
- [ ] Step 142: Create `aidlc-docs/construction/unit-1-core-foundation/code/pwa-summary.md`

### Phase 33: Frontend — Admin Project Setup
- [ ] Step 143: Create `frontend/admin/package.json` (Next.js 14+, dependencies)
- [ ] Step 144: Create `frontend/admin/next.config.js`
- [ ] Step 145: Create `frontend/admin/tailwind.config.js`
- [ ] Step 146: Create `frontend/admin/tsconfig.json`
- [ ] Step 147: Create `frontend/admin/src/app/layout.tsx`
- [ ] Step 148: Create `frontend/admin/src/app/globals.css`

### Phase 34: Frontend — Admin Login Page
- [ ] Step 149: Create `frontend/admin/src/app/login/page.tsx` (admin login only)
- [ ] Step 150: Create `frontend/admin/src/app/dashboard/page.tsx` (placeholder dashboard)

### Phase 35: Frontend — Admin State & API
- [ ] Step 151: Create `frontend/admin/src/store/adminStore.ts`
- [ ] Step 152: Create `frontend/admin/src/lib/api/client.ts`
- [ ] Step 153: Create `frontend/admin/src/lib/api/auth.ts`

### Phase 36: Frontend — Admin Summary Documentation
- [ ] Step 154: Create `aidlc-docs/construction/unit-1-core-foundation/code/admin-summary.md`

### Phase 37: Documentation — Deployment Artifacts
- [ ] Step 155: Create `aidlc-docs/construction/unit-1-core-foundation/code/deployment-guide.md`
  - Docker Compose usage
  - ECS deployment steps
  - Environment variables reference
  - Health check endpoints

### Phase 38: Documentation — API Documentation
- [ ] Step 156: Create `aidlc-docs/construction/unit-1-core-foundation/code/api-reference.md`
  - All REST endpoints with request/response examples
  - Authentication flow diagrams
  - Error codes reference

### Phase 39: Documentation — Testing Guide
- [ ] Step 157: Create `aidlc-docs/construction/unit-1-core-foundation/code/testing-guide.md`
  - How to run unit tests
  - How to run integration tests (future)
  - Test coverage requirements (≥80%)
  - MockServices usage guide

### Phase 40: Final Verification
- [ ] Step 158: Verify all 7 stories implemented (US-01-01 through US-02-02)
- [ ] Step 159: Verify all Definition of Done criteria met
- [ ] Step 160: Generate final code generation summary

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

- [ ] Đăng ký, đăng nhập, LINE Login hoạt động end-to-end
- [ ] JWT refresh token rotation hoạt động
- [ ] Phone verification flow hoạt động
- [ ] Upload ảnh lên S3 (LocalStack) hoạt động
- [ ] MockServices trả về response đúng cho Stripe và LINE Pay
- [ ] Docker Compose khởi động toàn bộ stack thành công
- [ ] Unit tests cho Auth và Profiles modules (≥80% coverage)

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
