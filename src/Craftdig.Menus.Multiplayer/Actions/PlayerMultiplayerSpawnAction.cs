namespace Craftdig;

[Player]
public class PlayerMultiplayerSpawnAction(PlayerSocket socket)
{
    public void Run()
    {
        socket.Send<SpawnPlayerCommand>();
    }
}
