namespace Crafthoe.Client;

[Player]
public class PlayerPingReceiver(PlayerSocket socket)
{
    public void Receive(PingCommand cmd) =>
        socket.Send((int)CommonCommand.Pong, new PongCommand() { Ping = cmd });
}
