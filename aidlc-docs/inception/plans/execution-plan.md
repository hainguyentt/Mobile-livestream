# Execution Plan
# Dự Án: App Livestream Hẹn Hò - Thị Trường Nhật Bản

**Ngày tạo**: 2026-03-21  
**Trạng thái**: Draft - Chờ phê duyệt

---

## 1. Tóm Tắt Phân Tích

### 1.1 Change Impact Assessment

| Khía cạnh | Đánh giá | Mô tả |
|---|---|---|
| User-facing changes | ✅ Yes | Toàn bộ app là user-facing: PWA, livestream, chat, payment |
| Structural changes | ✅ Yes | Greenfield — thiết kế kiến trúc từ đầu (PWA + .NET backend + AWS) |
| Data model changes | ✅ Yes | Nhiều entities mới: User, Stream, Coin, Gift, Message, Leaderboard |
| API changes | ✅ Yes | REST API + SignalR Hubs hoàn toàn mới |
| NFR impact | ✅ Yes | Real-time video (Agora), SignalR, payment (Stripe/LINE Pay), APPI compliance, security |

### 1.2 Risk Assessment

| Tiêu chí | Mức độ |
|---|---|
| Risk Level | **High** |
| Rollback Complexity | Moderate (greenfield, không ảnh hưởng hệ thống cũ) |
| Testing Complexity | Complex (real-time video, payment, content moderation) |

**Lý do High Risk:**
- Tích hợp Agora.io real-time video (latency-sensitive)
- Payment processing (Stripe + LINE Pay) — lỗi ảnh hưởng trực tiếp doanh thu
- Content moderation AI + manual workflow
- Tuân thủ APPI (dữ liệu cá nhân người dùng Nhật Bản)
- SignalR scale-out với Redis backplane trên AWS ECS Fargate

---

## 2. Workflow Visualization

```
INCEPTION PHASE
+---------------------------+
| Workspace Detection  DONE |
| Requirements Analysis DONE|
| User Stories         DONE |
| Workflow Planning    NOW  |
| Application Design EXECUTE|
| Units Generation   EXECUTE|
+---------------------------+
            |
            v
CONSTRUCTION PHASE (per unit)
+---------------------------+
| Functional Design  EXECUTE|
| NFR Requirements   EXECUTE|
| NFR Design         EXECUTE|
| Infrastructure     EXECUTE|
| Code Generation    EXECUTE|
+---------------------------+
            |
            v
+---------------------------+
| Build and Test     EXECUTE|
+---------------------------+
            |
            v
        Complete
```

**Ghi chú**: Reverse Engineering = SKIP (Greenfield project)

---

## 3. Phases to Execute

### 🔵 INCEPTION PHASE

- [x] Workspace Detection — COMPLETED
- [x] Reverse Engineering — SKIPPED (Greenfield)
- [x] Requirements Analysis — COMPLETED
- [x] User Stories — COMPLETED
- [x] Workflow Planning — IN PROGRESS

- [ ] **Application Design — EXECUTE**
  - Rationale: Dự án greenfield phức tạp với nhiều components mới (PWA frontend, .NET backend, Admin dashboard, SignalR hubs, Agora integration). Cần xác định rõ component boundaries, service layer, và API contracts trước khi code.

- [ ] **Units Generation — EXECUTE**
  - Rationale: Hệ thống có nhiều domain độc lập (Auth, Livestream, Payment, Moderation, Admin) có thể phát triển song song. Cần phân chia thành units of work rõ ràng để quản lý và track tiến độ.

### 🟢 CONSTRUCTION PHASE (per unit)

- [ ] **Functional Design — EXECUTE**
  - Rationale: Nhiều business logic phức tạp cần thiết kế chi tiết: coin billing theo phút, leaderboard ranking algorithm, content moderation workflow, payment reconciliation.

- [ ] **NFR Requirements — EXECUTE**
  - Rationale: Có nhiều NFR quan trọng: real-time latency <300ms (Agora), API <200ms, 10K concurrent users, APPI compliance, security (15 SECURITY rules đã bật), SignalR scale-out.

- [ ] **NFR Design — EXECUTE**
  - Rationale: NFR Requirements đã xác định → cần thiết kế patterns cụ thể: Redis backplane cho SignalR, rate limiting, JWT rotation, encryption at rest/transit, structured logging.

- [ ] **Infrastructure Design — EXECUTE**
  - Rationale: AWS infrastructure phức tạp: ECS Fargate, RDS PostgreSQL, ElastiCache Redis, S3/CloudFront, SES, SNS, Rekognition. Cần mapping rõ ràng trước khi code.

- [ ] **Code Generation — EXECUTE** (ALWAYS)
  - Rationale: Implementation của tất cả units.

