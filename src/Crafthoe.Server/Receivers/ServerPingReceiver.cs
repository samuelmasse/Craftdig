namespace Crafthoe.Server;

[Server]
public class ServerPingReceiver
{
    public void Receive(NetSocket ns, NetMessage msg)
    {
        var ping = MemoryMarshal.AsRef<PingCommand>(msg.Data);
        var pong = new PongCommand() { Ping = ping };

        ns.Send(new((int)CommonCommand.Pong,
            MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref pong, 1))));
    }
}
