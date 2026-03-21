# Domain Entities — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Module**: `LivestreamApp.Auth` + `LivestreamApp.Profiles` + `LivestreamApp.Shared`  
**Ngày tạo**: 2026-03-21

---

## 1. Tổng Quan Domain Model

```
Users (1) ──────────── (0..1) HostProfiles
  │
  ├── (1) ──── (0..N) UserPhotos        [index 0-5, reorderable]
  ├── (1) ──── (0..N) RefreshTokens
  ├── (1) ──── (0..1) PhoneVerifications
  ├── (1) ──── (0..N) LoginAttempts     [brute-force tracking]
  ├── (1) ──── (0..N) OtpCodes          [email OTP + phone OTP]
  └── (1) ──── (0..N) ExternalLogins    [LINE OAuth linkage]
```

---

## 2. Core Entities

### 2.1 User

**Module**: `LivestreamApp.Auth`  
**Table**: `users`

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `Id` | `Guid` | No | Primary key (ULID-style) |
| `Email` | `string(255)` | No | Unique, lowercase normalized |
| `PasswordHash` | `string(255)` | Yes | Null nếu chỉ dùng LINE Login |
| `Role` | `UserRole` enum | No | `Viewer` / `Host` / `Admin` |
| `Status` | `UserStatus` enum | No | `Active` / `Suspended` / `Banned` / `PendingVerification` |
| `IsEmailVerified` | `bool` | No | Default: false |
| `IsPhoneVerified` | `bool` | No | Default: false — optional, chỉ hiển thị badge |
| `PhoneNumber` | `string(20)` | Yes | E.164 format (+81...) |
| `FailedLoginCount` | `int` | No | Default: 0 |
| `LockoutUntil` | `DateTime?` | Yes | Null = không bị lock |
| `RequiresCaptcha` | `bool` | No | True sau 5 lần sai |
| `LastLoginAt` | `DateTime?` | Yes | |
| `CreatedAt` | `DateTime` | No | UTC |
| `UpdatedAt` | `DateTime` | No | UTC |

**Enums**:
```csharp
enum UserRole    { Viewer, Host, Admin }
enum UserStatus  { PendingVerification, Active, Suspended, Banned }
```

---

### 2.2 HostProfile

**Module**: `LivestreamApp.Profiles`  
**Table**: `host_profiles`  
**Quan hệ**: 1-1 optional với `Users` (chỉ tồn tại khi `User.Role == Host`)

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `UserId` | `Guid` | No | FK → users.Id (PK đồng thời) |
| `IsVerified` | `bool` | No | Default: false — Admin manually approve |
| `VerifiedAt` | `DateTime?` | Yes | Thời điểm admin grant badge |
| `VerifiedByAdminId` | `Guid?` | Yes | FK → users.Id (Admin) |
| `VerificationRequestedAt` | `DateTime?` | Yes | Thời điểm Host submit request |
| `VerificationStatus` | `VerificationStatus` enum | No | `None` / `Pending` / `Approved` / `Rejected` |
| `VerificationNote` | `string(500)` | Yes | Admin note khi reject |
| `TotalCoinsReceived` | `long` | No | Default: 0 — denormalized counter |
| `TotalGiftsReceived` | `int` | No | Default: 0 |
| `CreatedAt` | `DateTime` | No | UTC |

**Enum**:
```csharp
enum VerificationStatus { None, Pending, Approved, Rejected }
```

---

### 2.3 UserProfile

**Module**: `LivestreamApp.Profiles`  
**Table**: `user_profiles`  
**Quan hệ**: 1-1 bắt buộc với `Users`

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `UserId` | `Guid` | No | FK → users.Id (PK đồng thời) |
| `DisplayName` | `string(100)` | No | Unique trong hệ thống |
| `DateOfBirth` | `DateOnly` | No | Bắt buộc khi tạo profile |
| `Bio` | `string(500)` | Yes | Giới thiệu bản thân |
| `Interests` | `string[]` | Yes | Mảng tags sở thích |
| `PreferredLanguage` | `string(5)` | No | Default: `ja` (ISO 639-1) |
| `IsProfileComplete` | `bool` | No | True khi có DisplayName + DateOfBirth |
| `CreatedAt` | `DateTime` | No | UTC |
| `UpdatedAt` | `DateTime` | No | UTC |

