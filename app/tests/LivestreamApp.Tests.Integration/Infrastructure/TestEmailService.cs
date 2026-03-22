using LivestreamApp.Auth.Services;
using LivestreamApp.Shared.Domain.Enums;
using System.Collections.Concurrent;

namespace LivestreamApp.Tests.Integration.Infrastructure;

/// <summary>
/// In-memory email service for integration tests.
/// Captures sent OTP codes so tests can retrieve them without brute-forcing hashes.
/// </summary>
public class TestEmailService : IEmailService
{
    // Thread-safe store: email -> list of (purpose, code) tuples
    private readonly ConcurrentDictionary<string, List<(OtpPurpose Purpose, string Code)>> _sent = new();

    public Task SendOtpAsync(string toEmail, string code, OtpPurpose purpose, CancellationToken ct = default)
    {
        _sent.AddOrUpdate(
            toEmail.ToLowerInvariant(),
            _ => [(purpose, code)],
            (_, existing) => { existing.Add((purpose, code)); return existing; });
        return Task.CompletedTask;
    }

    /// <summary>Returns the most recently sent OTP code for the given email and purpose.</summary>
    public string? GetLatestCode(string email, OtpPurpose purpose)
    {
        if (!_sent.TryGetValue(email.ToLowerInvariant(), out var list))
            return null;
        return list.LastOrDefault(x => x.Purpose == purpose).Code;
    }

    /// <summary>Clears all captured codes — call between tests if needed.</summary>
    public void Clear() => _sent.Clear();
}
