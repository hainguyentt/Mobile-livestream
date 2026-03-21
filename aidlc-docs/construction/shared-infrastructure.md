# Shared Infrastructure — Toàn Dự Án

**Ngày tạo**: 2026-03-21  
**Scope**: Infrastructure dùng chung cho tất cả 5 units

---

## 1. Shared AWS Resources

Các resources sau được tạo 1 lần (Unit 1) và dùng chung cho tất cả units:

| Resource | Tạo ở Unit | Dùng bởi |
|---|---|---|
| VPC + Subnets + Security Groups | Unit 1 | Tất cả units |
| ECS Cluster (`livestream-cluster`) | Unit 1 | Tất cả units |
| RDS PostgreSQL | Unit 1 | Unit 1, 2, 3, 4, 5 |
| ElastiCache Redis | Unit 1 | Unit 1, 2, 3, 4 |
| S3 Bucket (profiles) | Unit 1 | Unit 1, 2 |
| CloudFront Distribution | Unit 1 | Tất cả units |
| ALB | Unit 1 | Tất cả units |
| ECR Repositories | Unit 1 | Tất cả units |
| Secrets Manager | Unit 1 | Tất cả units |
| CloudWatch Log Groups | Unit 1 | Tất cả units |
| SES (email) | Unit 1 | Unit 1, 4 |
| SNS (push notifications) | Unit 4 | Unit 4 |
| FCM (Firebase) | Unit 4 | Unit 4 |
| LocalStack (dev) | Unit 1 | Tất cả units |

---

## 2. Database Schema Ownership

Mỗi module sở hữu schema tables của mình, nhưng dùng chung 1 PostgreSQL instance:

| Unit | Module | Tables |
|---|---|---|
| Unit 1 | Auth | `users`, `refresh_tokens`, `otp_codes`, `external_logins`, `login_attempts` |
| Unit 1 | Profiles | `user_profiles`, `host_profiles`, `user_photos` |
| Unit 1 | Admin (skeleton) | `admin_action_logs` |
| Unit 1 | Hangfire | `hangfire_*` |
| Unit 2 | Livestream | `rooms`, `viewer_sessions`, `private_call_requests`, `call_sessions`, `billing_sessions` |
| Unit 2 | DirectChat | `conversations`, `direct_messages` (partitioned) |
| Unit 3 | Payment | `coin_balances`, `coin_transactions`, `coin_packages`, `virtual_gifts`, `payment_intents`, `withdrawal_requests` |
| Unit 4 | Social | `follows`, `likes`, `notification_settings`, `device_tokens`, `notification_history` |
| Unit 4 | Leaderboard | `leaderboard_snapshots`, `rank_badges` |
| Unit 5 | Moderation | `reports`, `moderation_actions`, `moderation_audit_log` |
| Unit 5 | Admin (full) | `admin_action_logs` (extended) |

---

## 3. Redis Namespace Ownership

| Namespace | Owner Unit | Pattern |
|---|---|---|
| `revoked_token:*` | Unit 1 | Auth blacklist |
| `user:profile:*` | Unit 1 | Profile cache |
| `otp:rate_limit:*` | Unit 1 | OTP rate limiting |
| `login:rate_limit:*` | Unit 1 | Login rate limiting |
| `room:{roomId}:chat` | Unit 2 | RoomChat Redis Streams |
| `signalr:*` | Unit 2 | SignalR backplane |
| `leaderboard:*` | Unit 4 | Sorted sets |

---

## 4. Docker Compose — Full Stack (tất cả units)

```yaml
# docker-compose.yml (evolves across units)
version: '3.9'

services:
  api:
    build: ./src/LivestreamApp.API
    ports: ["5000:8080"]
    depends_on: [postgres, redis, localstack, mockservices]
    volumes: ["./logs:/app/logs"]
    environment:
      ASPNETCORE_ENVIRONMENT: Development

  mockservices:
    build: ./src/LivestreamApp.MockServices
    ports: ["5001:5001"]

  pwa:
    build: ./pwa
    ports: ["3000:3000"]
    environment:
      NEXT_PUBLIC_API_URL: http://localhost:5000

  admin:
    build: ./admin
    ports: ["3001:3001"]
    environment:
      NEXT_PUBLIC_API_URL: http://localhost:5000

  postgres:
    image: postgres:16-alpine
    ports: ["5432:5432"]
    environment:
      POSTGRES_DB: livestream
      POSTGRES_USER: app_user
      POSTGRES_PASSWORD: dev_password
    volumes: ["postgres_data:/var/lib/postgresql/data"]

  redis:
    image: redis:7-alpine
    ports: ["6379:6379"]

  localstack:
    image: localstack/localstack:3
    ports: ["4566:4566"]
    environment:
      SERVICES: s3,ses,sns,sqs,rekognition
    volumes:
      - "./localstack-init:/etc/localstack/init/ready.d"
      - "localstack_data:/var/lib/localstack"

volumes:
  postgres_data:
  localstack_data:
```

---

## 5. Environment Variables Convention

```
# Database
ConnectionStrings__Default=Host=...;Database=livestream;...

# Redis
Redis__ConnectionString=localhost:6379

# JWT
Jwt__Secret={256-bit key}
Jwt__Issuer=livestream-app
Jwt__Audience=livestream-app-users
Jwt__AccessTokenExpiryMinutes=15
Jwt__RefreshTokenExpiryDays=30

# AWS
AWS__Region=ap-northeast-1
AWS__ServiceURL=http://localhost:4566  # LocalStack (dev only)
AWS__S3__BucketName=livestream-app-profiles
AWS__SES__SenderEmail=noreply@livestream-app.jp

# LINE Login
LineLogin__ClientId={line-client-id}
LineLogin__ClientSecret={from Secrets Manager}
LineLogin__CallbackUrl=https://livestream-app.jp/login/line/callback

# Cache TTL
Cache__UserProfileTtlMinutes=15

# Rate Limiting
RateLimit__AuthPerIpPerMinute=20
RateLimit__AuthGlobalPerMinute=1000

# MockServices
ExternalServices__Stripe__BaseUrl=http://localhost:5001/stripe
ExternalServices__Stripe__MockMode=true
ExternalServices__LinePay__BaseUrl=http://localhost:5001/linepay
ExternalServices__LinePay__MockMode=true
```

---

## 6. Cross-Cutting Technical Risk Patterns

Các patterns sau áp dụng cho **tất cả units** — implement ngay từ Unit 1, không refactor sau:

### 6.1 Dual DbContext (Read/Write Separation)
- Tất cả services inject `ReadOnlyDbContext` cho GET operations, `AppDbContext` cho writes
- MVP: cùng connection string → zero overhead
- Scale: đổi `ConnectionStrings:ReadOnly` → bật Read Replica, không cần refactor code

### 6.2 Hybrid EF Core + Dapper
- EF Core: CRUD đơn giản, domain operations
- Dapper: complex queries, analytics, reporting
- Convention: query > 3 JOINs hoặc có aggregation → dùng Dapper

### 6.3 Write Batching cho High-Frequency Operations
- Billing ticks (Unit 2): buffer Redis → flush DB mỗi 30s
- Leaderboard score updates (Unit 4): Redis Sorted Sets → snapshot DB async
- Không write DB synchronously trong hot paths (SignalR message handlers)

### 6.4 Optimistic Concurrency cho Shared Resources
- `coin_balances`: `xmin` column (PostgreSQL native)
- `user_profiles`: `UpdatedAt` timestamp check
- Tránh pessimistic locking (SELECT FOR UPDATE) trong high-concurrency paths
