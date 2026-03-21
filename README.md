# Livestream Hẹn Hò — Thị Trường Nhật Bản

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Next.js](https://img.shields.io/badge/Next.js-14+-black.svg)](https://nextjs.org/)
[![AWS](https://img.shields.io/badge/AWS-Cloud-orange.svg)](https://aws.amazon.com/)

Progressive Web App (PWA) kết hợp livestream và hẹn hò dành cho thị trường Nhật Bản. Ứng dụng nhắm đến đối tượng nam giới trưởng thành (18-70 tuổi), mô hình kinh doanh Pay-per-use thông qua hệ thống coin ảo.

---

## 📋 Tổng Quan

**Tính năng chính**:
- 🎥 Livestream public (1-N, tối đa 50 viewers) và private call 1-1
- 💬 Chat real-time (room chat + direct chat 1-1)
- 🎁 Virtual gifts với animation
- 💰 Hệ thống coin (Stripe + LINE Pay)
- 🏆 Leaderboard & ranking system
- 🔔 Push notifications (FCM)
- 🛡️ AI content moderation (AWS Rekognition)
- 👥 Matching algorithm & social features

**Tech Stack**:
- **Frontend**: Next.js 14+ (PWA), Tailwind CSS, SignalR client, Agora.io Web SDK
- **Backend**: .NET 8 (ASP.NET Core), SignalR, Entity Framework Core 8
- **Database**: PostgreSQL (AWS RDS), Redis (AWS ElastiCache)
- **Infrastructure**: AWS (ECS Fargate, S3, CloudFront, SES, SNS)
- **Video**: Agora.io
- **Payment**: Stripe + LINE Pay

---

## 🏗️ Kiến Trúc

**Modular Monolith** — một ASP.NET Core solution với 12 modules độc lập:

```
LivestreamApp.sln
├── src/
│   ├── LivestreamApp.API/              # Entry point (REST + SignalR)
│   ├── LivestreamApp.Auth/             # Authentication & Identity
│   ├── LivestreamApp.Profiles/         # User Profiles & Matching
│   ├── LivestreamApp.Livestream/       # Livestream (Public + Private)
│   ├── LivestreamApp.RoomChat/         # Room chat (Redis Streams)
│   ├── LivestreamApp.DirectChat/       # Direct chat 1-1 (PostgreSQL)
│   ├── LivestreamApp.Payment/          # Coin & Payment (Stripe + LINE Pay)
│   ├── LivestreamApp.Notification/     # Push Notifications (FCM)
│   ├── LivestreamApp.Leaderboard/      # Ranking & Leaderboard
│   ├── LivestreamApp.Moderation/       # Content Moderation (AI)
│   ├── LivestreamApp.Admin/            # Admin Dashboard API
│   └── LivestreamApp.Shared/           # Shared kernel
├── mock/
│   └── LivestreamApp.MockServices/     # Stripe + LINE Pay mock servers
├── frontend/
│   ├── pwa/                            # Next.js PWA (Viewer + Host)
│   └── admin/                          # Next.js Admin Dashboard
└── infra/
    └── docker-compose.yml              # Local development stack
```

Chi tiết kiến trúc: [`aidlc-docs/inception/application-design/`](aidlc-docs/inception/application-design/)

---

## 🚀 Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### Local Development

```bash
# 1. Clone repository
git clone https://github.com/your-org/mobile-livestream.git
cd mobile-livestream

# 2. Start infrastructure (PostgreSQL, Redis, LocalStack, MockServices)
docker-compose up -d

# 3. Run backend API
cd src/LivestreamApp.API
dotnet restore
dotnet run

# 4. Run PWA (new terminal)
cd frontend/pwa
npm install
npm run dev

# 5. Run Admin Dashboard (new terminal)
cd frontend/admin
npm install
npm run dev
```

**Access**:
- API: http://localhost:5000
- PWA: http://localhost:3000
- Admin: http://localhost:3001
- Hangfire Dashboard: http://localhost:5000/hangfire
- MockServices: http://localhost:5001

---

## 📚 Tài Liệu

### Inception Phase
- [Requirements](aidlc-docs/inception/requirements/requirements.md) — Yêu cầu chức năng & phi chức năng
- [User Stories](aidlc-docs/inception/user-stories/stories.md) — 37 stories, 9 epics
- [Application Design](aidlc-docs/inception/application-design/application-design.md) — Kiến trúc tổng quan
- [Units of Work](aidlc-docs/inception/application-design/unit-of-work.md) — 5 units phát triển

### Construction Phase
- [Functional Design](aidlc-docs/construction/unit-1-core-foundation/functional-design/) — Domain entities, business rules
- [NFR Requirements](aidlc-docs/construction/unit-1-core-foundation/nfr-requirements/) — Tech stack decisions
- [NFR Design](aidlc-docs/construction/unit-1-core-foundation/nfr-design/) — Design patterns, logical components
- [Infrastructure Design](aidlc-docs/construction/unit-1-core-foundation/infrastructure-design/) — AWS services, deployment

### Cross-Cutting Concerns
- [Shared Infrastructure](aidlc-docs/construction/cross-cutting/shared-infrastructure.md) — Shared AWS resources, schema ownership
- [Technical Risk Mitigation](aidlc-docs/construction/cross-cutting/technical-risk-mitigation.md) — 9 production risks + mitigation strategies

---

## 🧪 Testing

```bash
# Backend unit tests
cd src/LivestreamApp.API
dotnet test

# Frontend tests
cd frontend/pwa
npm run test

# Integration tests
cd tests/Integration
dotnet test
```

---

## 🏗️ Development Roadmap

### ✅ Unit 1: Core Foundation (In Progress)
- Authentication (Email + LINE Login)
- User Profiles
- Infrastructure setup (Docker Compose, LocalStack)
- Mock Services (Stripe + LINE Pay)

### 🔜 Unit 2: Livestream Engine
- Public livestream (1-N)
- Private call 1-1 (Agora.io)
- Real-time chat (SignalR)
- Per-minute billing

### 🔜 Unit 3: Coin & Payment
- Coin system
- Stripe integration
- LINE Pay integration
- Virtual gifts

### 🔜 Unit 4: Social & Discovery
- Matching algorithm
- Leaderboard & ranking
- Push notifications (FCM)
- Follow/Like features

### 🔜 Unit 5: Admin & Moderation
- Admin Dashboard
- AI content moderation (AWS Rekognition)
- Report handling
- Analytics

---

## 🌐 Deployment

### Production (AWS)

```bash
# Build Docker image
docker build -t livestream-api:latest -f src/LivestreamApp.API/Dockerfile .

# Push to ECR
aws ecr get-login-password --region ap-northeast-1 | docker login --username AWS --password-stdin {account}.dkr.ecr.ap-northeast-1.amazonaws.com
docker tag livestream-api:latest {account}.dkr.ecr.ap-northeast-1.amazonaws.com/livestream-api:latest
docker push {account}.dkr.ecr.ap-northeast-1.amazonaws.com/livestream-api:latest

# Deploy to ECS
aws ecs update-service --cluster livestream-cluster --service livestream-api-service --force-new-deployment
```

Chi tiết deployment: [`aidlc-docs/construction/unit-1-core-foundation/infrastructure-design/deployment-architecture.md`](aidlc-docs/construction/unit-1-core-foundation/infrastructure-design/deployment-architecture.md)

---

## 🔒 Security & Compliance

- ✅ APPI (Act on Protection of Personal Information) compliant
- ✅ TLS 1.3 encryption (at-rest + in-transit)
- ✅ JWT authentication với refresh token rotation
- ✅ Rate limiting trên tất cả endpoints
- ✅ PII access audit logging
- ✅ Incident response playbook

Chi tiết: [`aidlc-docs/construction/cross-cutting/technical-risk-mitigation.md`](aidlc-docs/construction/cross-cutting/technical-risk-mitigation.md) (Risk #9)

---

## 📊 Monitoring

- **Logs**: Serilog → CloudWatch Logs (production), File+Console (dev/test)
- **Metrics**: CloudWatch Metrics (ECS, RDS, Redis, ALB)
- **Alerts**: CloudWatch Alarms → SNS → Email
- **APM**: Hangfire Dashboard (background jobs)

---

## 🤝 Contributing

1. Fork repository
2. Create feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open Pull Request

**Coding Standards**:
- Backend: Follow [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Frontend: ESLint + Prettier configured
- Commit messages: [Conventional Commits](https://www.conventionalcommits.org/)

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👥 Team

- **Product Owner**: [Name]
- **Tech Lead**: [Name]
- **Backend Developers**: [Names]
- **Frontend Developers**: [Names]
- **DevOps Engineer**: [Name]

---

## 📞 Support

- **Documentation**: [aidlc-docs/](aidlc-docs/)
- **Issues**: [GitHub Issues](https://github.com/your-org/mobile-livestream/issues)
- **Email**: support@livestream-app.jp

---

## 🎯 Success Metrics (MVP)

| Metric | Target |
|---|---|
| Người dùng đăng ký (3 tháng đầu) | 5,000+ |
| DAU/MAU ratio | > 20% |
| Thời gian trung bình mỗi phiên | > 15 phút |
| Tỷ lệ chuyển đổi (free → paying) | > 10% |
| Uptime | > 99.9% |

---

**Built with ❤️ for the Japanese market**
