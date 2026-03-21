# Business Logic Model — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Ngày tạo**: 2026-03-21

---

## 1. Email Registration Flow (US-01-01)

```
Client                          AuthService                     EmailService        DB
  |                                 |                               |               |
  |-- POST /auth/register --------> |                               |               |
  |   {email, password}             |                               |               |
  |                                 |-- Validate email format       |               |
  |                                 |-- Check email unique ---------|-------------> |
  |                                 |<--------------------------------- email exists?|
  |                                 |                               |               |
  |   [IF email exists]             |                               |               |
  |<-- 409 Conflict --------------- |                               |               |
  |                                 |                               |               |
  |   [IF email new]                |                               |               |
  |                                 |-- Hash password (BCrypt 12)   |               |
  |                                 |-- Create User (PendingVerif.) |-------------> |
  |                                 |-- Generate OTP (6 digits)     |               |
  |                                 |-- Hash OTP + Save OtpCode ----|-------------> |
  |                                 |-- Send OTP email ------------> |               |
  |<-- 200 OK {message: "OTP sent"} |                               |               |
  |                                 |                               |               |
  |-- POST /auth/verify-email ----> |                               |               |
  |   {email, otp}                  |                               |               |
  |                                 |-- Load OtpCode by email ------|-------------> |
  |                                 |-- Check: not expired          |               |
  |                                 |-- Check: not used             |               |
  |                                 |-- Check: attempts < 3         |               |
  |                                 |-- Compare hash(otp)           |               |
  |                                 |                               |               |
  |   [IF invalid OTP]              |                               |               |
  |                                 |-- Increment AttemptCount -----|-------------> |
  |<-- 400 Bad Request ------------ |                               |               |
  |                                 |                               |               |
  |   [IF valid OTP]                |                               |               |
  |                                 |-- Mark OTP used               |               |
  |                                 |-- User.Status = Active        |               |
  |                                 |-- User.IsEmailVerified = true |               |
  |                                 |-- Save changes ---------------|-------------> |
  |                                 |-- Issue JWT + RefreshToken    |               |
  |                                 |-- Publish UserRegisteredEvent |               |
  |                                 |-- Publish UserEmailVerifiedEvent              |
  |<-- 200 OK {accessToken, user} - |                               |               |
  |   (Set httpOnly Cookie)         |                               |               |
```

---

## 2. Email/Password Login Flow (US-01-02)

```
Client                          AuthService                         DB
  |                                 |                               |
  |-- POST /auth/login -----------> |                               |
  |   {email, password, captchaToken?}                             |
  |                                 |-- Normalize email             |
  |                                 |-- Load User by email -------> |
  |                                 |<------------------------------ User?
  |                                 |                               |
  |   [IF user not found]           |                               |
  |<-- 401 Unauthorized ----------- |  (generic error, no leak)    |
  |                                 |                               |
  |   [IF user found]               |                               |
  |                                 |-- Check Status (Active?)      |
  |                                 |-- Check LockoutUntil          |
  |                                 |-- Check RequiresCaptcha       |
  |                                 |   → Validate captchaToken     |
  |                                 |-- BCrypt.Verify(password)     |
  |                                 |                               |
  |   [IF password wrong]           |                               |
  |                                 |-- FailedLoginCount++          |
  |                                 |-- Log LoginAttempt (fail) --> |
  |                                 |-- [IF count >= 5]             |
  |                                 |     RequiresCaptcha = true    |
  |                                 |-- [IF count >= 10]            |
  |                                 |     LockoutUntil = +24h       |
  |                                 |     Send lockout email        |
  |                                 |-- Save changes -------------> |
  |<-- 401 Unauthorized ----------- |                               |
  |                                 |                               |
  |   [IF password correct]         |                               |
  |                                 |-- FailedLoginCount = 0        |
  |                                 |-- RequiresCaptcha = false     |
  |                                 |-- LastLoginAt = now           |
  |                                 |-- Issue JWT (15min)           |
  |                                 |-- Issue RefreshToken (30d)    |
  |                                 |-- Log LoginAttempt (success)->|
  |                                 |-- Publish UserLoggedInEvent   |
  |<-- 200 OK {user} -------------- |                               |
  |   (Set httpOnly Cookie)         |                               |
```

---

## 3. LINE Login Flow (US-01-03)

