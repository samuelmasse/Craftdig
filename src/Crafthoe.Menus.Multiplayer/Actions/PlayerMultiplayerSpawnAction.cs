namespace Crafthoe.Menus.Multiplayer;

[Player]
public class PlayerMultiplayerSpawnAction(
    ModuleMultiplayerCredentials credentials,
    PlayerSocket socket)
{
    public void Run()
    {
        socket.Send<AuthCommand, byte>(Encoding.UTF8.GetBytes(credentials.GetFreshToken()));
        socket.Send<SpawnPlayerCommand>();
    }
}
