namespace Craftdig;

public sealed class ServerPresenceRegistration(
    ServerPresenceConnection connection,
    ServerIdentitySessionSnapshot? identity,
    bool refresh)
{
    private int activated;

    public readonly ServerPresenceConnection Connection = connection;
    public readonly ServerIdentitySessionSnapshot? Identity = identity;
    public readonly bool Refresh = refresh;

    public bool IsActivated => Volatile.Read(ref activated) != 0;

    public void Activate() => Interlocked.Exchange(ref activated, 1);
}
