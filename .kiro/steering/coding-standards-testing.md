---
inclusion: fileMatch
fileMatchPattern: "app/tests/**,app/frontend/**/__tests__/**"
---

# Testing Standards

## Backend — xUnit + Moq + FluentAssertions

### Test Project Structure
```
app/tests/LivestreamApp.Tests.Unit/
├── Auth/
│   ├── AuthServiceTests.cs
│   └── LineOAuthServiceTests.cs
├── Profiles/
│   ├── ProfileServiceTests.cs
│   └── PhotoServiceTests.cs
└── API/
    ├── AuthControllerTests.cs
    └── ProfilesControllerTests.cs
```

### Test Naming
Format: `{MethodName}_{Scenario}_{ExpectedResult}`

```csharp
// CORRECT
public async Task RegisterWithEmail_DuplicateEmail_ThrowsDomainException()
public async Task LoginWithEmail_InvalidPassword_ThrowsUnauthorized()
public async Task VerifyEmailOtp_Success_ReturnsTrue()

// WRONG
public async Task Test1()
public async Task RegisterTest()
```

### Test Structure — AAA Pattern
Every test must follow Arrange / Act / Assert:

```csharp
[Fact]
public async Task RegisterWithEmail_Success_ReturnsUser()
{
    // Arrange
    _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", default))
        .ReturnsAsync((User?)null);

    // Act
    var result = await _sut.RegisterWithEmailAsync("test@example.com", "Password123!");

    // Assert
    result.Should().NotBeNull();
    result.Email.Value.Should().Be("test@example.com");
}
```

### Mock Setup Rules
- Use `Mock<T>` from Moq for all dependencies
- Create mocks as private fields, initialize in constructor
- Name the system under test `_sut`
- Use `It.IsAny<T>()` only when the specific value doesn't matter for the test
- Verify interactions with `Verify(...)` only when the call itself is the behavior being tested

### FluentAssertions Usage
```csharp
// Prefer FluentAssertions over Assert.*
result.Should().NotBeNull();
result.Status.Should().Be(UserStatus.Active);
list.Should().HaveCount(3);
act.Should().ThrowAsync<DomainException>().WithMessage("*already exists*");

// NOT
Assert.NotNull(result);
Assert.Equal(UserStatus.Active, result.Status);
```

### Coverage Requirements
- Minimum **80% line coverage** for service classes
- All happy paths must be tested
- All exception/error paths must be tested
- Edge cases: boundary values, null inputs, empty collections

## Frontend — Jest + React Testing Library

### Test File Location
Co-located with source or in `__tests__/` subfolder:
```
src/__tests__/
├── components/
│   ├── AuthForm.test.tsx
│   └── OtpInput.test.tsx
└── store/
    └── authStore.test.ts
```

### Test Naming
Same format as backend: `{component/function}_{scenario}_{result}`

### Component Testing Rules
- Test **behavior**, not implementation details
- Query by accessible roles/labels, not CSS classes or test IDs (prefer `getByRole`, `getByLabelText`)
- Use `data-testid` only as last resort
- Always `await` async interactions with `waitFor` or `findBy*`

```tsx
// CORRECT — test behavior
it('shows error message when email is invalid', async () => {
  render(<AuthForm mode="login" onSuccess={jest.fn()} />)
  await userEvent.type(screen.getByLabelText('Email'), 'not-an-email')
  await userEvent.click(screen.getByRole('button', { name: /login/i }))
  expect(screen.getByRole('alert')).toBeInTheDocument()
})

// WRONG — testing implementation
it('sets state to error', () => {
  const { result } = renderHook(() => useAuthStore())
  expect(result.current.error).toBe(null)
})
```

### Store Testing
- Test store actions in isolation using `renderHook`
- Mock API calls with `jest.mock`
- Reset store state between tests with `beforeEach`

## What NOT to Test
- Framework internals (Next.js routing, EF Core queries)
- Simple getters/setters with no logic
- Private methods directly — test through public interface
- Third-party library behavior
