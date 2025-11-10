namespace Crafthoe.Client;

[Player]
public class PlayerPingReceiver(PlayerSocket socket)
{
    public void Receive(PingCommand cmd)
    {
        var pong = new PongCommand() { Ping = cmd };

        socket.Send(new((int)CommonCommand.Pong,
            MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref pong, 1))));
    }
}
