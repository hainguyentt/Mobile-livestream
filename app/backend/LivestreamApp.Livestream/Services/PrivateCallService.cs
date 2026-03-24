using LivestreamApp.Livestream.Domain.Entities;
using LivestreamApp.Livestream.Domain.ValueObjects;
using LivestreamApp.Livestream.Repositories;
using LivestreamApp.Shared.Exceptions;
using LivestreamApp.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace LivestreamApp.Livestream.Services;

public sealed class PrivateCallService : IPrivateCallService
{
    private readonly ICallSessionRepository _callRepo;
    private readonly IAgoraTokenService _agora;
    private readonly IFeatureFlagService _featureFlags;
    private readonly IBillingService _billing;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<PrivateCallService> _logger;

    // Default coin rate per 10-second tick (configurable in future)
    private const int DefaultCoinRatePerTick = 10;

    public PrivateCallService(
        ICallSessionRepository callRepo,
        IAgoraTokenService agora,
        IFeatureFlagService featureFlags,
        IBillingService billing,
        IUnitOfWork uow,
        ILogger<PrivateCallService> logger)
    {
        _callRepo = callRepo;
        _agora = agora;
        _featureFlags = featureFlags;
        _billing = billing;
        _uow = uow;
        _logger = logger;
    }

    public async Task<PrivateCallRequest> RequestCallAsync(Guid viewerId, Guid hostId, CancellationToken ct = default)
    {
        if (!await _featureFlags.IsEnabledAsync("private-call", ct))
            throw new DomainException("Private call is temporarily unavailable.");

        // BR-PC-01: Host can only have 1 pending request
        var pendingRequest = await _callRepo.GetPendingRequestByHostAsync(hostId, ct);
        if (pendingRequest != null)
            throw new DomainException("Host already has a pending call request.");

        // Check viewer has sufficient balance for at least 1 tick
        var hasSufficientBalance = await _billing.CheckSufficientBalanceAsync(viewerId, DefaultCoinRatePerTick, ct);
        if (!hasSufficientBalance)
            throw new DomainException("Insufficient coin balance to start a private call.");

        var request = PrivateCallRequest.Create(viewerId, hostId, DefaultCoinRatePerTick);
        await _callRepo.AddRequestAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Call request {RequestId} created by viewer {ViewerId} for host {HostId}",
            request.Id, viewerId, hostId);
        return request;
    }

    public async Task<(CallSession Session, AgoraToken HostToken, AgoraToken ViewerToken)> AcceptCallAsync(
        Guid requestId, Guid hostId, CancellationToken ct = default)
    {
        var request = await GetRequestOrThrowAsync(requestId, ct);
        if (request.HostId != hostId) throw new DomainException("Only the target host can accept this request.");
        if (request.IsExpired()) throw new DomainException("Call request has expired.");

        request.Accept();

        var session = CallSession.Create(request.Id, request.ViewerId, hostId, request.CoinRatePerTick);
        await _callRepo.AddSessionAsync(session, ct);
        await _callRepo.UpdateRequestAsync(request, ct);
        await _uow.SaveChangesAsync(ct);

        // Generate Agora tokens for both parties
        var hostToken = await _agora.GenerateTokenAsync(session.AgoraChannelName, hostId, AgoraRole.Publisher, ct);
        var viewerToken = await _agora.GenerateTokenAsync(session.AgoraChannelName, request.ViewerId, AgoraRole.Publisher, ct);

        _logger.LogInformation("Call session {SessionId} started for request {RequestId}", session.Id, requestId);
        return (session, hostToken, viewerToken);
    }

    public async Task RejectCallAsync(Guid requestId, Guid hostId, string? reason = null, CancellationToken ct = default)
    {
        var request = await GetRequestOrThrowAsync(requestId, ct);
        if (request.HostId != hostId) throw new DomainException("Only the target host can reject this request.");

        request.Reject(reason);
        await _callRepo.UpdateRequestAsync(request, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task EndCallAsync(Guid sessionId, Guid userId, string endedBy, CancellationToken ct = default)
    {
        var session = await _callRepo.GetByIdAsync(sessionId, ct);
        if (session == null) throw new NotFoundException("CallSession", sessionId);
        if (session.ViewerId != userId && session.HostId != userId)
            throw new DomainException("User is not a participant in this call.");

        session.End(endedBy);
        await _callRepo.UpdateSessionAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

        await _agora.RevokeChannelAsync(session.AgoraChannelName, ct);
        await _billing.FinalizeCallBillingAsync(sessionId, ct);

        _logger.LogInformation("Call session {SessionId} ended by {EndedBy}", sessionId, endedBy);
    }

    public async Task<AgoraToken> GetAgoraTokenAsync(Guid sessionId, Guid userId, CancellationToken ct = default)
    {
        var session = await _callRepo.GetByIdAsync(sessionId, ct);
        if (session == null) throw new NotFoundException("CallSession", sessionId);
        if (session.ViewerId != userId && session.HostId != userId)
            throw new DomainException("User is not a participant in this call.");

        var role = session.HostId == userId ? AgoraRole.Publisher : AgoraRole.Publisher;
        return await _agora.GenerateTokenAsync(session.AgoraChannelName, userId, role, ct);
    }

    public Task<CallSession?> GetCallStatusAsync(Guid sessionId, CancellationToken ct = default)
        => _callRepo.GetByIdAsync(sessionId, ct);

    private async Task<PrivateCallRequest> GetRequestOrThrowAsync(Guid requestId, CancellationToken ct)
    {
        var request = await _callRepo.GetRequestByIdAsync(requestId, ct);
        if (request == null) throw new NotFoundException("PrivateCallRequest", requestId);
        return request;
    }
}
