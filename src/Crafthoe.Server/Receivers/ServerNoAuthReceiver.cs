namespace Crafthoe.Server;

[Server]
public class ServerNoAuthReceiver(ServerSockets sockets)
{
    public void Receive(NetSocket ns, ReadOnlySpan<byte> data)
    {
        if (ns.Ent.IsAuthenticated())
        {
            ns.Disconnect();
            return;
        }

        var uid = Encoding.UTF8.GetString(data);

        sockets.ForEach(ns =>
        {
            if (ns.Ent.AuthenticatedUid() == uid)
                ns.Disconnect();
        });

        ns.Ent.AuthenticatedUid() = uid;
        ns.Ent.AuthenticatedEmail() = uid;
        ns.Ent.IsAuthenticated() = true;
    }
}