```
Client                    AuthService              LINE OAuth              DB
  |                           |                        |                   |
  |-- GET /auth/line/login --> |                        |                   |
  |                           |-- Generate state nonce |                   |
  |<-- 302 Redirect --------- |                        |                   |
  |   (LINE OAuth URL)        |                        |                   |
  |                           |                        |                   |
  |-- [User logs in LINE] --> |                        |                   |
  |                           |                        |                   |
  |-- GET /auth/line/callback |                        |                   |
  |   {code, state}           |                        |                   |
  |                           |-- Verify state nonce   |                   |
  |                           |-- Exchange code -----> |                   |
  |                           |<-- {access_token,      |                   |
  |                           |     id_token, profile} |                   |
  |                           |                        |                   |
  |                           |-- Extract: lineUserId, email, displayName  |
  |                           |                        |                   |
  |                           |-- Check ExternalLogin by lineUserId -----> |
  |                           |<------------------------------------------ exists?
  |                           |                        |                   |
  |   [IF ExternalLogin exists → User already linked]  |                   |
  |                           |-- Load User            |                   |
  |                           |-- Issue JWT + RefreshToken                 |
  |<-- 200 OK {user} -------- |                        |                   |
  |                           |                        |                   |
  |   [IF ExternalLogin NOT exists]                    |                   |
  |                           |-- Check User by email (if LINE provides email)
  |                           |                        |                   |
  |   [IF email matches existing User]                 |                   |
  |                           |-- Create ExternalLogin (link) -----------> |
  |                           |-- Publish LineAccountLinkedEvent           |
  |                           |-- Issue JWT + RefreshToken                 |
  |<-- 200 OK {user} -------- |                        |                   |
  |                           |                        |                   |
  |   [IF no email match → New User]                   |                   |
  |                           |-- Create User (Active, PasswordHash=null)  |
  |                           |-- Create ExternalLogin -----------------> |
  |                           |-- Create UserProfile (DisplayName from LINE)|
  |                           |-- Issue JWT + RefreshToken                 |
  |                           |-- Publish UserRegisteredEvent              |
  |<-- 200 OK {user,          |                        |                   |
  |    isNewUser: true} ------ |                       |                   |
  |   (Client shows Onboarding)|                       |                   |
```

---

## 4. Password Reset Flow (US-01-05)

```
Client                    AuthService              EmailService        DB
  |                           |                        |               |
  |-- POST /auth/forgot-pass->|                        |               |
  |   {email}                 |                        |               |
  |                           |-- Rate limit check (3/hour per email) |
  |                           |-- Load User by email --|-------------> |
  |                           |-- [Always return 200 regardless]      |
  |                           |                        |               |
  |   [IF user exists]        |                        |               |
  |                           |-- Generate signed token (HMAC-SHA256) |
  |                           |-- Token expires: +1h   |               |
  |                           |-- Save token hash -----|-------------> |
  |                           |-- Send reset email --> |               |
  |<-- 200 OK {message} ----- |                        |               |
  |                           |                        |               |
  |-- POST /auth/reset-pass ->|                        |               |
  |   {token, newPassword}    |                        |               |
  |                           |-- Verify token signature              |
  |                           |-- Check token not expired             |
  |                           |-- Check token not used                |
  |                           |-- Validate new password rules         |
  |                           |-- Hash new password (BCrypt 12)       |
  |                           |-- Update User.PasswordHash ---------->|
  |                           |-- Mark token used -----|-------------> |
  |                           |-- Revoke ALL RefreshTokens for user -> |
  |                           |-- Publish PasswordResetCompletedEvent |
  |<-- 200 OK --------------- |                        |               |
```

---

## 5. Profile Management Flow (US-02-01)

```
Client                    ProfileService              S3 (LocalStack)     DB
  |                           |                           |               |
  |-- GET /profiles/me -----> |                           |               |
  |                           |-- Load UserProfile + Photos by UserId --> |
  |<-- 200 OK {profile} ----- |                           |               |
  |                           |                           |               |
  |-- PUT /profiles/me -----> |                           |               |
  |   {displayName, dob, bio, interests}                  |               |
  |                           |-- Validate DisplayName unique ----------> |
  |                           |-- Validate DateOfBirth (18+)             |
  |                           |-- Update UserProfile ------------------- >|
  |                           |-- Publish ProfileUpdatedEvent            |
  |<-- 200 OK {profile} ----- |                           |               |
  |                           |                           |               |
  |-- POST /profiles/photos ->|                           |               |
  |   {file, displayIndex}    |                           |               |
  |                           |-- Validate: max 6 photos                 |
  |                           |-- Validate: file type + size             |
  |                           |-- Resize image (max 1080x1080)           |
  |                           |-- Generate S3 key                        |
  |                           |-- Upload to S3 ---------> |               |
  |                           |<-- S3 URL                 |               |
  |                           |-- [IF index exists] Delete old S3 object |
  |                           |-- Save/Update UserPhoto ----------------> |
  |                           |-- Publish PhotoUploadedEvent             |
  |<-- 200 OK {photo} ------- |                           |               |
  |                           |                           |               |
  |-- DELETE /profiles/photos/{index}                     |               |
  |                           |-- Load photo by (UserId, index) -------> |
  |                           |-- Delete from S3 -------> |               |
  |                           |-- Delete UserPhoto record --------------> |
  |<-- 204 No Content ------- |                           |               |
```

