# Infrastructure Design Plan — Unit 1: Core Foundation

**Unit**: Unit 1 — Core Foundation  
**Stage**: Infrastructure Design  
**Ngày tạo**: 2026-03-21  
**Trạng thái**: Chờ trả lời câu hỏi

---

## Đã xác định (không cần hỏi)

Từ NFR design + requirements.md:
- Cloud: AWS (ap-northeast-1 — Tokyo)
- Compute: ECS Fargate (containerized)
- DB: PostgreSQL RDS Multi-AZ
- Cache: Redis ElastiCache
- Storage: S3 + CloudFront CDN
- Email: AWS SES
- Dev: Docker Compose + LocalStack
- Logging: Serilog → CloudWatch

---

## Checklist Thực Thi

- [x] Step 1: Phân tích design artifacts
- [x] Step 2: Tạo plan với questions
- [x] Step 3: Thu thập câu trả lời
- [x] Step 4: Generate `infrastructure-design.md`
- [x] Step 5: Generate `deployment-architecture.md`
- [x] Step 6: Generate `shared-infrastructure.md`
- [x] Step 7: Present completion message
- [x] Step 8: User approved — proceed to Code Generation

---

## Câu Hỏi Làm Rõ

### Q1: ECS Fargate — Task Size cho MVP

Unit 1 API service chạy trên ECS Fargate. Task size phù hợp cho MVP (10K users)?

A. 0.25 vCPU / 512MB RAM — minimal, rẻ nhất (~$9/tháng per task)  
B. 0.5 vCPU / 1GB RAM — balance cho MVP (~$18/tháng per task)  
C. 1 vCPU / 2GB RAM — comfortable với .NET 8 runtime (~$36/tháng per task)  
D. 1 vCPU / 4GB RAM — headroom cho Hangfire + EF Core migrations (~$72/tháng per task)  

[Answer]: C

---

### Q2: RDS Instance Size cho MVP

PostgreSQL RDS instance size:

A. `db.t3.micro` (2 vCPU, 1GB RAM) — Free Tier eligible, dev/staging only  
B. `db.t3.small` (2 vCPU, 2GB RAM) — MVP production (~$25/tháng)  
C. `db.t3.medium` (2 vCPU, 4GB RAM) — comfortable cho production (~$50/tháng)  
D. `db.t4g.medium` (2 vCPU, 4GB RAM, ARM) — cost-optimized (~$40/tháng)  

[Answer]: B

---

### Q3: Networking — VPC Layout

Cấu trúc VPC cho production:

A. Simple: 1 public subnet (ALB) + 1 private subnet (ECS + RDS + Redis) — đủ cho MVP  
B. Standard: 1 public subnet (ALB) + 2 private subnets (app tier + data tier) — separation of concerns  
C. Full: Public + App + Data subnets, mỗi loại 2 AZ (high availability) — production-grade  

[Answer]: A

