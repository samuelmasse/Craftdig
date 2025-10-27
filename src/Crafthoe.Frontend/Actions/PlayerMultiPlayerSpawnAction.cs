namespace Crafthoe.Frontend;

[Player]
public class PlayerMultiPlayerSpawnAction(NetEcho netEcho, WorldSpawnWrapper spawnWrapper, PlayerSocket socket)
{
    public void Run()
    {
        socket.Send(netEcho.Wrap("Hello this is the client"));
        socket.Send(netEcho.Wrap("Please give me some chunks"));
        socket.Send(spawnWrapper.Wrap());
    }
}
