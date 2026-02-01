namespace Craftdig.Menus.Multiplayer;

[Player]
public class PlayerMultiplayerSpawnAction(
    AppClientOptions clientOptions,
    PlayerSocket socket)
{
    public void Run()
    {
        if (clientOptions.NoAuthUser != null)
        {
            socket.Send<NoAuthCommand, byte>(Encoding.UTF8.GetBytes(clientOptions.NoAuthUser));
            socket.Send<SpawnPlayerCommand>();
        }
        else socket.Send<BeginAuthCommand>();
    }
}