---

## 6. Host Verified Badge Flow (US-02-02)

```
Host Client               ProfileService              Admin Client        DB
  |                           |                           |               |
  |-- POST /profiles/verify-request                       |               |
  |                           |-- Check Role = Host       |               |
  |                           |-- Check no pending request -------------> |
  |                           |-- Create/Update HostProfile               |
  |                           |   VerificationStatus = Pending            |
  |                           |   VerificationRequestedAt = now -------> |
  |                           |-- Publish HostVerificationRequestedEvent  |
  |<-- 200 OK --------------- |                           |               |
  |                           |                           |               |
  |                           |                           |               |
  |                           |<-- GET /admin/verifications (Admin)       |
  |                           |-- Load pending requests ----------------> |
  |                           |-- Return list ----------> |               |
  |                           |                           |               |
  |                           |<-- POST /admin/verifications/{userId}/approve
  |                           |-- Update HostProfile:     |               |
  |                           |   IsVerified = true       |               |
  |                           |   VerificationStatus = Approved           |
  |                           |   VerifiedAt = now        |               |
  |                           |   VerifiedByAdminId = adminId ----------> |
  |                           |-- Publish HostVerifiedEvent               |
  |                           |-- [Notification to Host via NotificationHub - Unit 4]
  |                           |-- Return 200 -----------> |               |
```

---

## 7. Refresh Token Rotation Flow

```
Client                    AuthService                                 DB
  |                           |                                       |
  |-- POST /auth/refresh ----> |                                      |
  |   (httpOnly Cookie)        |                                      |
  |                           |-- Extract refresh token from cookie   |
  |                           |-- Hash token (SHA-256)                |
  |                           |-- Load RefreshToken by hash --------> |
  |                           |                                       |
  |   [IF token not found]    |                                       |
  |<-- 401 Unauthorized ------ |                                      |
  |                           |                                       |
  |   [IF token revoked]      |                                       |
  |                           |-- REUSE DETECTED                      |
  |                           |-- Revoke ALL tokens for user -------> |
  |<-- 401 Unauthorized ------ |                                      |
  |                           |                                       |
  |   [IF token valid]        |                                       |
  |                           |-- Revoke old token                    |
  |                           |-- Issue new JWT (15min)               |
  |                           |-- Issue new RefreshToken (30d)        |
  |                           |   ReplacedByTokenId = old.Id          |
  |                           |-- Save new token ------------------- >|
  |<-- 200 OK {user} -------- |                                       |
  |   (Set new httpOnly Cookie)|                                      |
```

---

## 8. MockServices — Stripe Skeleton

**Endpoint**: `POST /stripe/v1/payment_intents`

```
Request:
{
  "amount": 100000,        // cents (1000¥ = 100000)
  "currency": "jpy",
  "payment_method": "pm_card_visa"
}

Response (success):
{
  "id": "pi_mock_{guid}",
  "object": "payment_intent",
  "amount": 100000,
  "currency": "jpy",
  "status": "succeeded",
  "client_secret": "pi_mock_{guid}_secret_{guid}"
}
```

**Webhook**: `POST /stripe/webhooks`
```
Event: payment_intent.succeeded
{
  "type": "payment_intent.succeeded",
  "data": { "object": { "id": "pi_mock_{guid}", "status": "succeeded" } }
}
```

---

## 9. MockServices — LINE Pay Skeleton

**Endpoint**: `POST /linepay/v3/payments/request`
```
Response:
{
  "returnCode": "0000",
  "returnMessage": "Success",
  "info": {
    "transactionId": 2024000000000000,
    "paymentUrl": { "web": "http://localhost:5001/linepay/mock-payment-page" }
  }
}
```

**Endpoint**: `POST /linepay/v3/payments/{transactionId}/confirm`
```
Response:
{
  "returnCode": "0000",
  "returnMessage": "Success",
  "info": {
    "transactionId": 2024000000000000,
    "orderId": "order_{guid}",
    "payInfo": [{ "method": "BALANCE", "amount": 1000 }]
  }
}
```
