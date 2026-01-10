namespace Craftdig.Menus.Multiplayer;

[Player]
public class PlayerMultiplayerSpawnAction(
    AppClientOptions clientOptions,
    ModuleMultiplayerCredentials credentials,
    PlayerSocket socket)
{
    public void Run()
    {
        if (clientOptions.NoAuthUser != null)
            socket.Send<NoAuthCommand, byte>(Encoding.UTF8.GetBytes(clientOptions.NoAuthUser));
        else socket.Send<AuthCommand, byte>(Encoding.UTF8.GetBytes(credentials.GetFreshToken()));

        socket.Send<SpawnPlayerCommand>();
    }
}
