namespace LivestreamApp.API.DTOs.DirectChat;

public sealed record GetMessagesQuery
{
    /// <summary>Required — partition safeguard. Defaults to 30 days ago if not provided.</summary>
    public DateTime From { get; init; } = DateTime.UtcNow.AddDays(-30);
    public DateTime? To { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
