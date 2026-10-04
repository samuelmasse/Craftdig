namespace Craftdig;

public class ServerPresenceConnection(NetSocket socket, long generation, SessionId sessionId)
{
    private int canceled;
    private int released;

    public readonly NetSocket Socket = socket;
    public readonly long Generation = generation;
    public readonly SessionId SessionId = sessionId;

    public bool IsCanceled => Volatile.Read(ref canceled) != 0;
    public bool IsReleased => Volatile.Read(ref released) != 0;

    public void Cancel() => Interlocked.Exchange(ref canceled, 1);
    public void Release() => Volatile.Write(ref released, 1);
}
