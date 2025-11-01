namespace Crafthoe.Frontend;

[Player]
public class PlayerMultiPlayerSpawnAction(NetEcho netEcho, PlayerSocket socket)
{
    public void Run()
    {
        socket.Send(new((int)CommonCommand.Echo, netEcho.Wrap("Hello this is the client")));
        socket.Send(new((int)CommonCommand.Echo, netEcho.Wrap("Please give me some chunks")));
        socket.Send(new((int)ServerCommand.Spawn, []));
    }
}
