using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Domain.ValueObjects;
using LivestreamApp.Shared.Events;
using LivestreamApp.Shared.Exceptions;

namespace LivestreamApp.Auth.Domain.Entities;

public sealed class User : Entity<Guid>
{
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsPhoneVerified { get; private set; }
    public string? PhoneNumber { get; private set; }
    public int FailedLoginCount { get; private set; }
    public DateTime? LockoutUntil { get; private set; }

    /// <summary>True after 5 failed login attempts — persisted to DB.</summary>
    public bool RequiresCaptcha { get; private set; }

    public DateTime? LastLoginAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = [];
    public ICollection<ExternalLogin> ExternalLogins { get; private set; } = [];

    private User(Guid id, Email email) : base(id)
    {
        Email = email;
        Role = UserRole.Viewer;
        Status = UserStatus.PendingVerification;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // EF Core constructor
    private User() : base(Guid.Empty) { Email = null!; }

    public static User Create(string email, string passwordHash)
    {
        var user = new User(Guid.NewGuid(), Email.Create(email))
        {
            PasswordHash = passwordHash
        };
        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, email));
        return user;
    }

    public static User CreateFromExternalLogin(string email)
    {
        var user = new User(Guid.NewGuid(), Email.Create(email))
        {
            IsEmailVerified = true,
            Status = UserStatus.Active
        };
        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, email));
        return user;
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserEmailVerifiedEvent(Id, Email.Value));
    }

    public void VerifyPhone(string phoneNumber)
    {
        PhoneNumber = phoneNumber;
        IsPhoneVerified = true;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new UserPhoneVerifiedEvent(Id, phoneNumber));
    }

    public void RecordFailedLogin()
    {
        FailedLoginCount++;

        // BR-AUTH-02-1: require CAPTCHA after 5 failed attempts
        if (FailedLoginCount >= 5)
            RequiresCaptcha = true;

        // BR-AUTH-02-2: lock account for 24h after 10 failed attempts
        if (FailedLoginCount >= 10)
        {
            LockoutUntil = DateTime.UtcNow.AddHours(24);
            Status = UserStatus.Suspended;
            RaiseDomainEvent(new UserLockedOutEvent(Id, Email.Value, LockoutUntil.Value));
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetFailedLoginCount()
    {
        FailedLoginCount = 0;
        LockoutUntil = null;
        RequiresCaptcha = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Records a successful login — updates LastLoginAt and resets failed count.</summary>
    /// <param name="ipAddress">Caller IP address for audit event.</param>
    public void RecordSuccessfulLogin(string ipAddress)
    {
        LastLoginAt = DateTime.UtcNow;
        ResetFailedLoginCount();
        RaiseDomainEvent(new UserLoggedInEvent(Id, ipAddress, LastLoginAt.Value));
    }

    public bool IsLocked() =>
        LockoutUntil.HasValue && LockoutUntil.Value > DateTime.UtcNow;

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void PromoteToHost()
    {
        Role = UserRole.Host;
        UpdatedAt = DateTime.UtcNow;
    }
}
