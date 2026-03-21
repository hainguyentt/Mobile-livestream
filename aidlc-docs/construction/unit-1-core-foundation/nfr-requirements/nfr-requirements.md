# NFR Requirements — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Ngày tạo**: 2026-03-21  
**Nguồn**: requirements.md v1.3 + NFR Q&A session

---

## 1. Performance Requirements

### NFR-U1-PERF-01: API Response Time
| ID | Requirement | Target | Scope |
|---|---|---|---|
| NFR-U1-PERF-01-1 | Auth endpoints (login, register, OTP verify) | < 200ms p95 | Không tính BCrypt hash time |
| NFR-U1-PERF-01-2 | Profile read endpoints (GET /profiles/me) | < 100ms p95 | Với Redis cache hit |
| NFR-U1-PERF-01-3 | Profile write endpoints (PUT /profiles/me) | < 200ms p95 | |
| NFR-U1-PERF-01-4 | S3 presigned URL generation | < 50ms | Không tính upload time |
| NFR-U1-PERF-01-5 | BCrypt password hash (cost=12) | ~300-500ms | Acceptable — security trade-off |

> **Lưu ý**: BCrypt cost=12 intentionally slow (~300-500ms). Không tính vào 200ms SLA — đây là security requirement, không phải bug.

### NFR-U1-PERF-02: Database Connection Pool
| ID | Requirement |
|---|---|
| NFR-U1-PERF-02-1 | Dùng Npgsql default connection pool (min=1, max=100 per process) |
| NFR-U1-PERF-02-2 | Monitor pool exhaustion qua CloudWatch metrics |
| NFR-U1-PERF-02-3 | Điều chỉnh `MaxPoolSize` nếu pool exhaustion xảy ra trong production |

### NFR-U1-PERF-03: Redis Cache
| ID | Requirement | TTL |
|---|---|---|
| NFR-U1-PERF-03-1 | Cache blacklisted refresh tokens (revoked) | Bằng token expiry (30 ngày) |
| NFR-U1-PERF-03-2 | Cache user profile data (`user:profile:{userId}`) | 15 phút (configurable qua config) |
| NFR-U1-PERF-03-3 | Cache invalidation khi profile update | Immediate (delete key) |
| NFR-U1-PERF-03-4 | Cache TTL configurable qua `appsettings.json` — không hardcode | — |

**Redis key schema**:
```
revoked_token:{tokenHash}     → "1"  (TTL = token expiry)
user:profile:{userId}         → JSON (TTL = 15 phút)
```

---

## 2. Scalability Requirements

### NFR-U1-SCALE-01: Stateless Design
| ID | Requirement |
|---|---|
| NFR-U1-SCALE-01-1 | API service phải stateless — không lưu session state in-memory |
| NFR-U1-SCALE-01-2 | Tất cả state lưu trong PostgreSQL hoặc Redis |
| NFR-U1-SCALE-01-3 | Có thể chạy nhiều instances song song (ECS Fargate horizontal scaling) |
| NFR-U1-SCALE-01-4 | JWT validation không cần gọi DB (self-contained token) |

### NFR-U1-SCALE-02: Database
| ID | Requirement |
|---|---|
| NFR-U1-SCALE-02-1 | PostgreSQL RDS với Multi-AZ cho production |
| NFR-U1-SCALE-02-2 | Read replica khi DAU > 5,000 (không cần ở MVP) |
| NFR-U1-SCALE-02-3 | EF Core migrations auto-apply khi startup — idempotent |

---

## 3. Availability & Reliability Requirements

### NFR-U1-AVAIL-01: Uptime
| ID | Requirement |
|---|---|
| NFR-U1-AVAIL-01-1 | Target uptime: 99.9% (< 8.7 giờ downtime/năm) |
| NFR-U1-AVAIL-01-2 | ECS Fargate với minimum 2 tasks running (no single point of failure) |
| NFR-U1-AVAIL-01-3 | Rolling deployment — zero downtime deploy |

### NFR-U1-AVAIL-02: Health Checks
| Endpoint | Type | Check | ECS Probe |
|---|---|---|---|
| `GET /health/live` | Liveness | App process alive | Liveness probe (fail → restart container) |
| `GET /health/ready` | Readiness | DB connection + Redis connection | Readiness probe (fail → remove from load balancer) |
| `GET /health/startup` | Startup | DB migrations complete + app initialized | Startup probe (allow slow startup) |

**Startup probe config** (ECS):
```
initialDelaySeconds: 30   # cho EF migrations chạy xong
periodSeconds: 10
failureThreshold: 6       # 60 giây timeout tổng
```

