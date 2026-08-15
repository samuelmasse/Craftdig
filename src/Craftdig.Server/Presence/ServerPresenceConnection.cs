namespace Craftdig;

public sealed class ServerPresenceConnection(NetSocket socket, long generation, SessionId sessionId)
{
    private int cancelled;

    public readonly NetSocket Socket = socket;
    public readonly long Generation = generation;
    public readonly SessionId SessionId = sessionId;

    public bool IsCancelled => Volatile.Read(ref cancelled) != 0;

    public void Cancel() => Interlocked.Exchange(ref cancelled, 1);
}