- [ ] **Build and Test — EXECUTE** (ALWAYS)
  - Rationale: Build instructions, unit tests, integration tests, performance tests.

### 🟡 OPERATIONS PHASE

- [ ] Operations — PLACEHOLDER (future)

---

## 4. Đề Xuất Units of Work

Dựa trên domain analysis, đề xuất chia thành **5 units** phát triển theo thứ tự:

| # | Unit | Mô tả | Dependencies |
|---|---|---|---|
| 1 | **Core Foundation** | Auth, User Profile, Database schema, API base, SignalR setup | None |
| 2 | **Livestream Engine** | Public livestream, Private call, Agora integration, Real-time chat | Unit 1 |
| 3 | **Coin & Payment** | Coin system, Stripe, LINE Pay, Virtual gifts, Transaction history | Unit 1 |
| 4 | **Social & Discovery** | Matching algorithm, Leaderboard, Notifications, Chat 1-1 | Unit 1, 2, 3 |
| 5 | **Admin & Moderation** | Admin dashboard, Content moderation, AI filter, User management | Unit 1, 2, 3, 4 |

---

## 5. Chiến Lược Mock Services cho Third-party

Mục tiêu: cho phép team phát triển và test **độc lập với third-party** ngay từ đầu, không cần hợp đồng hay API key thật.

### 5.1 Tổng Quan Phương Án

| Third-party | Môi trường Dev/Test | Môi trường Staging | Production |
|---|---|---|---|
| **Stripe** | Mock Server tự xây (~4 man-days) + Test Mode fallback | Stripe Test Mode | Stripe Live |
| **LINE Pay** | Mock Service tự xây | LINE Pay Sandbox (cần đăng ký) | LINE Pay Live |
| **Agora.io** | Agora Free Tier (10K phút/tháng miễn phí) | Agora Free/Paid | Agora Paid |
| **AWS Rekognition** | LocalStack (giả lập AWS local) | AWS Dev account | AWS Production |
| **AWS SES/SNS** | LocalStack + MailHog | AWS Dev account | AWS Production |
| **AWS S3/CloudFront** | LocalStack + MinIO | AWS Dev account | AWS Production |

---

### 5.2 Chi Tiết Từng Third-party

#### Stripe — Mock Server + Test Mode (Song Song)

**Effort Analysis**: Xây Stripe Mock Server ước tính ~4 man-days < ngưỡng 5 man-days → **quyết định xây**.

**Lợi ích so với chỉ dùng Test Mode:**
- Test hoàn toàn offline, không phụ thuộc internet
- Kiểm soát 100% response/error scenarios mà không cần Stripe CLI
- CI/CD pipeline không cần Stripe account hay API key

**Stripe Mock Server (ASP.NET Core) — scope ~4 man-days:**

```csharp
// MockStripeController.cs
[ApiController]
[Route("mock/stripe/v1")]
public class MockStripeController : ControllerBase
{
    private static readonly Dictionary<string, object> _intents = new();

    // POST /mock/stripe/v1/payment_intents
    [HttpPost("payment_intents")]
    public IActionResult CreatePaymentIntent([FromForm] CreatePaymentIntentDto dto)
    {
        var id = $"pi_mock_{Guid.NewGuid():N}";
        var intent = new {
            id, object_type = "payment_intent",
            amount = dto.Amount, currency = dto.Currency,
            status = "requires_payment_method",
            client_secret = $"{id}_secret_mock"
        };
        _intents[id] = intent;
        return Ok(intent);
    }

    // POST /mock/stripe/v1/payment_intents/:id/confirm
    [HttpPost("payment_intents/{id}/confirm")]
    public IActionResult ConfirmPayment(string id, [FromForm] ConfirmPaymentDto dto)
    {
        // Test card: 4242... = success, 4000...0002 = decline
        var scenario = Environment.GetEnvironmentVariable("MOCK_STRIPE_SCENARIO") ?? "success";
        if (scenario == "decline" || dto.PaymentMethod == "pm_card_declined")
            return BadRequest(new { error = new { code = "card_declined", message = "Your card was declined." } });

        return Ok(new { id, status = "succeeded" });
    }

    // POST /mock/stripe/v1/refunds
    [HttpPost("refunds")]
    public IActionResult CreateRefund([FromForm] CreateRefundDto dto) =>
        Ok(new { id = $"re_mock_{Guid.NewGuid():N}", status = "succeeded", amount = dto.Amount });

    // POST /mock/stripe/v1/webhooks/trigger — kích hoạt webhook event thủ công
    [HttpPost("webhooks/trigger")]
    public IActionResult TriggerWebhook([FromBody] TriggerWebhookDto dto,
        [FromServices] IWebhookDispatcher dispatcher)
    {
        dispatcher.Dispatch(dto.EventType, dto.PaymentIntentId);
        return Ok(new { triggered = dto.EventType });
    }
}
```

