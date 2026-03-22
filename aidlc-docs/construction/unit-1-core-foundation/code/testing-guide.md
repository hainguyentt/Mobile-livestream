# Testing Guide — Unit 1 Core Foundation

---

## Backend Unit Tests (xUnit)

### Chạy tất cả tests
```bash
dotnet test app/tests/LivestreamApp.Tests.Unit
```

### Chạy với coverage report
```bash
dotnet test app/tests/LivestreamApp.Tests.Unit \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage

# Generate HTML report (cần reportgenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage/**/*.xml -targetdir:coverage/html -reporttypes:Html
```

### Test Coverage Requirements
- Minimum **80% line coverage** cho tất cả service classes
- Tất cả happy paths phải được test
- Tất cả exception/error paths phải được test

### Test Structure

```
app/tests/LivestreamApp.Tests.Unit/
├── Auth/
│   ├── AuthServiceTests.cs         (11 tests)
│   └── LineOAuthServiceTests.cs    (3 tests)
├── Profiles/
│   ├── ProfileServiceTests.cs      (5 tests)
│   ├── PhotoServiceTests.cs        (5 tests)
│   └── HostVerificationServiceTests.cs (3 tests)
├── API/
│   ├── AuthControllerTests.cs      (5 tests)
│   └── ProfilesControllerTests.cs  (4 tests)
└── MockServices/
    ├── StripeMockTests.cs          (4 tests)
    └── LinePayMockTests.cs         (3 tests)
```

**Total**: 46 tests, 0 failures

### Naming Convention
```
{MethodName}_{Scenario}_{ExpectedResult}

// Examples:
RegisterWithEmail_DuplicateEmail_ThrowsDomainException
LoginWithEmail_InvalidPassword_ThrowsUnauthorized
GetProfile_CacheHit_ReturnsFromCache
```

---

## Frontend Unit Tests (Jest + React Testing Library)

### Chạy tests (PWA)
```bash
cd app/frontend/pwa
npm install
npm run test:run    # Single run (no watch mode)
```

### Test files
```
app/frontend/pwa/src/__tests__/
├── components/
│   ├── AuthForm.test.tsx    (6 tests)
│   └── OtpInput.test.tsx    (4 tests)
└── store/
    └── authStore.test.ts    (4 tests)
```

### Testing Principles
- Test **behavior**, không test implementation details
- Query bằng accessible roles/labels (`getByRole`, `getByLabelText`)
- Dùng `data-testid` chỉ khi không có cách khác
- Mock API calls với `jest.mock`

---

## MockServices Testing

MockServices có thể test thủ công qua Swagger UI:
```
http://localhost:5200/swagger
```

Hoặc dùng unit tests:
```bash
dotnet test app/tests/LivestreamApp.Tests.Unit --filter "MockServices"
```

---

## Integration Tests (Future)

Integration tests sẽ được implement ở unit tiếp theo, bao gồm:
- Auth flow end-to-end (register → verify email → login → refresh → logout)
- Profile flow (create → update → upload photo → reorder)
- Host verification flow (request → approve)

Sẽ dùng `WebApplicationFactory<Program>` với test database (PostgreSQL in-memory hoặc Testcontainers).

---

## Performance Tests (Future)

Sẽ dùng k6 hoặc NBomber để test:
- Login endpoint: 100 concurrent users, p95 < 200ms
- Profile GET: 500 concurrent users, p95 < 100ms (cache hit)

---

## Chạy toàn bộ test suite

```bash
# Backend
dotnet test app/tests/LivestreamApp.Tests.Unit

# Frontend PWA
cd app/frontend/pwa && npm run test:run

# Kết quả mong đợi:
# Backend: 46/46 passing
# Frontend: 14/14 passing (khi đã install dependencies)
```
