namespace LivestreamApp.RoomChat.Services;

public interface IChatRateLimitService
{
    /// <summary>
    /// Attempts to acquire a rate limit token for the given user in the given room.
    /// Returns false if the user has exceeded 3 messages/second.
    /// </summary>
    bool TryAcquire(Guid userId, Guid roomId);
}
