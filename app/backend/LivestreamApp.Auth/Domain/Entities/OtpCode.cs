using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Domain.Primitives;

namespace LivestreamApp.Auth.Domain.Entities;

public sealed class OtpCode : Entity<Guid>
{
    public string Target { get; private set; }   // email or phone number
    public string CodeHash { get; private set; }
    public OtpPurpose Purpose { get; private set; }
    public int AttemptCount { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private OtpCode() : base(Guid.Empty) { Target = null!; CodeHash = null!; }

    public static OtpCode Create(string target, string codeHash, OtpPurpose purpose, int expiryMinutes = 10)
    {
        return new OtpCode
        {
            Id = Guid.NewGuid(),
            Target = target,
            CodeHash = codeHash,
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes),
            CreatedAt = DateTime.UtcNow
        };
    }

    private new Guid Id { get; set; }

    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid() => !IsUsed && !IsExpired() && AttemptCount < 3;

    public void RecordAttempt()
    {
        AttemptCount++;
        if (AttemptCount >= 3)
            IsUsed = true; // invalidate after 3 failed attempts
    }

    public void MarkUsed() => IsUsed = true;
}
