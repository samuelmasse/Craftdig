namespace Crafthoe.Server;

[Server]
public class ServerPingReceiver
{
    public void Receive(NetSocket ns, PingCommand cmd) =>
        ns.Send(new PongCommand() { Ping = cmd });
}
