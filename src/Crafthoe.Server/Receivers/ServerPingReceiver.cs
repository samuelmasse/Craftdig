namespace Crafthoe.Server;

[Server]
public class ServerPingReceiver
{
    public void Receive(NetSocket ns, PingCommand cmd)
    {
        var pong = new PongCommand() { Ping = cmd };

        ns.Send(new((int)CommonCommand.Pong,
            MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref pong, 1))));
    }
}
