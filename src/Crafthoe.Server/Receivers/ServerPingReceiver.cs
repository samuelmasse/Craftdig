namespace Crafthoe.Server;

[Server]
public class ServerPingReceiver
{
    public void Receive(NetSocket ns, PingCommand cmd)
    {
        var pong = new PongCommand() { Ping = cmd };

        ns.Send((int)CommonCommand.Pong, pong);
    }
}
