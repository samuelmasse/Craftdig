namespace Craftdig.Server;

[Server]
public class ServerAuthReceiver(AppLog log, ServerAuth auth, ServerNoAuth noAuth)
{
    public void BeginAuth(NetSocket ns, ReadOnlySpan<byte> body)
    {
        if (!body.IsEmpty)
        {
            log.Warn("Socket {0} sent a non-empty authentication-begin command", ns.Tag);
            ns.Disconnect();
            return;
        }

        auth.SendNonce(ns);
    }

    public void CompleteAuth(NetSocket ns, ReadOnlySpan<byte> data) => auth.AuthenticateTicket(ns, data);

    public void NoAuth(NetSocket ns, ReadOnlySpan<byte> data) => noAuth.Authenticate(ns, data);
}
