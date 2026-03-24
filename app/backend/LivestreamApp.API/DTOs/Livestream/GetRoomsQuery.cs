using LivestreamApp.Shared.Domain.Enums;

namespace LivestreamApp.API.DTOs.Livestream;

public sealed record GetRoomsQuery
{
    public RoomCategory? Category { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed record BanViewerRequest(string? Reason);