### NFR-U1-AVAIL-03: Email Service Reliability
| ID | Requirement |
|---|---|
| NFR-U1-AVAIL-03-1 | AWS SES retry: 3 lần với exponential backoff (1s → 2s → 4s) |
| NFR-U1-AVAIL-03-2 | Nếu tất cả retry fail → trả lỗi rõ ràng cho user (không fail silently) |
| NFR-U1-AVAIL-03-3 | Log tất cả SES failures vào CloudWatch với alert |
| NFR-U1-AVAIL-03-4 | User có thể request lại OTP sau khi nhận lỗi |

### NFR-U1-AVAIL-04: Data Backup
| ID | Requirement |
|---|---|
| NFR-U1-AVAIL-04-1 | RDS automated backup hàng ngày, retention 30 ngày |
| NFR-U1-AVAIL-04-2 | Point-in-time recovery (PITR) enabled |

---

## 4. Security Requirements

### NFR-U1-SEC-01: Authentication & Token Security
| ID | Requirement |
|---|---|
| NFR-U1-SEC-01-1 | JWT Access Token: HS256 hoặc RS256, expiry 15 phút |
| NFR-U1-SEC-01-2 | Refresh Token: 30 ngày, rotation on use, reuse detection |
| NFR-U1-SEC-01-3 | Tokens lưu trong `httpOnly Cookie` với flags: `HttpOnly`, `Secure`, `SameSite=Strict` |
| NFR-U1-SEC-01-4 | Không lưu tokens trong `localStorage` hoặc `sessionStorage` |
| NFR-U1-SEC-01-5 | Revoked tokens cached trong Redis để tránh DB lookup |

### NFR-U1-SEC-02: Password Security
| ID | Requirement |
|---|---|
| NFR-U1-SEC-02-1 | BCrypt hash với cost factor = 12 |
| NFR-U1-SEC-02-2 | Không log raw passwords ở bất kỳ đâu |
| NFR-U1-SEC-02-3 | Password min 8 chars, 1 uppercase, 1 lowercase, 1 digit |

### NFR-U1-SEC-03: Rate Limiting
| Endpoint Group | Per-IP Limit | Global Limit | Window |
|---|---|---|---|
| Auth endpoints (`/auth/*`) | 20 req | 1,000 req | 1 phút |
| OTP endpoints (`/auth/otp/*`, `/auth/phone/*`) | 5 req | 200 req | 1 phút |
| Password reset (`/auth/forgot-password`) | 3 req | 100 req | 1 giờ |
| Profile endpoints (`/profiles/*`) | 60 req | 5,000 req | 1 phút |
| Upload endpoints (`/profiles/photos`) | 10 req | 500 req | 1 phút |

> **Note**: Nâng cấp Per-User rate limiting cho protected endpoints trong Phase 2 khi có NAT issue.

### NFR-U1-SEC-04: Data Encryption
| ID | Requirement |
|---|---|
| NFR-U1-SEC-04-1 | TLS 1.3 cho tất cả in-transit communication |
| NFR-U1-SEC-04-2 | RDS encryption at-rest (AES-256) |
| NFR-U1-SEC-04-3 | S3 server-side encryption (SSE-S3) |
| NFR-U1-SEC-04-4 | LINE OAuth access tokens encrypted trước khi lưu DB (AES-256-GCM) |
| NFR-U1-SEC-04-5 | Refresh token lưu dạng SHA-256 hash, không lưu raw value |

### NFR-U1-SEC-05: APPI Compliance
| ID | Requirement |
|---|---|
| NFR-U1-SEC-05-1 | `LoginAttempts` data retention: 90 ngày → auto-delete qua Hangfire job |
| NFR-U1-SEC-05-2 | Account deletion: Soft delete 30 ngày (grace period) → Anonymize PII |
| NFR-U1-SEC-05-3 | Anonymization: email → SHA-256 hash, displayName → "Deleted User #{id}", phone → null |
| NFR-U1-SEC-05-4 | Transaction history giữ lại sau anonymize (financial audit requirement) |
| NFR-U1-SEC-05-5 | User có thể export dữ liệu cá nhân (APPI Article 33) — implement ở Unit 5 |
| NFR-U1-SEC-05-6 | Privacy Policy URL hiển thị trên Register screen |

### NFR-U1-SEC-06: Input Validation & CORS
| ID | Requirement |
|---|---|
| NFR-U1-SEC-06-1 | FluentValidation cho tất cả request DTOs |
| NFR-U1-SEC-06-2 | CORS whitelist: PWA domain + Admin domain only |
| NFR-U1-SEC-06-3 | S3 presigned URL: max expiry 5 phút, chỉ cho phép PUT method |
| NFR-U1-SEC-06-4 | File upload: validate MIME type server-side (không trust Content-Type header) |

