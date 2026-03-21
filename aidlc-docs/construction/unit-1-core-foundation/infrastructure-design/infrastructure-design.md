# Infrastructure Design — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Cloud Provider**: AWS (ap-northeast-1 — Tokyo)  
**Ngày tạo**: 2026-03-21

---

## 1. Infrastructure Services Map

| Logical Component | AWS Service | Tier | Config |
|---|---|---|---|
| API Application | ECS Fargate | Compute | 1 vCPU / 2GB RAM, min 2 tasks |
| MockServices | ECS Fargate (dev only) | Compute | 0.25 vCPU / 512MB (dev/staging) |
| PostgreSQL | RDS PostgreSQL 16 | Data | db.t3.small, Multi-AZ |
| Redis | ElastiCache Redis 7 | Cache | cache.t3.micro, single node (MVP) |
| Object Storage | S3 | Storage | Standard class, SSE-S3 |
| CDN | CloudFront | CDN | Origin: S3 + ALB |
| Email | SES | Messaging | ap-northeast-1, verified domain |
| Load Balancer | ALB | Networking | HTTPS listener, health check |
| Container Registry | ECR | Registry | Private repo per service |
| Secrets | Secrets Manager | Security | DB credentials, JWT secret, API keys |
| Logs | CloudWatch Logs | Observability | Log group `/livestream-app/api` |
| Metrics | CloudWatch Metrics | Observability | ECS + RDS + Redis metrics |
| Background Jobs | Hangfire (in-process) | Compute | Chạy trong ECS task, PostgreSQL storage |

---

## 2. Compute — ECS Fargate

### 2.1 API Service Task Definition

```json
{
  "family": "livestream-api",
  "cpu": "1024",
  "memory": "2048",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "containerDefinitions": [
    {
      "name": "api",
      "image": "{account}.dkr.ecr.ap-northeast-1.amazonaws.com/livestream-api:latest",
      "portMappings": [{ "containerPort": 8080, "protocol": "tcp" }],
      "environment": [
        { "name": "ASPNETCORE_ENVIRONMENT", "value": "Production" },
        { "name": "ASPNETCORE_URLS", "value": "http://+:8080" }
      ],
      "secrets": [
        { "name": "ConnectionStrings__Default",
          "valueFrom": "arn:aws:secretsmanager:...:livestream/db-connection" },
        { "name": "Redis__ConnectionString",
          "valueFrom": "arn:aws:secretsmanager:...:livestream/redis-connection" },
        { "name": "Jwt__Secret",
          "valueFrom": "arn:aws:secretsmanager:...:livestream/jwt-secret" },
        { "name": "LineLogin__ClientSecret",
          "valueFrom": "arn:aws:secretsmanager:...:livestream/line-client-secret" }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/livestream-app/api",
          "awslogs-region": "ap-northeast-1",
          "awslogs-stream-prefix": "ecs"
        }
      },
      "healthCheck": {
        "command": ["CMD-SHELL", "curl -f http://localhost:8080/health/live || exit 1"],
        "interval": 30,
        "timeout": 5,
        "retries": 3,
        "startPeriod": 60
      }
    }
  ]
}
```

### 2.2 ECS Service Config

```
Service: livestream-api-service
  Cluster: livestream-cluster
  Launch type: FARGATE
  Desired count: 2 (minimum for HA)
  Min healthy percent: 100  (rolling deploy — no downtime)
  Max percent: 200

Auto Scaling:
  Min: 2 tasks
  Max: 10 tasks
  Scale out: CPU > 70% for 2 minutes → +2 tasks
  Scale in:  CPU < 30% for 5 minutes → -1 task
```

### 2.3 Cost Estimate (MVP — 2 tasks running 24/7)

| Resource | Config | Cost/month |
|---|---|---|
| ECS Fargate (2 tasks) | 1vCPU × 2GB × 2 tasks | ~$72 |
| RDS db.t3.small Multi-AZ | 2 vCPU, 2GB | ~$50 |
| ElastiCache cache.t3.micro | 0.5 vCPU, 0.5GB | ~$13 |
| ALB | 1 ALB + LCU | ~$20 |
| S3 + CloudFront | 10GB storage + transfer | ~$5 |
| SES | 10K emails/month | ~$1 |
| CloudWatch | Logs + metrics | ~$5 |
| ECR | 2 repos | ~$1 |
| Secrets Manager | 5 secrets | ~$2 |
| **Total MVP** | | **~$169/tháng** |

---

## 3. Database — RDS PostgreSQL

### 3.1 Instance Config

```
Engine: PostgreSQL 16
Instance: db.t3.small (2 vCPU, 2GB RAM)
Storage: 20GB gp3 SSD (auto-scale to 100GB)
Multi-AZ: Yes (standby in ap-northeast-1c)
Backup: Automated daily, retention 30 days
PITR: Enabled
Encryption: AES-256 at-rest
Parameter group: Custom
  - max_connections: 100
  - shared_buffers: 512MB
  - log_min_duration_statement: 1000ms (log slow queries)
```

