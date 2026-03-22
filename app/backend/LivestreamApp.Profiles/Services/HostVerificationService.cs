using LivestreamApp.Profiles.Domain.Entities;
using LivestreamApp.Profiles.Repositories;
using LivestreamApp.Shared.Exceptions;
using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Profiles.Services;

public class HostVerificationService : IHostVerificationService
{
    private readonly IHostProfileRepository _hostProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public HostVerificationService(IHostProfileRepository hostProfileRepository, IUnitOfWork unitOfWork)
    {
        _hostProfileRepository = hostProfileRepository;
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc/>
    public async Task<HostProfile> RequestVerificationAsync(Guid userId, CancellationToken ct = default)
    {
        var hostProfile = await _hostProfileRepository.GetByUserIdAsync(userId, ct);

        if (hostProfile is null)
        {
            hostProfile = HostProfile.Create(userId);
            await _hostProfileRepository.AddAsync(hostProfile, ct);
        }

        hostProfile.RequestVerification();
        await _unitOfWork.SaveChangesAsync(ct);

        return hostProfile;
    }

    /// <inheritdoc/>
    public async Task<HostProfile> ApproveVerificationAsync(Guid userId, Guid adminId, CancellationToken ct = default)
    {
        var hostProfile = await _hostProfileRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(nameof(HostProfile), userId);

        hostProfile.Approve(adminId);
        await _unitOfWork.SaveChangesAsync(ct);

        return hostProfile;
    }

    /// <inheritdoc/>
    public async Task<HostProfile> RejectVerificationAsync(Guid userId, Guid adminId, string? note, CancellationToken ct = default)
    {
        var hostProfile = await _hostProfileRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(nameof(HostProfile), userId);

        hostProfile.Reject(adminId, note);
        await _unitOfWork.SaveChangesAsync(ct);

        return hostProfile;
    }
}
