using LivestreamApp.Shared.Domain.Primitives;

namespace LivestreamApp.Auth.Domain.Entities;

public sealed class LoginAttempt : Entity<Guid>
{
    public Guid? UserId { get; private set; }
    public string Email { get; private set; }
    public string IpAddress { get; private set; }
    public bool IsSuccess { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime AttemptedAt { get; private set; }

    private LoginAttempt() : base(Guid.Empty) { Email = null!; IpAddress = null!; }

    public static LoginAttempt Record(string email, string ipAddress, bool isSuccess, Guid? userId = null, string? failureReason = null)
    {
        return new LoginAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = email,
            IpAddress = ipAddress,
            IsSuccess = isSuccess,
            FailureReason = failureReason,
            AttemptedAt = DateTime.UtcNow
        };
    }

    private new Guid Id { get; set; }
}
