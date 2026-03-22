# Auth Module — Code Summary

**Unit**: Unit 1 — Core Foundation
**Status**: Complete
**Tests**: 12/12 passing

## Generated Files

### Domain Entities
- `app/backend/LivestreamApp.Auth/Domain/Entities/User.cs`
- `app/backend/LivestreamApp.Auth/Domain/Entities/RefreshToken.cs`
- `app/backend/LivestreamApp.Auth/Domain/Entities/OtpCode.cs`
- `app/backend/LivestreamApp.Auth/Domain/Entities/ExternalLogin.cs`
- `app/backend/LivestreamApp.Auth/Domain/Entities/LoginAttempt.cs`

### Services
- `app/backend/LivestreamApp.Auth/Services/IAuthService.cs`
- `app/backend/LivestreamApp.Auth/Services/AuthService.cs`
- `app/backend/LivestreamApp.Auth/Services/ILineOAuthService.cs`
- `app/backend/LivestreamApp.Auth/Services/LineOAuthService.cs`
- `app/backend/LivestreamApp.Auth/Services/IEmailService.cs`
- `app/backend/LivestreamApp.Auth/Services/ISmsService.cs`

### Repositories
- `app/backend/LivestreamApp.Auth/Repositories/IUserRepository.cs`
- `app/backend/LivestreamApp.Auth/Repositories/IOtpRepository.cs`

### Options
- `app/backend/LivestreamApp.Auth/Options/JwtOptions.cs`
- `app/backend/LivestreamApp.Auth/Options/LineOptions.cs`

### Tests
- `app/tests/LivestreamApp.Tests.Unit/Auth/AuthServiceTests.cs` (11 tests)
- `app/tests/LivestreamApp.Tests.Unit/Auth/LineOAuthServiceTests.cs` (3 tests)

## Business Logic Summary

| Method | Story | Description |
|---|---|---|
| RegisterWithEmailAsync | US-01-01 | Create user, hash password, send email OTP |
| LoginWithEmailAsync | US-01-02 | Verify password, issue JWT + refresh token |
| LoginWithLineAsync | US-01-03 | Exchange LINE code, link/merge account |
| VerifyEmailOtpAsync | US-01-01 | Verify 6-digit OTP, activate account |
| VerifyPhoneOtpAsync | US-01-04 | Verify phone OTP |
| ResetPasswordAsync | US-01-05 | Verify OTP then update password hash |
| RefreshAccessTokenAsync | US-01-02 | Rotate refresh token, issue new access token |

## Security Notes
- Passwords: BCrypt work factor 12
- Refresh tokens: SHA-256 hashed before storage
- OTP: 6-digit, 10-min expiry, max 3 attempts
- Account lockout: after 10 failed logins (30 min)
- Captcha required: after 5 failed logins
