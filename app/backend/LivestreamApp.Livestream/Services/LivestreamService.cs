using LivestreamApp.Livestream.Domain.Entities;
using LivestreamApp.Livestream.Repositories;
using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Exceptions;
using LivestreamApp.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace LivestreamApp.Livestream.Services;

public sealed class LivestreamService : ILivestreamService
{
    private readonly ILivestreamRoomRepository _rooms;
    private readonly IViewerSessionRepository _sessions;
    private readonly IViewerCountService _viewerCount;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<LivestreamService> _logger;

    public LivestreamService(
        ILivestreamRoomRepository rooms,
        IViewerSessionRepository sessions,
        IViewerCountService viewerCount,
        IUnitOfWork uow,
        ILogger<LivestreamService> logger)
    {
        _rooms = rooms;
        _sessions = sessions;
        _viewerCount = viewerCount;
        _uow = uow;
        _logger = logger;
    }

    public async Task<LivestreamRoom> CreateRoomAsync(Guid hostId, string title, RoomCategory category, CancellationToken ct = default)
    {
        // BR-LS-01: Host can only have 1 active room
        var existing = await _rooms.GetActiveRoomByHostAsync(hostId, ct);
        if (existing != null)
            throw new DomainException("Host already has an active livestream room.");

        var room = LivestreamRoom.Create(hostId, title, category);
        await _rooms.AddAsync(room, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Room {RoomId} created by host {HostId}", room.Id, hostId);
        return room;
    }

    public async Task StartStreamAsync(Guid roomId, Guid hostId, CancellationToken ct = default)
    {
        var room = await GetRoomOrThrowAsync(roomId, ct);
        if (room.HostId != hostId) throw new DomainException("Only the host can start the stream.");

        room.StartStream();
        await _rooms.UpdateAsync(room, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Stream started for room {RoomId}", roomId);
    }

    public async Task EndStreamAsync(Guid roomId, Guid hostId, CancellationToken ct = default)
    {
        var room = await GetRoomOrThrowAsync(roomId, ct);
        if (room.HostId != hostId) throw new DomainException("Only the host can end the stream.");

        room.EndStream();
        await _rooms.UpdateAsync(room, ct);
        await _viewerCount.ResetAsync(roomId, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Stream ended for room {RoomId}", roomId);
    }

    public async Task<(LivestreamRoom Room, ViewerSession Session)> JoinRoomAsync(Guid roomId, Guid viewerId, CancellationToken ct = default)
    {
        var room = await GetRoomOrThrowAsync(roomId, ct);
        if (room.Status != RoomStatus.Live)
            throw new DomainException("Room is not live.");

        // BR-LS-03: Check if viewer is kicked
        var isKicked = await _sessions.IsViewerKickedAsync(roomId, viewerId, ct);
        if (isKicked) throw new DomainException("Viewer is banned from this room.");

        // BR-LS-03: Max 1000 viewers
        var currentCount = await _viewerCount.GetCountAsync(roomId, ct);
        if (currentCount >= 1000) throw new DomainException("Room has reached maximum viewer capacity.");

        // Close any existing active session (reconnect scenario)
        var existingSession = await _sessions.GetActiveSessionAsync(roomId, viewerId, ct);
        if (existingSession != null)
        {
            existingSession.RecordLeave();
            await _sessions.UpdateAsync(existingSession, ct);
        }

        var session = ViewerSession.Create(roomId, viewerId);
        room.IncrementTotalViewers();

        await _sessions.AddAsync(session, ct);
        await _rooms.UpdateAsync(room, ct);
        await _uow.SaveChangesAsync(ct);

        await _viewerCount.IncrementAsync(roomId, ct);

        return (room, session);
    }

    public async Task LeaveRoomAsync(Guid roomId, Guid viewerId, CancellationToken ct = default)
    {
        var session = await _sessions.GetActiveSessionAsync(roomId, viewerId, ct);
        if (session == null) return; // Already left

        session.RecordLeave();
        await _sessions.UpdateAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

        await _viewerCount.DecrementAsync(roomId, ct);
    }

    public async Task<(List<LivestreamRoom> Rooms, int Total)> GetActiveRoomsAsync(
        RoomCategory? category, int page, int pageSize, CancellationToken ct = default)
    {
        var rooms = await _rooms.GetActiveRoomsAsync(category, page, pageSize, ct);
        var total = await _rooms.CountActiveRoomsAsync(category, ct);
        return (rooms, total);
    }

    public Task<LivestreamRoom?> GetRoomByIdAsync(Guid roomId, CancellationToken ct = default)
        => _rooms.GetByIdAsync(roomId, ct);

    public async Task BanViewerAsync(Guid roomId, Guid hostId, Guid viewerId, string? reason, CancellationToken ct = default)
    {
        var room = await GetRoomOrThrowAsync(roomId, ct);
        if (room.HostId != hostId) throw new DomainException("Only the host can ban viewers.");
        if (room.Status != RoomStatus.Live) throw new DomainException("Room is not live.");

        var session = await _sessions.GetActiveSessionAsync(roomId, viewerId, ct);
        if (session != null)
        {
            session.MarkAsKicked();
            await _sessions.UpdateAsync(session, ct);
            await _viewerCount.DecrementAsync(roomId, ct);
        }

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Viewer {ViewerId} banned from room {RoomId} by host {HostId}", viewerId, roomId, hostId);
    }

    public Task<int> GetViewerCountAsync(Guid roomId, CancellationToken ct = default)
        => _viewerCount.GetCountAsync(roomId, ct).ContinueWith(t => (int)t.Result, ct);

    public Task<List<ViewerSession>> GetActiveViewersAsync(Guid roomId, CancellationToken ct = default)
        => Task.FromResult(new List<ViewerSession>()); // Implemented via repository query in controller

    private async Task<LivestreamRoom> GetRoomOrThrowAsync(Guid roomId, CancellationToken ct)
    {
        var room = await _rooms.GetByIdAsync(roomId, ct);
        if (room == null) throw new NotFoundException("LivestreamRoom", roomId);
        return room;
    }
}
