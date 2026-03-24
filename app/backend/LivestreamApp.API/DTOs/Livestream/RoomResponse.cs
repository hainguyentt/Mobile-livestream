using LivestreamApp.Livestream.Domain.Entities;

namespace LivestreamApp.API.DTOs.Livestream;

public sealed record RoomResponse(
    Guid Id,
    Guid HostId,
    string Title,
    string Category,
    string Status,
    string AgoraChannelName,
    int ViewerCount,
    int PeakViewerCount,
    DateTime? StartedAt,
    DateTime CreatedAt)
{
    public static RoomResponse From(LivestreamRoom room) => new(
        room.Id,
        room.HostId,
        room.Title,
        room.Category.ToString(),
        room.Status.ToString(),
        room.AgoraChannelName,
        room.ViewerCount,
        room.PeakViewerCount,
        room.StartedAt,
        room.CreatedAt);
}
