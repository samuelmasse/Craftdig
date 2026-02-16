namespace Craftdig.Server;

[Server]
public class ServerAuthReceiver(ServerAuth auth)
{
    public void BeginAuth(NetSocket ns)
    {
        lock (auth)
        {
            auth.SendNonce(ns);
        }
    }

    public void CompleteAuth(NetSocket ns, ReadOnlySpan<byte> data)
    {
        lock (auth)
        {
            auth.AuthenticateJwt(ns, data);
        }
    }

    public void NoAuth(NetSocket ns, ReadOnlySpan<byte> data)
    {
        lock (auth)
        {
            auth.AuthenticateNoAuth(ns, data);
        }
    }
}
