namespace Crafthoe.Client;

[Player]
public class PlayerPingReceiver(PlayerSocket socket)
{
    public void Receive(NetSocket ns, NetMessage msg)
    {
        var ping = MemoryMarshal.AsRef<PingCommand>(msg.Data);
        var pong = new PongCommand() { Ping = ping };

        socket.Send(new((int)CommonCommand.Pong,
            MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref pong, 1))));
    }
}