### 3.2 Connection String Pattern

```
Host=livestream-db.xxxxx.ap-northeast-1.rds.amazonaws.com;
Port=5432;
Database=livestream;
Username=app_user;
Password={from Secrets Manager};
SSL Mode=Require;
Trust Server Certificate=false;
Maximum Pool Size=50;
Connection Idle Lifetime=300;
```

### 3.3 Database Users

| User | Permissions | Mục đích |
|---|---|---|
| `app_user` | SELECT, INSERT, UPDATE, DELETE | Application runtime |
| `migration_user` | ALL (schema changes) | EF Core migrations only |
| `readonly_user` | SELECT only | Analytics, debugging |

> **Security**: `app_user` không có DROP/ALTER quyền. Migrations chạy với `migration_user` credentials riêng.

---

## 4. Cache — ElastiCache Redis

### 4.1 Instance Config

```
Engine: Redis 7.x
Node type: cache.t3.micro (0.5 vCPU, 0.5GB RAM)
Cluster mode: Disabled (single node, MVP)
Multi-AZ: No (MVP — upgrade khi cần)
Encryption in-transit: Yes (TLS)
Encryption at-rest: Yes
Auth token: Yes (from Secrets Manager)
Backup: Daily snapshot, retention 7 days
```

### 4.2 Memory Estimation (MVP)

| Key Pattern | Estimated Count | Size/key | Total |
|---|---|---|---|
| `revoked_token:*` | ~1,000 active | 50 bytes | ~50KB |
| `user:profile:*` | ~500 cached | 2KB | ~1MB |
| `otp:rate_limit:*` | ~100 active | 20 bytes | ~2KB |
| `login:rate_limit:*` | ~200 active | 20 bytes | ~4KB |
| **Total** | | | **~1.1MB** (well within 0.5GB) |

---

## 5. Storage — S3

### 5.1 Bucket Config

```
Bucket: livestream-app-profiles-{account-id}
Region: ap-northeast-1
Versioning: Disabled (photos replaced, not versioned)
Encryption: SSE-S3 (AES-256)
Public access: Block all public access
Lifecycle rules:
  - Incomplete multipart uploads: Delete after 1 day
CORS: Not needed (presigned URL upload, no browser direct access)
```