> **Lưu ý**: `DisplayName` unique — không có format restriction (cho phép tiếng Nhật, Latin, ký tự đặc biệt). Unique index trên `lower(display_name)` để case-insensitive uniqueness.

---

### 2.4 UserPhoto

**Module**: `LivestreamApp.Profiles`  
**Table**: `user_photos`

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `Id` | `Guid` | No | PK |
| `UserId` | `Guid` | No | FK → users.Id |
| `S3Key` | `string(500)` | No | S3 object key |
| `S3Url` | `string(1000)` | No | CloudFront URL |
| `DisplayIndex` | `int` | No | 0-5, unique per user |
| `FileSizeBytes` | `long` | No | |
| `MimeType` | `string(50)` | No | `image/jpeg`, `image/png`, `image/webp` |
| `UploadedAt` | `DateTime` | No | UTC |

**Constraints**:
- Max 6 ảnh per user (`DisplayIndex` 0-5)
- `DisplayIndex` = 0 là avatar chính
- Unique constraint: `(UserId, DisplayIndex)`

---

### 2.5 RefreshToken

**Module**: `LivestreamApp.Auth`  
**Table**: `refresh_tokens`

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `Id` | `Guid` | No | PK |
| `UserId` | `Guid` | No | FK → users.Id |
| `TokenHash` | `string(255)` | No | SHA-256 hash của token thực |
| `ExpiresAt` | `DateTime` | No | UTC, default: now + 30 ngày |
| `IsRevoked` | `bool` | No | Default: false |
| `RevokedAt` | `DateTime?` | Yes | |
| `ReplacedByTokenId` | `Guid?` | Yes | Token mới thay thế (rotation chain) |
| `CreatedAt` | `DateTime` | No | UTC |
| `DeviceInfo` | `string(255)` | Yes | User-Agent / device fingerprint |
| `IpAddress` | `string(45)` | Yes | IPv4/IPv6 |

---

### 2.6 OtpCode

**Module**: `LivestreamApp.Auth`  
**Table**: `otp_codes`

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `Id` | `Guid` | No | PK |
| `UserId` | `Guid?` | Yes | Null khi OTP gửi trước khi tạo account |
| `Target` | `string(255)` | No | Email hoặc phone number |
| `CodeHash` | `string(255)` | No | SHA-256 hash của OTP 6 số |
| `Purpose` | `OtpPurpose` enum | No | `EmailVerification` / `PhoneVerification` / `PasswordReset` |
| `ExpiresAt` | `DateTime` | No | UTC, default: now + 10 phút |
| `IsUsed` | `bool` | No | Default: false |
| `AttemptCount` | `int` | No | Default: 0, max: 3 |
| `CreatedAt` | `DateTime` | No | UTC |

**Enum**:
```csharp
enum OtpPurpose { EmailVerification, PhoneVerification, PasswordReset }
```

---

### 2.7 ExternalLogin

**Module**: `LivestreamApp.Auth`  
**Table**: `external_logins`

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `Id` | `Guid` | No | PK |
| `UserId` | `Guid` | No | FK → users.Id |
| `Provider` | `string(50)` | No | `LINE` / `Google` / `Apple` |
| `ProviderUserId` | `string(255)` | No | ID từ provider (LINE userId) |
| `ProviderEmail` | `string(255)` | Yes | Email từ provider (nếu có) |
| `AccessToken` | `string(2000)` | Yes | Encrypted, nullable sau khi không cần |
| `RefreshToken` | `string(2000)` | Yes | Encrypted |
| `TokenExpiresAt` | `DateTime?` | Yes | |
| `CreatedAt` | `DateTime` | No | UTC |
| `UpdatedAt` | `DateTime` | No | UTC |

