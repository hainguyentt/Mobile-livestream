---
inclusion: fileMatch
fileMatchPattern: "app/backend/**,app/tests/**"
---

# Data Access Standards — EF Core & Redis

## EF Core

### 1. Dual DbContext — inject đúng context

- `AppDbContext` — chỉ dùng cho INSERT / UPDATE / DELETE
- `ReadOnlyDbContext` — dùng cho tất cả SELECT / query
- `ReadOnlyDbContext` phải được register với `UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)`
- Không inject `AppDbContext` vào query-only services

```csharp
// Write service
public class AuthService(AppDbContext db, IUnitOfWork uow) { ... }

// Query service
public class ProfileQueryService(ReadOnlyDbContext readDb) { ... }
```

### 2. Phòng ngừa N+1

Không lazy load. Luôn dùng explicit eager loading.

```csharp
// WRONG — N+1
var users = await db.Users.ToListAsync();
foreach (var u in users)
    u.Profile = await db.Profiles.FindAsync(u.Id);

// CORRECT
var users = await db.Users
    .Include(u => u.Profile)
    .Include(u => u.Photos.Where(p => p.DisplayIndex == 0))
    .AsSplitQuery()   // bắt buộc khi Include >= 2 collections
    .ToListAsync();
```

- Không dùng `virtual` navigation properties — tắt lazy loading hoàn toàn
- Không expose `IQueryable<T>` ra ngoài repository layer
- Dùng `.AsSplitQuery()` khi Include từ 2 collections trở lên

### 3. Khi nào dùng Dapper

| Query Type | Tool |
|---|---|
| CRUD đơn giản, domain operations | EF Core |
| JOIN ≤ 3 tables | EF Core |
| JOIN > 3 tables, aggregation, reporting | Dapper + `ReadOnlyDbContext` connection |
| Full-text search | PostgreSQL `tsvector` + EF Core |

```csharp
// Dapper cho complex queries — luôn dùng parameterized queries
public Task<IEnumerable<TopHostDto>> GetTopHostsAsync(DateRange range) =>
    _readConn.QueryAsync<TopHostDto>("""
        SELECT u.id, up.display_name, SUM(ct.amount) as total_coins
        FROM coin_transactions ct
        JOIN users u ON ct.recipient_id = u.id
        JOIN user_profiles up ON u.id = up.user_id
        WHERE ct.created_at BETWEEN @Start AND @End
        GROUP BY u.id, up.display_name
        ORDER BY total_coins DESC LIMIT 50
        """, new { range.Start, range.End });
```

### 4. Slow Query Logging

Threshold cấu hình trong `appsettings.json` — không hardcode.

```json
"EFCore": { "SlowQueryThresholdMs": 500 }
```

```csharp
options.UseNpgsql(connectionString)
       .LogTo(
           (eventId, _) => eventId == RelationalEventId.CommandExecuted,
           eventData =>
           {
               if (eventData is CommandExecutedEventData cmd &&
                   cmd.Duration.TotalMilliseconds > slowQueryThresholdMs)
                   logger.LogWarning("Slow query ({Duration}ms): {Sql}",
                       cmd.Duration.TotalMilliseconds, cmd.Command.CommandText);
           });
```

### 5. Optimistic Concurrency

Dùng cho tất cả entities có shared mutable state (coin balance, seat count).

```csharp
// Entity
public uint RowVersion { get; set; }  // maps to PostgreSQL xmin

// EF Core config
builder.Property(e => e.RowVersion)
       .HasColumnName("xmin")
       .HasColumnType("xid")
       .ValueGeneratedOnAddOrUpdate()
       .IsConcurrencyToken();
```

Caller phải handle `DbUpdateConcurrencyException` — không swallow silently:

```csharp
catch (DbUpdateConcurrencyException)
{
    throw new DomainException("Concurrent update detected. Please retry.");
}
```

### 6. Entity Configuration

