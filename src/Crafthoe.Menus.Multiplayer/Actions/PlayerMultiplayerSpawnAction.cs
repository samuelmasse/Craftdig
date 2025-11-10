namespace Crafthoe.Menus.Multiplayer;

[Player]
public class PlayerMultiplayerSpawnAction(PlayerSocket socket)
{
    public void Run()
    {
        socket.Send(new((int)ServerCommand.SpawnPlayer, []));
    }
}