### 5.2 IAM Policy cho ECS Task Role

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": ["s3:PutObject", "s3:GetObject", "s3:DeleteObject", "s3:HeadObject"],
      "Resource": "arn:aws:s3:::livestream-app-profiles-*/*"
    },
    {
      "Effect": "Allow",
      "Action": ["s3:GeneratePresignedUrl"],
      "Resource": "arn:aws:s3:::livestream-app-profiles-*/*"
    }
  ]
}
```

### 5.3 CloudFront Distribution

```
Origin: S3 bucket (OAC — Origin Access Control)
Price class: PriceClass_200 (US, EU, Asia — bao gồm Nhật)
Cache behavior:
  - /profiles/*: Cache 1 ngày (ảnh không thay đổi sau upload)
  - Default TTL: 86400s
Viewer protocol: HTTPS only
```

---

## 6. Networking — VPC (Simple Layout)

### 6.1 VPC Structure

```
VPC: 10.0.0.0/16 (ap-northeast-1)
│
├── Public Subnet: 10.0.1.0/24 (ap-northeast-1a)
│   └── ALB (internet-facing)
│
├── Public Subnet: 10.0.2.0/24 (ap-northeast-1c)
│   └── ALB (second AZ for HA)
│
└── Private Subnet: 10.0.10.0/24 (ap-northeast-1a)
    ├── ECS Fargate Tasks
    ├── RDS Primary
    ├── RDS Standby (Multi-AZ, same private subnet range)
    └── ElastiCache Redis
```

### 6.2 Security Groups

```
sg-alb (ALB):
  Inbound:  443 (HTTPS) from 0.0.0.0/0
            80 (HTTP) from 0.0.0.0/0 → redirect to 443
  Outbound: 8080 to sg-ecs

sg-ecs (ECS Tasks):
  Inbound:  8080 from sg-alb only
  Outbound: 5432 to sg-rds
            6379 to sg-redis
            443 to 0.0.0.0/0 (SES, S3, Secrets Manager, ECR)

sg-rds (RDS):
  Inbound:  5432 from sg-ecs only
  Outbound: None

sg-redis (ElastiCache):
  Inbound:  6379 from sg-ecs only
  Outbound: None
```

### 6.3 ALB Config

```
Scheme: internet-facing
Listeners:
  - HTTP:80 → Redirect to HTTPS:443
  - HTTPS:443 → Forward to ECS target group

Target Group:
  Protocol: HTTP
  Port: 8080
  Health check: GET /health/ready
  Healthy threshold: 2
  Unhealthy threshold: 3
  Interval: 30s
  Timeout: 10s

SSL Certificate: ACM (*.livestream-app.jp)
```

---

## 7. Secrets Management

| Secret Name | Content | Rotation |
|---|---|---|
| `livestream/db-connection` | PostgreSQL connection string | Manual (quarterly) |
| `livestream/redis-connection` | Redis connection + auth token | Manual |
| `livestream/jwt-secret` | JWT signing key (256-bit) | Manual (yearly) |
| `livestream/line-client-secret` | LINE Login client secret | Manual |
| `livestream/ses-config` | SES region + sender email | Static |

**Access**: ECS Task Role có `secretsmanager:GetSecretValue` permission cho prefix `livestream/*`.

---

## 8. CI/CD Pipeline (Skeleton)

```
GitHub Actions / AWS CodePipeline:

1. Code push → main branch
2. Build: dotnet build + test
3. Docker build: docker build -t livestream-api .
4. Push to ECR: docker push {ecr-url}/livestream-api:latest
5. ECS Deploy: aws ecs update-service --force-new-deployment
   → ECS rolling update (min 100% healthy → new tasks start → old tasks stop)
6. Health check: ALB confirms /health/ready returns 200
```

**Dockerfile skeleton**:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/LivestreamApp.API/LivestreamApp.API.csproj", "src/LivestreamApp.API/"]
RUN dotnet restore "src/LivestreamApp.API/LivestreamApp.API.csproj"
COPY . .
RUN dotnet publish "src/LivestreamApp.API/LivestreamApp.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LivestreamApp.API.dll"]
```

---

## 9. Monitoring & Alerting

### 9.1 CloudWatch Alarms (Unit 1)

| Alarm | Metric | Threshold | Action |
|---|---|---|---|
| API High CPU | ECS CPUUtilization | > 80% for 5 min | SNS → Email |
| API High Memory | ECS MemoryUtilization | > 85% for 5 min | SNS → Email |
| RDS High CPU | RDS CPUUtilization | > 80% for 5 min | SNS → Email |
| RDS Low Storage | RDS FreeStorageSpace | < 5GB | SNS → Email |
| ALB 5xx Errors | ALB HTTPCode_Target_5XX | > 10/min | SNS → Email |
| ALB High Latency | ALB TargetResponseTime | > 1s p95 | SNS → Email |
| Redis High Memory | ElastiCache DatabaseMemoryUsagePercentage | > 80% | SNS → Email |

### 9.2 CloudWatch Log Insights Queries

```sql
-- Top errors in last 1 hour
fields @timestamp, @message
| filter level = "Error"
| sort @timestamp desc
| limit 50

-- Slow API requests (> 500ms)
fields @timestamp, requestPath, durationMs
| filter durationMs > 500
| sort durationMs desc
| limit 20

-- Failed login attempts by IP
fields @timestamp, clientIp, email
| filter message like "Failed login"
| stats count() as attempts by clientIp
| sort attempts desc
```

---

## 10. Technical Risk Mitigation

Chi tiết về 9 production risks và mitigation strategies được document riêng tại:

**📄 [`../../cross-cutting/technical-risk-mitigation.md`](../../cross-cutting/technical-risk-mitigation.md)**

**Tóm tắt 9 risks**:
1. DB Write Bottleneck (High) — Batch writes, optimistic concurrency
2. Read/Write Separation (Medium) — Dual DbContext pattern
3. EF Core Performance (Medium) — Hybrid EF+Dapper
4. SignalR Scalability (High) — Redis backplane, connection limits
5. Redis Memory Exhaustion (High) — LRU eviction, mandatory TTL
6. S3 Cost Explosion (Medium) — Hard limits, orphan cleanup
7. JWT Secret Rotation (Medium) — Multi-key validation
8. Agora Free Tier (High) — Usage tracking, channel limits
9. APPI Data Breach (Critical) — Audit logs, incident response playbook

Tất cả risks đều có MVP-level mitigation. Implementation effort: +2-3 ngày development time.

---

## 11. Shared Infrastructure

Chi tiết về shared AWS resources, database schema ownership, Redis namespaces, và cross-cutting patterns:

**📄 [`../../cross-cutting/shared-infrastructure.md`](../../cross-cutting/shared-infrastructure.md)**

**Tóm tắt**:
- Shared AWS resources: VPC, ECS Cluster, RDS, Redis, S3, ALB, CloudFront (tạo ở Unit 1, dùng chung cho tất cả units)
- Database schema ownership: Mỗi module sở hữu tables của mình trong cùng 1 PostgreSQL instance
- Redis namespace convention: `revoked_token:*`, `user:profile:*`, `room:{id}:chat`, `signalr:*`, `leaderboard:*`
- Cross-cutting patterns: Dual DbContext, Hybrid EF+Dapper, Write Batching, Optimistic Concurrency