**Unique constraint**: `(Provider, ProviderUserId)`

---

### 2.8 LoginAttempt

**Module**: `LivestreamApp.Auth`  
**Table**: `login_attempts`  
**Mục đích**: Tracking brute-force per email/IP

| Field | Type | Nullable | Mô tả |
|---|---|---|---|
| `Id` | `Guid` | No | PK |
| `Email` | `string(255)` | No | Lowercase normalized |
| `IpAddress` | `string(45)` | No | |
| `IsSuccess` | `bool` | No | |
| `FailureReason` | `string(100)` | Yes | `InvalidPassword` / `AccountLocked` / `AccountBanned` |
| `AttemptedAt` | `DateTime` | No | UTC |

> **Lưu ý**: Table này chỉ dùng để audit. Brute-force state (count, lockout) được lưu trực tiếp trên `Users` entity để tránh N+1 query.

---

## 3. Value Objects (Shared)

### 3.1 Email
```csharp
record Email(string Value)
{
    // Validation: RFC 5322, max 255 chars, lowercase normalized
    // Format: local@domain.tld
}
```

### 3.2 PhoneNumber
```csharp
record PhoneNumber(string Value)
{
    // E.164 format: +[country code][number]
    // Nhật Bản: +81XXXXXXXXXX
    // Max 20 chars
}
```

### 3.3 DisplayName
```csharp
record DisplayName(string Value)
{
    // Unique trong hệ thống (case-insensitive)
    // Không có format restriction (tiếng Nhật, Latin, ký tự đặc biệt)
    // Trim whitespace đầu/cuối
    // Không được rỗng
}
```

---

## 4. Domain Events

| Event | Trigger | Payload |
|---|---|---|
| `UserRegisteredEvent` | Sau khi tạo account thành công | `UserId`, `Email`, `CreatedAt` |
| `UserEmailVerifiedEvent` | Sau khi verify OTP email | `UserId`, `Email`, `VerifiedAt` |
| `UserPhoneVerifiedEvent` | Sau khi verify OTP phone | `UserId`, `PhoneNumber`, `VerifiedAt` |
| `UserLoggedInEvent` | Sau khi login thành công | `UserId`, `Provider`, `IpAddress`, `LoginAt` |
| `UserLockedOutEvent` | Sau khi bị lock do brute-force | `UserId`, `Email`, `LockoutUntil` |
| `PasswordResetCompletedEvent` | Sau khi reset password thành công | `UserId`, `ResetAt` |
| `ProfileUpdatedEvent` | Sau khi cập nhật profile | `UserId`, `UpdatedFields[]`, `UpdatedAt` |
| `PhotoUploadedEvent` | Sau khi upload ảnh thành công | `UserId`, `PhotoId`, `DisplayIndex`, `S3Key` |
| `HostVerificationRequestedEvent` | Khi Host submit verification request | `UserId`, `RequestedAt` |
| `HostVerifiedEvent` | Khi Admin approve verified badge | `UserId`, `AdminId`, `VerifiedAt` |
| `LineAccountLinkedEvent` | Khi LINE account được link/merge | `UserId`, `LineUserId`, `LinkedAt` |

---

## 5. Aggregate Boundaries

| Aggregate Root | Entities thuộc về |
|---|---|
| `User` | `RefreshToken[]`, `OtpCode[]`, `ExternalLogin[]`, `LoginAttempt[]` |
| `UserProfile` | `UserPhoto[]` |
| `HostProfile` | (standalone, reference User by ID) |

> **Lưu ý**: `UserProfile` và `HostProfile` là separate aggregates để tránh large aggregate. Chúng reference `User` bằng `UserId` (không phải navigation property trực tiếp trong domain layer).
