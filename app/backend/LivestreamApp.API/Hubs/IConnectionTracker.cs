namespace LivestreamApp.API.Hubs;

public interface IConnectionTracker
{
    void Add(string connectionId, Guid userId, Guid? roomId = null);
    void Remove(string connectionId);
    IEnumerable<string> GetConnectionIds(Guid userId);
    Guid? GetUserId(string connectionId);
    Guid? GetRoomId(string connectionId);
    int GetConnectionCount();
    IEnumerable<Guid> GetActiveRoomIds();
}
