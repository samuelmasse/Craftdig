namespace Crafthoe.Server;

[Server]
public class ServerNoAuthReceiver(AppLog log, ServerSockets sockets, ServerClientLimits clientLimits)
{
    public void Receive(NetSocket ns, ReadOnlySpan<byte> data)
    {
        if (ns.Ent.IsAuthenticated())
        {
            log.Warn("Socket {0} tried to re-authenticate", ns.Ent.Tag());
            ns.Disconnect();
            return;
        }

        var uid = Encoding.UTF8.GetString(data);

        sockets.ForEach(ns =>
        {
            if (ns.Ent.AuthenticatedUid() == uid)
                ns.Disconnect();
        });

        log.Info("Socket {0} authenticated : {1}", ns.Ent.Tag(), uid);

        ns.Ent.Tag() = uid;
        ns.Ent.AuthenticatedUid() = uid;
        ns.Ent.AuthenticatedEmail() = uid;
        ns.Ent.IsAuthenticated() = true;
        clientLimits.Pulse();
    }
}
