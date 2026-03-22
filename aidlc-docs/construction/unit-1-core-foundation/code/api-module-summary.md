# API Module Summary — LivestreamApp.API

**Module**: LivestreamApp.API  
**Phase**: Construction — Unit 1 Core Foundation  
**Ngày hoàn thành**: 2026-03-22

---

## Tổng quan

`LivestreamApp.API` là entry point của toàn bộ backend. Module này chứa:
- ASP.NET Core Web API (controllers, middleware, DI setup)
- EF Core DbContext + entity configurations
- Infrastructure repositories (UnitOfWork, domain repositories)
- Infrastructure services (RedisCacheService)
- DTOs + FluentValidation validators

---

## Controllers

### AuthController — `/api/v1/auth`

| Method | Endpoint | Auth | Mô tả |
|---|---|---|---|
| POST | `/register` | Public | Đăng ký bằng email + password |
| POST | `/login` | Public | Đăng nhập, set httpOnly cookie |
| POST | `/login/line` | Public | LINE OAuth login |
| POST | `/refresh` | Cookie | Rotate refresh token |
| POST | `/logout` | JWT | Revoke token, xóa cookie |
| POST | `/otp/email/send` | Public | Gửi OTP qua email |
| POST | `/otp/email/verify` | Public | Xác minh OTP email |
| POST | `/otp/phone/send` | JWT | Gửi OTP qua SMS |
| POST | `/otp/phone/verify` | JWT | Xác minh OTP phone |
| POST | `/password/reset` | Public | Đặt lại mật khẩu bằng OTP |

### ProfilesController — `/api/v1/profiles`

| Method | Endpoint | Auth | Mô tả |
|---|---|---|---|
| GET | `/me` | JWT | Lấy profile của user hiện tại |
| PUT | `/me` | JWT | Cập nhật bio, interests, language |
| POST | `/photos/presign` | JWT | Tạo presigned S3 upload URL |
| POST | `/photos/confirm` | JWT | Xác nhận upload hoàn tất |
| DELETE | `/photos/{photoId}` | JWT | Xóa ảnh |
| PUT | `/photos/reorder` | JWT | Sắp xếp lại thứ tự ảnh |

### HostVerificationController — `/api/v1/host`

| Method | Endpoint | Auth | Mô tả |
|---|---|---|---|
| POST | `/verification/request` | JWT | Gửi yêu cầu xác minh host |
| POST | `/verification/approve` | Admin | Duyệt xác minh |
| POST | `/verification/reject` | Admin | Từ chối xác minh |

---

## Infrastructure

### AppDbContext

Dual context pattern:
- `AppDbContext` — write operations (INSERT/UPDATE/DELETE)
- `ReadOnlyDbContext` — read operations (SELECT, NoTracking)

DbSets: `Users`, `RefreshTokens`, `OtpCodes`, `ExternalLogins`, `LoginAttempts`, `UserProfiles`, `HostProfiles`, `UserPhotos`

### Entity Configurations (8 files)

| File | Entity |
|---|---|
| `UserConfiguration.cs` | User (email unique index, cascade refresh tokens) |
| `RefreshTokenConfiguration.cs` | RefreshToken (token hash unique index) |
| `OtpCodeConfiguration.cs` | OtpCode (composite index email+purpose) |
| `ExternalLoginConfiguration.cs` | ExternalLogin (provider+providerUserId unique) |
| `LoginAttemptConfiguration.cs` | LoginAttempt (append-only) |
| `UserProfileConfiguration.cs` | UserProfile (displayName unique, 1-1 with User) |
| `HostProfileConfiguration.cs` | HostProfile (1-1 with UserProfile) |
| `UserPhotoConfiguration.cs` | UserPhoto (composite unique userId+displayIndex) |

### Repositories

| Repository | Interface |
|---|---|
| `UnitOfWork` | `IUnitOfWork` |
| `UserRepository` | `IUserRepository` |
| `OtpRepository` | `IOtpRepository` |
| `ProfileRepository` | `IProfileRepository` |
| `HostProfileRepository` | `IHostProfileRepository` |
| `PhotoRepository` | `IPhotoRepository` |

### Services

| Service | Interface |
|---|---|
| `RedisCacheService` | `ICacheService` |

---

## Middleware

| Middleware | Chức năng |
|---|---|
| `ExceptionHandlingMiddleware` | Bắt tất cả exceptions, trả về RFC 7807 ProblemDetails |
| `RateLimitingMiddleware` | Per-IP rate limiting dùng Redis sliding window |

---

## DTOs & Validators

### Auth DTOs
- `RegisterRequest` — email, password (min 8 chars, uppercase, digit, special)
- `LoginRequest` — email, password
- `RefreshTokenRequest` — refreshToken
- `SendOtpRequest` — target (email/phone), purpose
- `VerifyOtpRequest` — target, code (6 digits), purpose
- `ResetPasswordRequest` — email, newPassword, otpCode

### Profiles DTOs
- `UpdateProfileRequest` — bio (max 500), interests (max 10), preferredLanguage
- `PresignPhotoRequest` — displayIndex (0-5), contentType, fileSizeBytes (max 10MB)
- `ConfirmPhotoRequest` — photoId, s3Key, s3Url, fileSizeBytes, mimeType
- `ReorderPhotosRequest` — orderedPhotoIds (array of Guid)

---

## DI Registration

`ServiceCollectionExtensions` đăng ký:
- JWT Authentication + Authorization
- EF Core (AppDbContext + ReadOnlyDbContext)
- Redis (StackExchange.Redis)
- AWS S3 (AWSSDK.S3)
- FluentValidation validators
- All repositories và services

---

## Configuration

| File | Môi trường |
|---|---|
| `appsettings.json` | Base config (JWT, EFCore slow query threshold) |
| `appsettings.Development.json` | LocalStack (S3, SES), local PostgreSQL, Redis |
| `appsettings.Production.json` | AWS (S3, SES), RDS, ElastiCache |

---

## Build Status

- Build: ✅ 0 errors, 0 warnings
- Unit Tests: ✅ 30/30 passing (Auth: 14, Profiles: 16)
