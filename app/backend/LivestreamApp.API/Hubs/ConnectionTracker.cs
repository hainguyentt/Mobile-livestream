using System.Collections.Concurrent;

namespace LivestreamApp.API.Hubs;

/// <summary>
/// In-memory connection tracker per ECS task.
/// Thread-safe via ConcurrentDictionary. Not persisted — reconnect rebuilds state.
/// </summary>
public sealed class ConnectionTracker : IConnectionTracker
{
    private record ConnectionInfo(Guid UserId, Guid? RoomId);

    private readonly ConcurrentDictionary<string, ConnectionInfo> _byConnection = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentHashSet<string>> _byUser = new();

    public void Add(string connectionId, Guid userId, Guid? roomId = null)
    {
        _byConnection[connectionId] = new ConnectionInfo(userId, roomId);
        _byUser.GetOrAdd(userId, _ => new ConcurrentHashSet<string>()).Add(connectionId);
    }

    public void Remove(string connectionId)
    {
        if (_byConnection.TryRemove(connectionId, out var info))
        {
            if (_byUser.TryGetValue(info.UserId, out var connections))
            {
                connections.Remove(connectionId);
                if (connections.IsEmpty)
                    _byUser.TryRemove(info.UserId, out _);
            }
        }
    }

    public IEnumerable<string> GetConnectionIds(Guid userId)
        => _byUser.TryGetValue(userId, out var connections) ? connections : [];

    public Guid? GetUserId(string connectionId)
        => _byConnection.TryGetValue(connectionId, out var info) ? info.UserId : null;

    public Guid? GetRoomId(string connectionId)
        => _byConnection.TryGetValue(connectionId, out var info) ? info.RoomId : null;

    public int GetConnectionCount() => _byConnection.Count;

    public IEnumerable<Guid> GetActiveRoomIds()
        => _byConnection.Values
            .Where(c => c.RoomId.HasValue)
            .Select(c => c.RoomId!.Value)
            .Distinct();
}

/// <summary>Thread-safe HashSet wrapper.</summary>
internal sealed class ConcurrentHashSet<T> : IEnumerable<T> where T : notnull
{
    private readonly ConcurrentDictionary<T, byte> _dict = new();
    public bool Add(T item) => _dict.TryAdd(item, 0);
    public bool Remove(T item) => _dict.TryRemove(item, out _);
    public bool IsEmpty => _dict.IsEmpty;
    public IEnumerator<T> GetEnumerator() => _dict.Keys.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
