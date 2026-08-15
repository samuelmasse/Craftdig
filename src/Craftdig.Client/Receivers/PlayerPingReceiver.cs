namespace Craftdig;

[Player]
public class PlayerPingReceiver(PlayerSocket socket)
{
    public void Receive(PingCommand cmd) =>
        socket.Send(new PongCommand() { Ping = cmd });
}
