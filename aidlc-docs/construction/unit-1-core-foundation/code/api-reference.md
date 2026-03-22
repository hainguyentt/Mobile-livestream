# API Reference — Unit 1 Core Foundation

**Base URL**: `http://localhost:5000` (local) | `https://api.livestream.app` (production)  
**Authentication**: httpOnly cookie (`access_token`) set on login

---

## Authentication — `/api/v1/auth`

### POST /register
Đăng ký tài khoản mới bằng email.

**Request**
```json
{ "email": "user@example.com", "password": "Password123!" }
```
**Response 200**
```json
{ "message": "Registration successful. Please verify your email.", "userId": "uuid" }
```
**Response 409** — Email đã tồn tại

---

### POST /login
Đăng nhập bằng email/password. Set httpOnly cookies.

**Request**
```json
{ "email": "user@example.com", "password": "Password123!" }
```
**Response 200** — Sets `access_token` (15min) và `refresh_token` (30 days) cookies
```json
{ "message": "Login successful." }
```
**Response 401** — Sai credentials hoặc tài khoản bị khóa

---

### POST /login/line
Đăng nhập bằng LINE OAuth.

**Request**
```json
{ "code": "line_authorization_code" }
```
**Response 200** — Sets cookies giống `/login`

---

### POST /refresh
Rotate refresh token. Đọc từ `refresh_token` cookie.

**Response 200** — Sets cookies mới
**Response 401** — Token hết hạn hoặc bị revoke

---

### POST /logout
Revoke refresh token và xóa cookies.

**Auth**: Required (JWT)  
**Response 200**
```json
{ "message": "Logged out successfully." }
```

---

### POST /otp/email/send
Gửi OTP qua email.

**Request**
```json
{ "target": "user@example.com", "purpose": "EmailVerification" }
```
**Purpose values**: `EmailVerification`, `PasswordReset`

---

### POST /otp/email/verify
Xác minh OTP email.

**Request**
```json
{ "target": "user@example.com", "code": "123456", "purpose": "EmailVerification" }
```
**Response 200** — Verified  
**Response 400** — Invalid/expired OTP

---

### POST /otp/phone/send
Gửi OTP qua SMS. **Auth**: Required.

**Request**
```json
{ "target": "+819012345678", "purpose": "PhoneVerification" }
```

---

### POST /otp/phone/verify
Xác minh OTP phone. **Auth**: Required.

**Request**
```json
{ "target": "+819012345678", "code": "123456", "purpose": "PhoneVerification" }
```

---

### POST /password/reset
Đặt lại mật khẩu bằng OTP.

**Request**
```json
{ "email": "user@example.com", "newPassword": "NewPass123!", "otpCode": "123456" }
```

---

## Profiles — `/api/v1/profiles`

**Auth**: Required (JWT) cho tất cả endpoints.

### GET /me
Lấy profile của user hiện tại.

**Response 200**
```json
{
  "userId": "uuid",
  "displayName": "Tanaka Hanako",
  "bio": "Hello!",
  "interests": ["music", "travel"],
  "preferredLanguage": "ja",
  "isVerifiedHost": false,
  "photos": [
    { "id": "uuid", "displayIndex": 0, "s3Url": "https://...", "mimeType": "image/jpeg" }
  ]
}
```

---

### PUT /me
Cập nhật profile.

**Request**
```json
{ "bio": "Updated bio", "interests": ["music"], "preferredLanguage": "ja" }
```

---

### POST /photos/presign
Tạo presigned S3 URL để upload ảnh trực tiếp.

**Request**
```json
{ "displayIndex": 0, "contentType": "image/jpeg", "fileSizeBytes": 1048576 }
```
**Response 200**
```json
{ "uploadUrl": "https://s3.amazonaws.com/...", "photoId": "uuid" }
```

---

### POST /photos/confirm
Xác nhận upload hoàn tất sau khi PUT file lên S3.

**Request**
```json
{
  "photoId": "uuid",
  "s3Key": "photos/user-id/photo-id.jpg",
  "s3Url": "https://s3.amazonaws.com/...",
  "fileSizeBytes": 1048576,
  "mimeType": "image/jpeg"
}
```
**Response 201** — UserPhoto object

---

### DELETE /photos/{photoId}
Xóa ảnh.

**Response 204**

---

### PUT /photos/reorder
Sắp xếp lại thứ tự ảnh.

**Request**
```json
{ "orderedPhotoIds": ["uuid1", "uuid2", "uuid3"] }
```

---

## Host Verification — `/api/v1/host`

**Auth**: Required (JWT).

### POST /verification/request
Gửi yêu cầu xác minh host.

**Response 200**
```json
{ "message": "Verification request submitted.", "status": "Pending" }
```

---

### POST /verification/approve
Duyệt xác minh. **Auth**: Admin role required.

**Request**
```json
{ "userId": "uuid" }
```

---

### POST /verification/reject
Từ chối xác minh. **Auth**: Admin role required.

**Request**
```json
{ "userId": "uuid", "note": "Reason for rejection" }
```

---

## Error Response Format

Tất cả lỗi trả về RFC 7807 ProblemDetails:

```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Domain Error",
  "status": 400,
  "detail": "Email address is already registered.",
  "traceId": "00-abc123-def456-00"
}
```

### HTTP Status Codes
| Code | Meaning |
|---|---|
| 200 | Success |
| 201 | Created |
| 204 | No Content |
| 400 | Bad Request / Validation Error |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 409 | Conflict (duplicate) |
| 429 | Too Many Requests (rate limited) |
| 500 | Internal Server Error |
