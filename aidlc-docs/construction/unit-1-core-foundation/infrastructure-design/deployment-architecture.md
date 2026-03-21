# Deployment Architecture — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Ngày tạo**: 2026-03-21

---

## 1. Environment Overview

| Environment | Mục đích | Infrastructure |
|---|---|---|
| **Local Dev** | Developer machine | Docker Compose + LocalStack |
| **Staging** | Integration testing | AWS (scaled-down, shared) |
| **Production** | Live system | AWS (ap-northeast-1, full config) |

---

## 2. Local Development Architecture

```
Developer Machine
│
└── docker-compose up
        │
        ├── api (port 5000)
        │   └── ASP.NET Core 8
        │       ├── EF Core → postgres:5432
        │       ├── StackExchange.Redis → redis:6379
        │       ├── AWSSDK.S3 → localstack:4566
        │       ├── AWSSDK.SES → localstack:4566
        │       └── HttpClient → mockservices:5001
        │
        ├── mockservices (port 5001)
        │   └── Stripe Mock + LINE Pay Mock
        │
        ├── postgres (port 5432)
        │   └── PostgreSQL 16
        │       └── Volume: postgres_data
        │
        ├── redis (port 6379)
        │   └── Redis 7 (no auth in dev)
        │
        └── localstack (port 4566)
            └── Services: s3, ses, sns, sqs
                └── Init scripts:
                    ├── create-s3-bucket.sh
                    └── verify-ses-email.sh

Log files: ./logs/app-{date}.log (mounted volume)
```

**Startup sequence**:
```bash
# 1 lệnh khởi động toàn bộ stack
docker-compose up

# EF Core migrations chạy tự động khi API start
# LocalStack init scripts chạy tự động
# Hangfire dashboard: http://localhost:5000/hangfire
# API: http://localhost:5000
# MockServices: http://localhost:5001
```

---

## 3. Production Architecture (AWS ap-northeast-1)

```
Internet
    │
    │ HTTPS (TLS 1.3)
    ▼
┌─────────────────────────────────────────────────────┐
│              AWS CloudFront (CDN)                   │
│  - Static assets (PWA build)                        │
│  - S3 profile photos (/profiles/*)                  │
│  - Cache: 1 day for images                          │
└──────────────┬──────────────────────────────────────┘
               │ API requests (/api/*)
               ▼
┌─────────────────────────────────────────────────────┐
│         Application Load Balancer (ALB)             │
│  - HTTPS:443 listener                               │
│  - HTTP:80 → redirect HTTPS                         │
│  - SSL: ACM certificate (*.livestream-app.jp)       │
│  - Health check: GET /health/ready                  │
│  Public Subnets: 10.0.1.0/24, 10.0.2.0/24          │
└──────────────┬──────────────────────────────────────┘
               │ HTTP:8080 (sg-alb → sg-ecs)
               ▼
┌─────────────────────────────────────────────────────┐
│              ECS Fargate Cluster                    │
│         Private Subnet: 10.0.10.0/24               │
│                                                     │
│  ┌─────────────────┐  ┌─────────────────┐          │
│  │  API Task #1    │  │  API Task #2    │          │
│  │  1vCPU / 2GB    │  │  1vCPU / 2GB    │          │
│  │  port 8080      │  │  port 8080      │          │
│  └────────┬────────┘  └────────┬────────┘          │
│           │                    │                    │
│           └──────────┬─────────┘                   │
│                      │                             │
│  Auto Scaling: 2-10 tasks (CPU > 70% → scale out)  │
└──────────────────────┬──────────────────────────────┘
                       │
        ┌──────────────┼──────────────────┐
        │              │                  │
        ▼              ▼                  ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────────┐
│  RDS         │ │ ElastiCache  │ │  AWS Services    │
│  PostgreSQL  │ │  Redis 7     │ │                  │
│  db.t3.small │ │ cache.t3.micro│ │  S3 (profiles)  │
│  Multi-AZ    │ │  Single node │ │  SES (email)     │
│  Primary:    │ │  TLS + Auth  │ │  Secrets Manager │
│  10.0.10.x   │ │  10.0.10.x   │ │  CloudWatch      │
│  Standby:    │ └──────────────┘ │  ECR             │
│  10.0.10.x   │                  └──────────────────┘
└──────────────┘
```

---

## 4. Deployment Flow (Rolling Update)

```
Developer pushes to main branch
        │
        ▼
GitHub Actions / CodePipeline
        │
        ├── Step 1: Build & Test
        │   dotnet build + dotnet test
        │   → Fail → Stop, notify team
        │
        ├── Step 2: Docker Build
        │   docker build -t livestream-api:{git-sha} .
        │
        ├── Step 3: Push to ECR
        │   docker push {ecr}/livestream-api:{git-sha}
        │   docker push {ecr}/livestream-api:latest
        │
        └── Step 4: ECS Rolling Update
            aws ecs update-service \
              --cluster livestream-cluster \
              --service livestream-api-service \
              --force-new-deployment

ECS Rolling Update Process:
        │
        ├── Start new Task #3 (new image)
        │   └── Startup probe: /health/startup (wait for migrations)
        │   └── Readiness probe: /health/ready (DB + Redis OK)
        │
        ├── [Task #3 healthy] → ALB adds Task #3 to target group
        │
        ├── Drain Task #1 (60s connection draining)
        │   └── ALB stops sending new requests to Task #1
        │   └── Existing connections complete
        │
        ├── Stop Task #1
        │
        ├── Start new Task #4 (new image)
        │   └── Same health check process
        │
        └── Stop Task #2
            → Deployment complete, 0 downtime
```

---

## 5. EF Core Migration Strategy (Production)

```
ECS Task Startup:
        │
        ├── Program.cs: await db.Database.MigrateAsync()
        │       │
        │       ├── Connect to RDS (migration_user credentials)
        │       ├── Check __EFMigrationsHistory table
        │       ├── Apply pending migrations (if any)
        │       └── Log: "Migrations applied: {count}"
        │
        ├── /health/startup → 200 OK (migrations done)
        │
        └── App starts accepting traffic

Migration Safety:
- Migrations phải backward compatible (không DROP column ngay)
- Pattern: Add column nullable → Deploy → Backfill → Make NOT NULL → Deploy
- Rollback: ECS redeploy previous image tag (migrations không auto-rollback)
```

---

## 6. Staging Environment

```
Staging = Production-lite (same AWS account, different resources)

Differences from Production:
  ECS: 1 task (không HA)
  RDS: db.t3.micro (single-AZ)
  ElastiCache: cache.t3.micro (same)
  MockServices: Deployed as ECS task (không dùng LocalStack)
  Domain: staging.livestream-app.jp

Cost: ~$50/tháng (staging)
```

---

## 7. Infrastructure as Code (IaC)

**Tool**: AWS CDK (TypeScript) hoặc Terraform — quyết định ở Code Generation stage.

**Scope Unit 1**:
```
infrastructure/
├── vpc.ts          # VPC, subnets, security groups
├── ecs.ts          # Cluster, task definition, service, auto-scaling
├── rds.ts          # RDS instance, parameter group, subnet group
├── elasticache.ts  # Redis cluster, subnet group
├── s3.ts           # Bucket, bucket policy, CloudFront OAC
├── cloudfront.ts   # Distribution, behaviors
├── alb.ts          # Load balancer, listeners, target group
├── secrets.ts      # Secrets Manager entries
├── iam.ts          # Task role, execution role, policies
└── monitoring.ts   # CloudWatch alarms, log groups
```

> **Note**: IaC files sẽ được generate trong Code Generation stage. Infrastructure design này là blueprint.
