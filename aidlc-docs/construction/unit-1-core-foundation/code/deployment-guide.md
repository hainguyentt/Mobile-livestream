# Deployment Guide — Unit 1 Core Foundation

---

## Local Development (Docker Compose)

### Prerequisites
- Docker Desktop
- .NET 8 SDK
- Node.js 20+

### Khởi động toàn bộ stack

```bash
# 1. Copy env file
cp .env.example .env
# Điền các giá trị thực vào .env

# 2. Khởi động infrastructure (PostgreSQL, Redis, LocalStack, MockServices)
docker compose up -d postgres redis localstack mockservices

# 3. Chờ LocalStack healthy, sau đó init S3 + SES
docker compose exec localstack bash /etc/localstack/init/ready.d/create-s3-bucket.sh
docker compose exec localstack bash /etc/localstack/init/ready.d/verify-ses-email.sh

# 4. Chạy EF Core migrations
dotnet ef database update --project app/backend/LivestreamApp.API

# 5. Chạy API
dotnet run --project app/backend/LivestreamApp.API

# 6. Chạy PWA frontend
cd app/frontend/pwa && npm install && npm run dev

# 7. Chạy Admin frontend
cd app/frontend/admin && npm install && npm run dev
```

### Service URLs (local)
| Service | URL |
|---|---|
| API | http://localhost:5000 |
| API Swagger | http://localhost:5000/swagger |
| PWA | http://localhost:3000 |
| Admin | http://localhost:3001 |
| MockServices | http://localhost:5200 |
| MockServices Swagger | http://localhost:5200/swagger |
| PostgreSQL | localhost:5432 |
| Redis | localhost:6379 |
| LocalStack | http://localhost:4566 |

---

## Docker Compose (Full Stack)

```bash
# Build và chạy toàn bộ stack
docker compose up --build

# Chỉ rebuild API
docker compose up --build api

# Xem logs
docker compose logs -f api
```

---

## Environment Variables Reference

Xem `.env.example` để biết tất cả biến môi trường cần thiết.

### Biến bắt buộc cho production
- `JWT_SECRET` — ít nhất 32 ký tự, random
- `DB_PASSWORD` — mật khẩu PostgreSQL
- `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY` — AWS credentials
- `LINE_CHANNEL_SECRET` — LINE OAuth secret
- `STRIPE_SECRET_KEY` — Stripe secret key

---

## Health Check Endpoints

| Endpoint | Mô tả |
|---|---|
| `GET /health` | Overall health |
| `GET /health/ready` | Readiness (DB + Redis connected) |
| `GET /health/live` | Liveness |

---

## EF Core Migrations

```bash
# Tạo migration mới
dotnet ef migrations add {MigrationName} --project app/backend/LivestreamApp.API

# Apply migrations
dotnet ef database update --project app/backend/LivestreamApp.API

# Xem SQL script
dotnet ef migrations script --project app/backend/LivestreamApp.API

# Rollback migration
dotnet ef database update {PreviousMigrationName} --project app/backend/LivestreamApp.API
```

---

## Production Deployment (AWS ECS)

> Chi tiết deployment lên AWS ECS sẽ được bổ sung ở Operations phase.

Tóm tắt các bước:
1. Build Docker images và push lên ECR
2. Update ECS task definitions với image mới
3. Run EF migrations trước khi deploy (ECS task hoặc migration job)
4. Deploy ECS services (rolling update)
5. Verify health checks pass
6. Monitor CloudWatch logs