**Cấu hình environment:**
```bash
# .env.development — dùng mock
STRIPE_BASE_URL=http://localhost:5001/mock/stripe
STRIPE_SECRET_KEY=sk_mock_any_value
MOCK_STRIPE_SCENARIO=success   # success | decline | timeout | network_error

# .env.test — dùng Stripe Test Mode thật (fallback)
STRIPE_BASE_URL=https://api.stripe.com
STRIPE_SECRET_KEY=sk_test_xxxxx
```

**Kịch bản test được hỗ trợ:**

| Scenario | Cách kích hoạt |
|---|---|
| Thanh toán thành công | `MOCK_STRIPE_SCENARIO=success` |
| Thẻ bị từ chối | `MOCK_STRIPE_SCENARIO=decline` |
| Network timeout | `MOCK_STRIPE_SCENARIO=timeout` |
| Webhook payment_intent.succeeded | `POST /mock/stripe/v1/webhooks/trigger` |
| Webhook payment_intent.payment_failed | `POST /mock/stripe/v1/webhooks/trigger` |

**Effort breakdown:**
- Implement endpoints (create, confirm, refund): 1 ngày
- Webhook simulation + dispatcher: 1 ngày
- Error scenarios + idempotency: 1 ngày
- Unit tests cho mock server: 0.5 ngày
- Integration + documentation: 0.5 ngày
- **Tổng: ~4 man-days ✅ (< 5 man-days)**

---

#### LINE Pay — Tự xây Mock Service

LINE Pay Sandbox yêu cầu đăng ký doanh nghiệp Nhật Bản → **không khả dụng sớm**. Giải pháp: tự xây mock.

**Cách triển khai — LINE Pay Mock Server (ASP.NET Core):**

```csharp
// MockLinePayController.cs — giả lập LINE Pay API
[ApiController]
[Route("mock/linepay")]
public class MockLinePayController : ControllerBase
{
    // POST /mock/linepay/v3/payments/request
    [HttpPost("v3/payments/request")]
    public IActionResult RequestPayment([FromBody] LinePayRequestDto dto)
    {
        var transactionId = $"MOCK_{Guid.NewGuid():N}";
        return Ok(new {
            returnCode = "0000",
            returnMessage = "Success",
            info = new {
                transactionId,
                paymentUrl = new {
                    web = $"http://localhost:5001/mock/linepay/confirm-ui?txId={transactionId}",
                    app = $"linepay://mock?txId={transactionId}"
                }
            }
        });
    }

    // POST /mock/linepay/v3/payments/{transactionId}/confirm
    [HttpPost("v3/payments/{transactionId}/confirm")]
    public IActionResult ConfirmPayment(string transactionId)
    {
        // Simulate success (có thể config để test failure)
        return Ok(new {
            returnCode = "0000",
            returnMessage = "Success",
            info = new { transactionId, orderId = $"ORDER_{transactionId}" }
        });
    }
}
```

**Cấu hình qua environment variable:**
```bash
# .env.development
LINE_PAY_BASE_URL=http://localhost:5001/mock/linepay   # Mock
# LINE_PAY_BASE_URL=https://sandbox-api-pay.line.me    # Sandbox (khi có)
# LINE_PAY_BASE_URL=https://api-pay.line.me            # Production
```

**Kịch bản test có thể cấu hình:**
- `MOCK_LINEPAY_SCENARIO=success` — thanh toán thành công
- `MOCK_LINEPAY_SCENARIO=failure` — thanh toán thất bại
- `MOCK_LINEPAY_SCENARIO=timeout` — giả lập timeout

---

#### Agora.io — Dùng Free Tier (Không cần hợp đồng)

Agora cung cấp **Free Tier 10,000 phút/tháng** — đủ cho dev và test, không cần hợp đồng.

**Cách triển khai:**
- Đăng ký tài khoản Agora miễn phí tại agora.io → lấy `AGORA_APP_ID` và `AGORA_APP_CERTIFICATE`
- Tạo token server-side (ASP.NET Core) để generate RTC tokens
- Free tier hoạt động y hệt paid — không cần mock

**Nếu muốn test offline hoàn toàn (không cần internet):**
```csharp
// MockAgoraService.cs — giả lập Agora token generation
public class MockAgoraTokenService : IAgoraTokenService
{
    public string GenerateRtcToken(string channelName, string uid, int expireSeconds)
    {
        // Trả về fake token — Agora SDK sẽ fail khi connect
        // nhưng đủ để test luồng UI và API logic
        return $"MOCK_TOKEN_{channelName}_{uid}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
    }
}
```

