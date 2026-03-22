using LivestreamApp.Auth.Domain.Entities;
using LivestreamApp.Auth.Repositories;
using LivestreamApp.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.Infrastructure.Repositories;

/// <summary>EF Core implementation of IOtpRepository.</summary>
public class OtpRepository : IOtpRepository
{
    private readonly AppDbContext _context;

    public OtpRepository(AppDbContext context) => _context = context;

    public async Task<OtpCode?> GetActiveAsync(string target, OtpPurpose purpose, CancellationToken ct = default) =>
        await _context.OtpCodes
            .Where(o => o.Target == target && o.Purpose == purpose && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(OtpCode otp, CancellationToken ct = default) =>
        await _context.OtpCodes.AddAsync(otp, ct);

    public async Task InvalidatePreviousAsync(string target, OtpPurpose purpose, CancellationToken ct = default)
    {
        var active = await _context.OtpCodes
            .Where(o => o.Target == target && o.Purpose == purpose && !o.IsUsed)
            .ToListAsync(ct);

        foreach (var otp in active)
            otp.MarkUsed();
    }
}
