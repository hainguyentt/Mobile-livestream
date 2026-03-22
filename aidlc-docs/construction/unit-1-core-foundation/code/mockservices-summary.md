# MockServices Module Summary — LivestreamApp.MockServices

**Module**: LivestreamApp.MockServices  
**Phase**: Construction — Unit 1 Core Foundation  
**Ngày hoàn thành**: 2026-03-22

---

## Tổng quan

`LivestreamApp.MockServices` là một ASP.NET Core Web API project dùng để mock các external payment services trong môi trường local development và testing. Không cần tài khoản Stripe hay LINE Pay thật để phát triển.

---

## Controllers

### StripeMockController — `/mock/stripe/v1`

| Method | Endpoint | Mô tả |
|---|---|---|
| POST | `/payment_intents` | Tạo mock payment intent |
| POST | `/payment_intents/{id}/confirm` | Xác nhận payment (simulate success) |
| POST | `/webhooks` | Trigger webhook event thủ công |

**In-memory storage**: Payment intents được lưu trong `Dictionary<string, StripePaymentIntent>` — reset khi restart.

### LinePayMockController — `/mock/linepay/v3/payments`

| Method | Endpoint | Mô tả |
|---|---|---|
| POST | `/request` | Tạo payment request, trả về payment URL |
| POST | `/{transactionId}/confirm` | Xác nhận payment (simulate CAPTURE) |
| GET | `/{transactionId}` | Lấy trạng thái transaction |

**Transaction IDs**: Bắt đầu từ `1000000000`, tăng dần với `Interlocked.Increment`.

---

## Models

### Stripe Models
- `StripePaymentIntent` — id, amount, currency, status, clientSecret, paymentMethod, metadata
- `StripeWebhookEvent` — id, type, data (contains payment intent object)

### LINE Pay Models
- `LinePayRequest` — amount, currency, orderId, packages, redirectUrls
- `LinePayResponse` — returnCode, returnMessage, info (transactionId, paymentUrl, status)
- `LinePayProduct` — id, name, amount, quantity
- `LinePayRedirectUrls` — confirmUrl, cancelUrl
- `LinePayResponseInfo` — transactionId, orderId, paymentUrl, transactionStatus

---

## Unit Tests

| Test Class | Tests | Status |
|---|---|---|
| `StripeMockTests` | 4 tests | ✅ Passing |
| `LinePayMockTests` | 3 tests | ✅ Passing |

**Total**: 7 tests, 0 failures

---

## Cách sử dụng

### Chạy MockServices
```bash
dotnet run --project app/mock/LivestreamApp.MockServices
# Swagger UI: http://localhost:5200/swagger
```

### Cấu hình trong appsettings.Development.json
```json
{
  "Stripe": {
    "BaseUrl": "http://localhost:5200/mock/stripe"
  },
  "LinePay": {
    "BaseUrl": "http://localhost:5200/mock/linepay"
  }
}
```

### Test Stripe flow
```bash
# 1. Tạo payment intent
POST http://localhost:5200/mock/stripe/v1/payment_intents
{ "amount": 1000, "currency": "jpy" }

# 2. Confirm payment
POST http://localhost:5200/mock/stripe/v1/payment_intents/{id}/confirm
{ "paymentMethod": "pm_card_visa" }

# 3. Trigger webhook
POST http://localhost:5200/mock/stripe/v1/webhooks
{ "paymentIntentId": "{id}", "eventType": "payment_intent.succeeded" }
```

### Test LINE Pay flow
```bash
# 1. Request payment
POST http://localhost:5200/mock/linepay/v3/payments/request
{ "amount": 1000, "currency": "JPY", "orderId": "order-001", ... }

# 2. Confirm payment
POST http://localhost:5200/mock/linepay/v3/payments/{transactionId}/confirm
{ "amount": 1000, "currency": "JPY" }
```

---

## Build Status

- Build: ✅ 0 errors, 0 warnings
- Unit Tests: ✅ 7/7 passing