### NFR-U1-SEC-07: Admin Audit Log
| ID | Requirement |
|---|---|
| NFR-U1-SEC-07-1 | Tất cả admin actions lưu vào `AdminActionLog` table |
| NFR-U1-SEC-07-2 | Log fields: `AdminId`, `Action`, `TargetUserId`, `Details` (JSON), `IpAddress`, `Timestamp` |
| NFR-U1-SEC-07-3 | Audit log không được xóa (append-only) |
| NFR-U1-SEC-07-4 | Retention: 1 năm (APPI compliance) |

---

## 5. Usability Requirements

### NFR-U1-USE-01: Internationalization
| ID | Requirement |
|---|---|
| NFR-U1-USE-01-1 | PWA hỗ trợ tiếng Nhật (ja) và tiếng Anh (en) |
| NFR-U1-USE-01-2 | Default locale: `ja` |
| NFR-U1-USE-01-3 | Tất cả error messages phải có bản dịch JP/EN |
| NFR-U1-USE-01-4 | Date format: `YYYY年MM月DD日` (JP), `MM/DD/YYYY` (EN) |
| NFR-U1-USE-01-5 | Font hỗ trợ Hiragana, Katakana, Kanji (Noto Sans JP) |

### NFR-U1-USE-02: PWA Requirements
| ID | Requirement |
|---|---|
| NFR-U1-USE-02-1 | PWA installable trên iOS Safari và Android Chrome |
| NFR-U1-USE-02-2 | First load < 3 giây trên 4G (Next.js code splitting + CDN) |
| NFR-U1-USE-02-3 | Responsive: mobile-first (360px+), tablet, desktop |
| NFR-U1-USE-02-4 | Dark mode support (CSS media query `prefers-color-scheme`) |

---

## 6. Maintainability Requirements

### NFR-U1-MAINT-01: Code Quality
| ID | Requirement |
|---|---|
| NFR-U1-MAINT-01-1 | Unit test coverage ≥ 80% cho Auth và Profiles modules |
| NFR-U1-MAINT-01-2 | Integration tests cho tất cả API endpoints |
| NFR-U1-MAINT-01-3 | EF Core migrations: auto-apply khi startup, idempotent |

### NFR-U1-MAINT-02: Logging
| Environment | Sinks | Min Level |
|---|---|---|
| Development / Test | Console (colored) + File (rolling daily, 7 ngày) | Debug |
| Production (ECS) | AWS CloudWatch Logs (`/livestream-app/api`) | Information |

**Log format**:
- Dev: `[HH:mm:ss LEVEL] Message {Exception}`
- Prod: JSON structured (`timestamp`, `level`, `message`, `requestId`, `userId`, `exception`)

**Sensitive data**: Không log passwords, OTP codes, tokens, credit card numbers.

### NFR-U1-MAINT-03: Background Jobs (Hangfire)
| Job | Schedule | Mục đích |
|---|---|---|
| `PurgeExpiredLoginAttempts` | Daily 03:00 JST | Xóa LoginAttempts > 90 ngày |
| `ProcessPendingAccountDeletions` | Daily 02:00 JST | Anonymize accounts đã soft-delete > 30 ngày |

---

## 7. Compliance Summary (Security Baseline Extension)

| Rule | Status | Notes |
|---|---|---|
| SEC-01: Password hashing | ✅ Compliant | BCrypt cost=12 |
| SEC-02: No sensitive data in logs | ✅ Compliant | Serilog destructuring policies |
| SEC-03: Token storage | ✅ Compliant | httpOnly Cookie only |
| SEC-04: Cookie flags | ✅ Compliant | HttpOnly + Secure + SameSite=Strict |
| SEC-05: External token encryption | ✅ Compliant | AES-256-GCM cho LINE tokens |
| SEC-06: Rate limiting | ✅ Compliant | Per-IP + Global, 5 endpoint groups |
| SEC-07: CORS whitelist | ✅ Compliant | PWA + Admin domains only |
| SEC-08: JWT required | ✅ Compliant | Tất cả protected endpoints |
| SEC-09: S3 presigned URL | ✅ Compliant | 5 phút expiry, PUT only |
| SEC-10: Input validation | ✅ Compliant | FluentValidation |
| SEC-11: APPI compliance | ✅ Compliant | Retention + Anonymization policies |
| SEC-12: Audit logging | ✅ Compliant | AdminActionLog table |
| SEC-13: TLS 1.3 | ✅ Compliant | All in-transit |
| SEC-14: Encryption at-rest | ✅ Compliant | RDS + S3 SSE |
| SEC-15: Brute-force protection | ✅ Compliant | 5 attempts → CAPTCHA, 10 → Lock 24h |
