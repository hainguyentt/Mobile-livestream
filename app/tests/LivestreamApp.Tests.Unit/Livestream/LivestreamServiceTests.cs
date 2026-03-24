using LivestreamApp.Livestream.Domain.Entities;
using LivestreamApp.Livestream.Repositories;
using LivestreamApp.Livestream.Services;
using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Exceptions;
using LivestreamApp.Shared.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace LivestreamApp.Tests.Unit.Livestream;

public sealed class LivestreamServiceTests
{
    private readonly ILivestreamRoomRepository _rooms = Substitute.For<ILivestreamRoomRepository>();
    private readonly IViewerSessionRepository _sessions = Substitute.For<IViewerSessionRepository>();
    private readonly IViewerCountService _viewerCount = Substitute.For<IViewerCountService>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly LivestreamService _sut;

    public LivestreamServiceTests()
    {
        _sut = new LivestreamService(_rooms, _sessions, _viewerCount, _uow,
            NullLogger<LivestreamService>.Instance);
    }

    [Fact]
    public async Task CreateRoom_WhenHostHasNoActiveRoom_CreatesSuccessfully()
    {
        var hostId = Guid.NewGuid();
        _rooms.GetActiveRoomByHostAsync(hostId).Returns((LivestreamRoom?)null);

        var room = await _sut.CreateRoomAsync(hostId, "Test Room", RoomCategory.Talk);

        Assert.Equal(hostId, room.HostId);
        Assert.Equal("Test Room", room.Title);
        Assert.Equal(RoomStatus.Scheduled, room.Status);
        await _rooms.Received(1).AddAsync(Arg.Any<LivestreamRoom>());
    }

    [Fact]
    public async Task CreateRoom_WhenHostAlreadyHasActiveRoom_ThrowsDomainException()
    {
        var hostId = Guid.NewGuid();
        var existingRoom = LivestreamRoom.Create(hostId, "Existing Room", RoomCategory.Music);
        _rooms.GetActiveRoomByHostAsync(hostId).Returns(existingRoom);

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.CreateRoomAsync(hostId, "New Room", RoomCategory.Talk));
    }

    [Fact]
    public async Task StartStream_WhenHostOwnsRoom_StartsSuccessfully()
    {
        var hostId = Guid.NewGuid();
        var room = LivestreamRoom.Create(hostId, "Test Room", RoomCategory.Talk);
        _rooms.GetByIdAsync(room.Id).Returns(room);

        await _sut.StartStreamAsync(room.Id, hostId);

        Assert.Equal(RoomStatus.Live, room.Status);
        Assert.NotNull(room.StartedAt);
    }

    [Fact]
    public async Task StartStream_WhenNotHost_ThrowsDomainException()
    {
        var hostId = Guid.NewGuid();
        var room = LivestreamRoom.Create(hostId, "Test Room", RoomCategory.Talk);
        _rooms.GetByIdAsync(room.Id).Returns(room);

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.StartStreamAsync(room.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task JoinRoom_WhenRoomLiveAndViewerEligible_CreatesSession()
    {
        var hostId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var room = LivestreamRoom.Create(hostId, "Test Room", RoomCategory.Talk);
        room.StartStream();

        _rooms.GetByIdAsync(room.Id).Returns(room);
        _sessions.IsViewerKickedAsync(room.Id, viewerId).Returns(false);
        _viewerCount.GetCountAsync(room.Id).Returns(0L);
        _sessions.GetActiveSessionAsync(room.Id, viewerId).Returns((ViewerSession?)null);

        var (returnedRoom, session) = await _sut.JoinRoomAsync(room.Id, viewerId);

        Assert.Equal(room.Id, returnedRoom.Id);
        Assert.Equal(viewerId, session.ViewerId);
        await _sessions.Received(1).AddAsync(Arg.Any<ViewerSession>());
        await _viewerCount.Received(1).IncrementAsync(room.Id);
    }

    [Fact]
    public async Task JoinRoom_WhenViewerIsBanned_ThrowsDomainException()
    {
        var hostId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var room = LivestreamRoom.Create(hostId, "Test Room", RoomCategory.Talk);
        room.StartStream();

        _rooms.GetByIdAsync(room.Id).Returns(room);
        _sessions.IsViewerKickedAsync(room.Id, viewerId).Returns(true);

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.JoinRoomAsync(room.Id, viewerId));
    }

    [Fact]
    public async Task JoinRoom_WhenRoomAtCapacity_ThrowsDomainException()
    {
        var hostId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var room = LivestreamRoom.Create(hostId, "Test Room", RoomCategory.Talk);
        room.StartStream();

        _rooms.GetByIdAsync(room.Id).Returns(room);
        _sessions.IsViewerKickedAsync(room.Id, viewerId).Returns(false);
        _viewerCount.GetCountAsync(room.Id).Returns(1000L);

        await Assert.ThrowsAsync<DomainException>(() =>
            _sut.JoinRoomAsync(room.Id, viewerId));
    }

    [Fact]
    public async Task BanViewer_WhenHostBansViewer_KicksAndDecrementsCount()
    {
        var hostId = Guid.NewGuid();
        var viewerId = Guid.NewGuid();
        var room = LivestreamRoom.Create(hostId, "Test Room", RoomCategory.Talk);
        room.StartStream();
        var session = ViewerSession.Create(room.Id, viewerId);

        _rooms.GetByIdAsync(room.Id).Returns(room);
        _sessions.GetActiveSessionAsync(room.Id, viewerId).Returns(session);

        await _sut.BanViewerAsync(room.Id, hostId, viewerId, "Spam");

        Assert.True(session.IsKicked);
        await _viewerCount.Received(1).DecrementAsync(room.Id);
    }
}