**Khuyến nghị**: Dùng Agora Free Tier thật — đơn giản hơn mock và test được end-to-end video.

---

#### AWS Services — Dùng LocalStack

**LocalStack** giả lập toàn bộ AWS services locally — không cần AWS account, không tốn phí.

**Services cần mock:** S3, SES, SNS, SQS, Rekognition, ElastiCache (Redis dùng thật local)

**docker-compose.yml (thêm vào):**
```yaml
services:
  localstack:
    image: localstack/localstack:latest
    ports:
      - "4566:4566"
    environment:
      - SERVICES=s3,ses,sns,sqs,rekognition
      - DEFAULT_REGION=ap-northeast-1
      - AWS_DEFAULT_REGION=ap-northeast-1
    volumes:
      - ./localstack-init:/etc/localstack/init/ready.d  # auto-create buckets/topics

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_DB: livestream_dev
      POSTGRES_USER: dev
      POSTGRES_PASSWORD: dev123
    ports:
      - "5432:5432"
```

**localstack-init/init.sh:**
```bash
#!/bin/bash
# Tự động tạo resources khi LocalStack khởi động
awslocal s3 mb s3://livestream-media-dev
awslocal sns create-topic --name push-notifications-dev
awslocal sqs create-queue --queue-name background-jobs-dev
```

**Cấu hình .NET để dùng LocalStack:**
```csharp
// Program.cs — switch giữa LocalStack và AWS thật
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
        new AmazonS3Config { ServiceURL = "http://localhost:4566", ForcePathStyle = true }
    ));
    // tương tự cho SES, SNS, SQS, Rekognition
}
else
{
    builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
    builder.Services.AddAWSService<IAmazonS3>();
}
```

**Rekognition Mock cho Content Moderation:**
```csharp
// MockRekognitionService.cs — dùng khi LocalStack Rekognition chưa đủ
public class MockRekognitionService : IContentModerationService
{
    public Task<ModerationResult> AnalyzeFrameAsync(byte[] imageBytes)
    {
        // Luôn trả về "safe" trong dev — hoặc config để test violation
        return Task.FromResult(new ModerationResult { IsSafe = true, Confidence = 99.9f });
    }
}
```

---

### 5.3 Tóm Tắt — Thứ Tự Ưu Tiên Setup

| Bước | Action | Thời gian | Chi phí |
|---|---|---|---|
| 1 | Đăng ký Agora account (Free Tier) | 15 phút | Miễn phí |
| 2 | Setup LocalStack qua Docker Compose | 30 phút | Miễn phí |
| 3 | Xây LINE Pay Mock Server | 2-4 giờ | Miễn phí |
| 4 | Xây Stripe Mock Server (effort ~4 man-days ✅) | 4 ngày | Miễn phí |
| 5 | Đăng ký Stripe Test Mode (fallback khi cần) | 15 phút | Miễn phí |
| 6 | Đăng ký LINE Pay Sandbox (khi sẵn sàng) | 1-2 tuần (approval) | Miễn phí |
| 7 | Chuyển sang AWS account thật (staging) | Khi cần | Trả phí |

**Kết quả**: Team có thể test **100% tính năng** từ ngày đầu mà không cần hợp đồng hay chi phí.

---

### 5.4 Tích Hợp vào Units

| Unit | Mock cần thiết |
|---|---|
| Unit 1: Core Foundation | LocalStack (S3 cho ảnh profile, SES cho email OTP) |
| Unit 2: Livestream Engine | Agora Free Tier (thật), LocalStack SNS (push notification) |
| Unit 3: Coin & Payment | Stripe Mock Server (tự xây), LINE Pay Mock Server, Redis local |
| Unit 4: Social & Discovery | LocalStack SNS, Redis local |
| Unit 5: Admin & Moderation | LocalStack Rekognition Mock, LocalStack SQS |

---

## 6. Estimated Timeline

| Phase | Ước tính |
|---|---|
| Application Design | 1 session |
| Units Generation | 1 session |
| Construction per Unit (x5) | 2-3 sessions/unit |
| Build and Test | 1 session |
| **Tổng** | **~15-18 sessions** |

---

## 6. Success Criteria

- **Primary Goal**: PWA livestream hẹn hò production-ready cho thị trường Nhật Bản
- **Key Deliverables**: Source code đầy đủ, IaC (AWS CDK/Terraform), test suite, build instructions
- **Quality Gates**: Tất cả 15 SECURITY rules compliant, unit test coverage ≥80%, API latency <200ms
