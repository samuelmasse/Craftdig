namespace Craftdig;

public sealed class ServerPresenceConnection(NetSocket socket, long generation, SessionId sessionId)
{
    private int canceled;

    public readonly NetSocket Socket = socket;
    public readonly long Generation = generation;
    public readonly SessionId SessionId = sessionId;

    public bool IsCanceled => Volatile.Read(ref canceled) != 0;

    public void Cancel() => Interlocked.Exchange(ref canceled, 1);
}