- Mỗi entity có file `{EntityName}Configuration.cs` riêng implement `IEntityTypeConfiguration<T>`
- Không dùng Data Annotations — chỉ dùng Fluent API
- Tất cả `string` columns phải có `HasMaxLength()` — không để unbounded
- Tất cả `DateTime` columns phải convert về UTC
- Foreign keys phải có explicit `DeleteBehavior`

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.CreatedAt)
               .HasConversion(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasMany(u => u.RefreshTokens)
               .WithOne()
               .HasForeignKey(t => t.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### 7. Migrations

- Không sửa migration file đã commit — tạo migration mới để fix
- Không xóa migration file đã apply vào production
- Review generated SQL trước khi commit: `dotnet ef migrations script`
- Naming: `dotnet ef migrations add {PascalCaseDescription}` (ví dụ: `AddUserPhoneVerified`)

### 8. Append-Only Repositories

Các tables không được UPDATE/DELETE (`AdminActionLog`, `LoginAttempt`, `CoinTransaction`) chỉ expose `AppendAsync()`:

```csharp
public interface IAdminActionLogRepository
{
    Task AppendAsync(AdminActionLog log, CancellationToken ct = default);
    // No Update, no Delete
}
```

### 9. Unit of Work

- `IUnitOfWork` chịu trách nhiệm gọi `SaveChangesAsync` — service không gọi trực tiếp trên `AppDbContext`
- Một `SaveChangesAsync` duy nhất per business operation — không gọi nhiều lần trong cùng một flow
- Chỉ inject `IUnitOfWork` vào write services; read services dùng `ReadOnlyDbContext` không cần UoW

```csharp
// CORRECT — service dùng UoW
public async Task<UserProfile> CreateProfileAsync(Guid userId, string displayName, CancellationToken ct)
{
    var profile = UserProfile.Create(userId, displayName);
    await _profileRepo.AddAsync(profile, ct);
    await _unitOfWork.SaveChangesAsync(ct);  // một lần duy nhất
    return profile;
}

// WRONG — gọi SaveChanges trực tiếp trên DbContext
await _db.SaveChangesAsync(ct);
```

### 10. Không Dùng

| Pattern | Lý do |
|---|---|
| `virtual` navigation properties | Ẩn N+1, khó debug |
| `ExecuteSqlRaw()` với string interpolation | SQL injection risk |
| `.ToList()` trước khi filter | Load toàn bộ table vào memory |
| `SaveChanges()` (sync) | Blocking |
| `Find()` thay vì `FindAsync()` | Blocking |
| Expose `DbContext` ra ngoài repository | Phá vỡ abstraction |

---

## Redis Cache

### 1. Mandatory TTL

Mọi cache write phải có TTL — không có key vĩnh viễn. `ICacheService.SetAsync()` phải throw nếu TTL null.

```csharp
// CORRECT
await _cache.SetAsync(CacheKeys.UserProfile(userId), profile, TimeSpan.FromMinutes(15));

// WRONG — không có TTL
await _cache.SetAsync(CacheKeys.UserProfile(userId), profile);
```

### 2. Centralized Cache Keys — không dùng magic strings

```csharp
public static class CacheKeys
{
    public static string UserProfile(Guid userId)      => $"user:profile:{userId}";
    public static string RevokedToken(string hash)     => $"revoked_token:{hash}";
    public static string OtpRateLimit(string email)    => $"otp:rate_limit:{email}";
    public static string LoginRateLimit(string ip)     => $"login:rate_limit:{ip}";
}
```

### 3. Write-Invalidate Pattern

Xóa cache sau khi write — không dùng write-through (tránh race condition).

```csharp
// Sau mỗi write operation ảnh hưởng đến profile
await _unitOfWork.SaveChangesAsync(ct);
await _cache.RemoveAsync(CacheKeys.UserProfile(userId));
// Cache populate lại ở lần GET tiếp theo (lazy)
```
