using LivestreamApp.Livestream.Domain.Entities;
using LivestreamApp.Livestream.Domain.ValueObjects;
using LivestreamApp.Livestream.Repositories;
using LivestreamApp.Livestream.Services;
using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Exceptions;
using LivestreamApp.Shared.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace LivestreamApp.Tests.Unit.Livestream;

public sealed class PrivateCallServiceTests
{
    private readonly ICallSessionRepository _callRepo = Substitute.For<ICallSessionRepository>();
    private readonly IAgoraTokenService _agora = Substitute.For<IAgoraTokenService>();
    private readonly IFeatureFlagService _featureFlags = Substitute.For<IFeatureFlagService>();
    private readonly IBillingService _billing = Substitute.For<IBillingService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly PrivateCallService _sut;

    public PrivateCallServiceTests()
    {
        _sut = new PrivateCallService(_callRepo, _agora, _featureFlags, _billing, _uow,
            NullLogger<PrivateCallService>.Instance);
    }

    [Fact]
    public async Task RequestCall_WhenFeatureEnabledAndNoPendingRequest_CreatesRequest()
    {
        var viewerId = Guid.NewGuid();
        var hostId = Guid.NewGuid();

        _featureFlags.IsEnabledAsync("private-call").Returns(true);
        _callRepo.GetPendingRequestByHostAsync(hostId).Returns((PrivateCallRequest?)null);
        _billing.CheckSufficientBalanceAsync(viewerId, Arg.Any<int>()).Returns(true);

        var request = await _sut.RequestCallAsync(viewerId, hostId);

        Assert.Equal(viewerId, request.ViewerId);
        Assert.Equal(hostId, request.HostId);
        Assert.Equal(CallRequestStatus.Pending, request.Status);
        await _callRepo.Received(1).AddRequestAsync(Arg.Any<PrivateCallRequest>());
    }

    [Fact]
    public async Task RequestCall_WhenHostHasPendingRequest_ThrowsDomainException()
    {
        var viewerId = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var existingRequest = PrivateCallRequest.Create(Guid.NewGuid(), hostId, 10);

        _featureFlags.IsEnabledAsync("private-call").Returns(true);
        _callRepo.GetPendingRequestByHostAsync(hostId).Returns(existingRequest);

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.RequestCallAsync(viewerId, hostId));
    }

    [Fact]
    public async Task RequestCall_WhenFeatureDisabled_ThrowsDomainException()
    {
        _featureFlags.IsEnabledAsync("private-call").Returns(false);

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.RequestCallAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task AcceptCall_WhenHostAccepts_CreatesSessionAndGeneratesTokens()
    {
        var viewerId = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var request = PrivateCallRequest.Create(viewerId, hostId, 10);

        _callRepo.GetRequestByIdAsync(request.Id).Returns(request);
        _agora.GenerateTokenAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<AgoraRole>())
            .Returns(new AgoraToken("fake-token", "channel", AgoraRole.Publisher, DateTime.UtcNow.AddHours(4)));

        var (session, hostToken, viewerToken) = await _sut.AcceptCallAsync(request.Id, hostId);

        Assert.Equal(CallRequestStatus.Accepted, request.Status);
        Assert.Equal(viewerId, session.ViewerId);
        Assert.Equal(hostId, session.HostId);
        Assert.NotNull(hostToken);
        Assert.NotNull(viewerToken);
    }

    [Fact]
    public async Task RejectCall_WhenHostRejects_SetsRejectedStatus()
    {
        var viewerId = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var request = PrivateCallRequest.Create(viewerId, hostId, 10);

        _callRepo.GetRequestByIdAsync(request.Id).Returns(request);

        await _sut.RejectCallAsync(request.Id, hostId);

        Assert.Equal(CallRequestStatus.Rejected, request.Status);
        await _callRepo.Received(1).UpdateRequestAsync(request);
    }

    [Fact]
    public async Task EndCall_WhenParticipantEnds_EndsSession()
    {
        var viewerId = Guid.NewGuid();
        var hostId = Guid.NewGuid();
        var request = PrivateCallRequest.Create(viewerId, hostId, 10);
        var session = CallSession.Create(request.Id, viewerId, hostId, 10);

        _callRepo.GetByIdAsync(session.Id).Returns(session);

        await _sut.EndCallAsync(session.Id, viewerId, "Viewer");

        Assert.Equal(CallSessionStatus.Ended, session.Status);
        Assert.Equal("Viewer", session.EndedBy);
    }
}
